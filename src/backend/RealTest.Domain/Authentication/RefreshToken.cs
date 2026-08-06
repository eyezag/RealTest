using RealTest.Domain.Common;

namespace RealTest.Domain.Authentication;

/// <summary>
/// A persisted, hashed refresh token. The plain token value is only ever handed to the
/// client once, at issuance; only its hash is stored so a database leak cannot be used
/// to mint new access tokens.
/// </summary>
public sealed class RefreshToken : Entity
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public string CreatedByIpAddress { get; private set; } = string.Empty;

    public string? RevokedByIpAddress { get; private set; }

    private RefreshToken()
    {
    }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc,
        string createdByIpAddress)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        }

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException("Expiry must be after creation time.", nameof(expiresAtUtc));
        }

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc,
            CreatedByIpAddress = createdByIpAddress
        };
    }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

    public void Revoke(DateTime utcNow, string? revokedByIpAddress, Guid? replacedByTokenId = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = utcNow;
        RevokedByIpAddress = revokedByIpAddress;
        ReplacedByTokenId = replacedByTokenId;
    }
}
