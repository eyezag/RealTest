using FluentValidation;
using Microsoft.Extensions.Logging;
using RealTest.Application.Common.Exceptions;
using RealTest.Application.Common.Interfaces;
using RealTest.Contracts.Auth;
using RealTest.Contracts.Common;
using RealTest.Domain.Authentication;
using RealTest.Domain.Users;
using AccountStatus = RealTest.Domain.Enums.AccountStatus;

namespace RealTest.Application.Authentication;

public sealed class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly IValidator<ResendVerificationRequest> _resendVerificationValidator;
    private readonly IValidator<VerifyEmailRequest> _verifyEmailValidator;
    private readonly ILogger<AuthService> _logger;

    private const string GenericLoginFailureMessage = "Invalid email or password.";
    private const string GenericInvalidTokenMessage = "This link is invalid or has expired.";

    public AuthService(
        IIdentityService identityService,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailService emailService,
        IDateTimeProvider dateTimeProvider,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator,
        IValidator<ResendVerificationRequest> resendVerificationValidator,
        IValidator<VerifyEmailRequest> verifyEmailValidator,
        ILogger<AuthService> logger)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _emailService = emailService;
        _dateTimeProvider = dateTimeProvider;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _resendVerificationValidator = resendVerificationValidator;
        _verifyEmailValidator = verifyEmailValidator;
        _logger = logger;
    }

    public async Task<AuthOperationResult> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken)
    {
        await ValidateAsync(_registerValidator, request, cancellationToken);

        var existingUser = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException("An account with this email address already exists.");
        }

        var utcNow = _dateTimeProvider.UtcNow;
        var displayName = $"{request.FirstName} {request.LastName}".Trim();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DisplayName = displayName,
            UserType = request.UserType.ToDomain(),
            AccountStatus = AccountStatus.PendingVerification,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        var createResult = await _identityService.CreateUserAsync(user, request.Password, cancellationToken);
        if (!createResult.Succeeded)
        {
            throw new ValidationAppException("password", string.Join(' ', createResult.Errors));
        }

        await _identityService.AddToRoleAsync(user, user.UserType.ToString(), cancellationToken);

        var verificationToken = await _identityService.GenerateEmailVerificationTokenAsync(user, cancellationToken);
        await _emailService.SendEmailVerificationAsync(user.Email!, user.DisplayName, verificationToken, cancellationToken);

        _logger.LogInformation("New account registered for {UserType} role.", user.UserType);

        return await IssueTokensAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthOperationResult> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken)
    {
        await ValidateAsync(_loginValidator, request, cancellationToken);

        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Authentication failure: unknown email address.");
            throw new UnauthorizedAppException(GenericLoginFailureMessage);
        }

        if (await _identityService.IsLockedOutAsync(user, cancellationToken))
        {
            _logger.LogWarning("Authentication blocked: account {UserId} is locked out.", user.Id);
            throw new UnauthorizedAppException("This account is temporarily locked due to repeated failed sign-in attempts. Try again later.");
        }

        if (!user.IsLoginAllowed())
        {
            _logger.LogWarning("Authentication blocked: account {UserId} has status {AccountStatus}.", user.Id, user.AccountStatus);
            throw new ForbiddenAppException("This account is not able to sign in. Contact support for assistance.");
        }

        if (!await _identityService.CheckPasswordAsync(user, request.Password, cancellationToken))
        {
            await _identityService.RecordLoginFailureAsync(user, cancellationToken);
            _logger.LogWarning("Authentication failure: invalid password for account {UserId}.", user.Id);
            throw new UnauthorizedAppException(GenericLoginFailureMessage);
        }

        await _identityService.ResetLoginFailureCountAsync(user, cancellationToken);

        user.LastLoginAt = _dateTimeProvider.UtcNow;
        user.UpdatedAt = _dateTimeProvider.UtcNow;
        await _identityService.UpdateAsync(user, cancellationToken);

        return await IssueTokensAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthOperationResult> RefreshAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAppException("A refresh token is required.");
        }

        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null)
        {
            _logger.LogWarning("Refresh token reuse attempt: unknown token presented.");
            throw new UnauthorizedAppException("Invalid refresh token.");
        }

        var utcNow = _dateTimeProvider.UtcNow;

        if (storedToken.IsRevoked)
        {
            _logger.LogWarning(
                "Refresh token reuse attempt detected for user {UserId}. Revoking all active refresh tokens for this user.",
                storedToken.UserId);

            await _refreshTokenRepository.RevokeAllActiveForUserAsync(storedToken.UserId, utcNow, ipAddress, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAppException("Invalid refresh token.");
        }

        if (storedToken.IsExpired(utcNow))
        {
            throw new UnauthorizedAppException("Refresh token has expired. Please sign in again.");
        }

        var user = await _identityService.FindByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null || !user.IsLoginAllowed())
        {
            storedToken.Revoke(utcNow, ipAddress);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAppException("Invalid refresh token.");
        }

        var newPlainToken = _tokenService.GenerateRefreshTokenValue();
        var newHash = _tokenService.HashRefreshToken(newPlainToken);
        var newToken = RefreshToken.Create(user.Id, newHash, utcNow, utcNow.Add(_tokenService.RefreshTokenLifetime), ipAddress);

        storedToken.Revoke(utcNow, ipAddress, newToken.Id);

        await _refreshTokenRepository.AddAsync(newToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user, await _identityService.GetRolesAsync(user, cancellationToken) as IReadOnlyCollection<string> ?? []);

        return new AuthOperationResult(
            new AuthResponse(accessToken.Token, accessToken.ExpiresAtUtc, user.ToSummary()),
            newPlainToken,
            newToken.ExpiresAtUtc);
    }

    public async Task LogoutAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || storedToken.IsRevoked)
        {
            return;
        }

        storedToken.Revoke(_dateTimeProvider.UtcNow, ipAddress);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserSummaryResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        return user.ToSummary();
    }

    public async Task<MessageResponse> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(_verifyEmailValidator, request, cancellationToken);

        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null || !await _identityService.ConfirmEmailAsync(user, request.Token, cancellationToken))
        {
            throw new ValidationAppException("token", GenericInvalidTokenMessage);
        }

        var utcNow = _dateTimeProvider.UtcNow;
        user.EmailVerifiedAt = utcNow;
        user.UpdatedAt = utcNow;
        if (user.AccountStatus == AccountStatus.PendingVerification)
        {
            user.AccountStatus = AccountStatus.Active;
        }

        await _identityService.UpdateAsync(user, cancellationToken);

        return new MessageResponse("Your email address has been verified.");
    }

    public async Task<MessageResponse> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(_resendVerificationValidator, request, cancellationToken);

        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is not null && user.EmailVerifiedAt is null)
        {
            var token = await _identityService.GenerateEmailVerificationTokenAsync(user, cancellationToken);
            await _emailService.SendEmailVerificationAsync(user.Email!, user.DisplayName, token, cancellationToken);
        }

        // Always return a generic success message so callers cannot use this endpoint to enumerate registered emails.
        return new MessageResponse("If an account exists for that email address, a verification link has been sent.");
    }

    public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(_forgotPasswordValidator, request, cancellationToken);

        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is not null)
        {
            var token = await _identityService.GeneratePasswordResetTokenAsync(user, cancellationToken);
            await _emailService.SendPasswordResetAsync(user.Email!, user.DisplayName, token, cancellationToken);
        }

        // Always return a generic success message so callers cannot use this endpoint to enumerate registered emails.
        return new MessageResponse("If an account exists for that email address, a password reset link has been sent.");
    }

    public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(_resetPasswordValidator, request, cancellationToken);

        var user = await _identityService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            throw new ValidationAppException("token", GenericInvalidTokenMessage);
        }

        var resetResult = await _identityService.ResetPasswordAsync(user, request.Token, request.NewPassword, cancellationToken);
        if (!resetResult.Succeeded)
        {
            throw new ValidationAppException("token", GenericInvalidTokenMessage);
        }

        user.UpdatedAt = _dateTimeProvider.UtcNow;
        await _identityService.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password reset completed for account {UserId}. Revoking all active refresh tokens.", user.Id);
        await _refreshTokenRepository.RevokeAllActiveForUserAsync(user.Id, _dateTimeProvider.UtcNow, revokedByIpAddress: null, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new MessageResponse("Your password has been reset. Please sign in with your new password.");
    }

    private async Task<AuthOperationResult> IssueTokensAsync(ApplicationUser user, string ipAddress, CancellationToken cancellationToken)
    {
        var roles = await _identityService.GetRolesAsync(user, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user, (IReadOnlyCollection<string>)roles);

        var utcNow = _dateTimeProvider.UtcNow;
        var plainRefreshToken = _tokenService.GenerateRefreshTokenValue();
        var refreshTokenHash = _tokenService.HashRefreshToken(plainRefreshToken);
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenHash, utcNow, utcNow.Add(_tokenService.RefreshTokenLifetime), ipAddress);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthOperationResult(
            new AuthResponse(accessToken.Token, accessToken.ExpiresAtUtc, user.ToSummary()),
            plainRefreshToken,
            refreshToken.ExpiresAtUtc);
    }

    private static async Task ValidateAsync<T>(IValidator<T> validator, T instance, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(instance, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => ToCamelCase(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new ValidationAppException(errors);
        }
    }

    private static string ToCamelCase(string propertyName) =>
        string.IsNullOrEmpty(propertyName)
            ? propertyName
            : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
}
