using RealTest.Domain.Authentication;
using Shouldly;
using Xunit;

namespace RealTest.UnitTests.Authentication;

public sealed class RefreshTokenTests
{
    private static readonly DateTime CreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithExpiryBeforeCreation_Throws()
    {
        Should.Throw<ArgumentException>(() => RefreshToken.Create(
            Guid.NewGuid(), "hash", CreatedAt, CreatedAt.AddMinutes(-1), "127.0.0.1"));
    }

    [Fact]
    public void Create_WithEmptyTokenHash_Throws()
    {
        Should.Throw<ArgumentException>(() => RefreshToken.Create(
            Guid.NewGuid(), string.Empty, CreatedAt, CreatedAt.AddDays(7), "127.0.0.1"));
    }

    [Fact]
    public void IsExpired_BeforeExpiry_ReturnsFalse()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", CreatedAt, CreatedAt.AddDays(7), "127.0.0.1");

        token.IsExpired(CreatedAt.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    public void IsExpired_AtOrAfterExpiry_ReturnsTrue()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", CreatedAt, CreatedAt.AddDays(7), "127.0.0.1");

        token.IsExpired(CreatedAt.AddDays(7)).ShouldBeTrue();
        token.IsExpired(CreatedAt.AddDays(8)).ShouldBeTrue();
    }

    [Fact]
    public void IsActive_WhenNeitherExpiredNorRevoked_ReturnsTrue()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", CreatedAt, CreatedAt.AddDays(7), "127.0.0.1");

        token.IsActive(CreatedAt.AddHours(1)).ShouldBeTrue();
    }

    [Fact]
    public void Revoke_SetsRevokedAtIpAndReplacementToken()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", CreatedAt, CreatedAt.AddDays(7), "127.0.0.1");
        var replacementId = Guid.NewGuid();
        var revokedAt = CreatedAt.AddHours(2);

        token.Revoke(revokedAt, "203.0.113.5", replacementId);

        token.IsRevoked.ShouldBeTrue();
        token.RevokedAtUtc.ShouldBe(revokedAt);
        token.RevokedByIpAddress.ShouldBe("203.0.113.5");
        token.ReplacedByTokenId.ShouldBe(replacementId);
        token.IsActive(revokedAt).ShouldBeFalse();
    }

    [Fact]
    public void Revoke_CalledTwice_KeepsFirstRevocationDetails()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", CreatedAt, CreatedAt.AddDays(7), "127.0.0.1");
        var firstRevokeAt = CreatedAt.AddHours(1);

        token.Revoke(firstRevokeAt, "203.0.113.5");
        token.Revoke(CreatedAt.AddHours(2), "198.51.100.9");

        token.RevokedAtUtc.ShouldBe(firstRevokeAt);
        token.RevokedByIpAddress.ShouldBe("203.0.113.5");
    }
}
