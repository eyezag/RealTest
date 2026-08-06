namespace RealTest.Api.Common;

/// <summary>
/// Centralizes the refresh-token cookie's name and options so every place that sets or
/// clears it agrees on Path/HttpOnly/Secure/SameSite. The refresh token never appears in a
/// JSON response body — only in this cookie, which JavaScript cannot read.
/// </summary>
public static class RefreshTokenCookie
{
    public const string Name = "realtest_refresh_token";

    public static CookieOptions BuildOptions(DateTime expiresAtUtc, bool isDevelopment) => new()
    {
        HttpOnly = true,
        Secure = !isDevelopment,
        SameSite = SameSiteMode.Strict,
        Path = "/api/v1/auth",
        Expires = expiresAtUtc
    };

    public static CookieOptions BuildDeleteOptions(bool isDevelopment) => new()
    {
        HttpOnly = true,
        Secure = !isDevelopment,
        SameSite = SameSiteMode.Strict,
        Path = "/api/v1/auth"
    };
}
