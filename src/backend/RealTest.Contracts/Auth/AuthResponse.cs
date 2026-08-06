namespace RealTest.Contracts.Auth;

/// <summary>
/// Response for register/login/refresh. The refresh token itself is never included in the
/// JSON body — it is issued as a secure, HTTP-only cookie so it is unreachable from JavaScript.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    UserSummaryResponse User);
