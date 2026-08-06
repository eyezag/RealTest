using RealTest.Domain.Enums;
using RealTest.Domain.Users;
using Shouldly;
using Xunit;

namespace RealTest.UnitTests.Users;

public sealed class ApplicationUserAccountStatusTests
{
    [Theory]
    [InlineData(AccountStatus.PendingVerification, true)]
    [InlineData(AccountStatus.Active, true)]
    [InlineData(AccountStatus.Suspended, false)]
    [InlineData(AccountStatus.Disabled, false)]
    public void IsLoginAllowed_ReflectsAccountStatus(AccountStatus status, bool expected)
    {
        var user = new ApplicationUser { AccountStatus = status };

        user.IsLoginAllowed().ShouldBe(expected);
    }
}
