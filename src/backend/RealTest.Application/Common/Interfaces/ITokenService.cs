using RealTest.Domain.Users;

namespace RealTest.Application.Common.Interfaces;

public interface ITokenService
{
    GeneratedAccessToken GenerateAccessToken(ApplicationUser user, IReadOnlyCollection<string> roles);

    /// <summary>Generates a new, cryptographically random plain-text refresh token value.</summary>
    string GenerateRefreshTokenValue();

    /// <summary>Hashes a plain-text refresh token value for storage/lookup. Never store the plain value.</summary>
    string HashRefreshToken(string plainTextToken);

    TimeSpan RefreshTokenLifetime { get; }
}

public sealed record GeneratedAccessToken(string Token, DateTime ExpiresAtUtc);
