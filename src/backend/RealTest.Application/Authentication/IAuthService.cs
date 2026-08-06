using RealTest.Contracts.Auth;
using RealTest.Contracts.Common;

namespace RealTest.Application.Authentication;

public interface IAuthService
{
    Task<AuthOperationResult> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken);

    Task<AuthOperationResult> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken);

    Task<AuthOperationResult> RefreshAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken);

    Task LogoutAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken);

    Task<UserSummaryResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<MessageResponse> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken);

    Task<MessageResponse> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken);

    Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);

    Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
}
