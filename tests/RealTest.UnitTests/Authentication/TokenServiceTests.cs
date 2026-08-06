using Microsoft.Extensions.Options;
using RealTest.Domain.Enums;
using RealTest.Domain.Users;
using RealTest.Infrastructure.Authentication;
using Shouldly;
using Xunit;

namespace RealTest.UnitTests.Authentication;

public sealed class TokenServiceTests
{
    private readonly TokenService _tokenService = new(Options.Create(new JwtOptions
    {
        Issuer = "realtest-tests",
        Audience = "realtest-tests",
        SigningKey = "unit-test-signing-key-at-least-32-characters-long",
        AccessTokenLifetimeMinutes = 15,
        RefreshTokenLifetimeDays = 7
    }));

    [Fact]
    public void HashRefreshToken_IsDeterministic_ForTheSameInput()
    {
        var plainToken = _tokenService.GenerateRefreshTokenValue();

        var hash1 = _tokenService.HashRefreshToken(plainToken);
        var hash2 = _tokenService.HashRefreshToken(plainToken);

        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void HashRefreshToken_NeverEqualsThePlainTextValue()
    {
        var plainToken = _tokenService.GenerateRefreshTokenValue();

        var hash = _tokenService.HashRefreshToken(plainToken);

        hash.ShouldNotBe(plainToken);
    }

    [Fact]
    public void GenerateRefreshTokenValue_ProducesUniqueValuesEachCall()
    {
        var first = _tokenService.GenerateRefreshTokenValue();
        var second = _tokenService.GenerateRefreshTokenValue();

        first.ShouldNotBe(second);
    }

    [Fact]
    public void HashRefreshToken_DifferentInputsProduceDifferentHashes()
    {
        var hashA = _tokenService.HashRefreshToken("token-a");
        var hashB = _tokenService.HashRefreshToken("token-b");

        hashA.ShouldNotBe(hashB);
    }

    [Fact]
    public void GenerateAccessToken_SetsExpiryAccordingToConfiguredLifetime()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "ada@example.com",
            DisplayName = "Ada Lovelace",
            UserType = UserType.Tester,
            AccountStatus = AccountStatus.Active
        };

        var before = DateTime.UtcNow;
        var result = _tokenService.GenerateAccessToken(user, [nameof(UserType.Tester)]);
        var after = DateTime.UtcNow;

        result.Token.ShouldNotBeNullOrWhiteSpace();
        result.ExpiresAtUtc.ShouldBeInRange(before.AddMinutes(15), after.AddMinutes(15).AddSeconds(5));
    }

    [Fact]
    public void RefreshTokenLifetime_MatchesConfiguredDays()
    {
        _tokenService.RefreshTokenLifetime.ShouldBe(TimeSpan.FromDays(7));
    }
}
