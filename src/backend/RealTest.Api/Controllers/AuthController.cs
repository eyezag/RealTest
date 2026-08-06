using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTest.Api.Common;
using RealTest.Application.Authentication;
using RealTest.Contracts.Auth;
using RealTest.Contracts.Common;

namespace RealTest.Api.Controllers;

[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController(IAuthService authService, IHostEnvironment environment) : ApiControllerBase
{
    /// <summary>Registers a new Tester or CompanyUser account and signs the caller in.</summary>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, ClientIpAddress, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        return Ok(result.Response);
    }

    /// <summary>Authenticates an existing account and issues a new access/refresh token pair.</summary>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, ClientIpAddress, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        return Ok(result.Response);
    }

    /// <summary>Rotates the refresh token carried in the HTTP-only cookie and issues a new access token.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie.Name] ?? string.Empty;
        var result = await authService.RefreshAsync(refreshToken, ClientIpAddress, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        return Ok(result.Response);
    }

    /// <summary>Revokes the current refresh token and clears the cookie.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie.Name] ?? string.Empty;
        await authService.LogoutAsync(refreshToken, ClientIpAddress, cancellationToken);
        Response.Cookies.Delete(RefreshTokenCookie.Name, RefreshTokenCookie.BuildDeleteOptions(environment.IsDevelopment()));
        return NoContent();
    }

    /// <summary>Returns the authenticated caller's profile summary.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserSummaryResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var user = await authService.GetCurrentUserAsync(CurrentUserId, cancellationToken);
        return Ok(user);
    }

    /// <summary>Confirms an email address using the token issued at registration.</summary>
    [HttpPost("verify-email")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.VerifyEmailAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Re-sends the email verification link. Always returns success to avoid leaking account existence.</summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.ResendVerificationAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Requests a password reset email. Always returns success to avoid leaking account existence.</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Resets a password using a forgot-password token, then revokes all active sessions.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.ResetPasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(
            RefreshTokenCookie.Name,
            refreshToken,
            RefreshTokenCookie.BuildOptions(expiresAtUtc, environment.IsDevelopment()));
    }
}
