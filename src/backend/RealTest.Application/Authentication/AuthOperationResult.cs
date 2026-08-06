using RealTest.Contracts.Auth;

namespace RealTest.Application.Authentication;

/// <summary>
/// Internal (non-Contracts) result of an operation that issues tokens. Kept out of
/// RealTest.Contracts deliberately: the refresh token must never appear in a JSON response
/// body — the Api layer uses <see cref="RefreshToken"/> only to set an HTTP-only cookie.
/// </summary>
public sealed record AuthOperationResult(
    AuthResponse Response,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
