using RealTest.Domain.Authentication;

namespace RealTest.Application.Common.Interfaces;

/// <summary>
/// Feature-specific persistence port for refresh tokens. Deliberately not a generic repository —
/// it exposes only the operations the refresh-token lifecycle actually needs.
/// </summary>
public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>Revokes every active token belonging to the user — used on password reset and on detected token reuse.</summary>
    Task RevokeAllActiveForUserAsync(Guid userId, DateTime utcNow, string? revokedByIpAddress, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
