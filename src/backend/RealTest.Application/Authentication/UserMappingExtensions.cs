using RealTest.Contracts.Auth;
using RealTest.Domain.Users;
using ContractsAccountStatus = RealTest.Contracts.Common.AccountStatus;
using ContractsUserType = RealTest.Contracts.Common.UserType;
using DomainAccountStatus = RealTest.Domain.Enums.AccountStatus;
using DomainUserType = RealTest.Domain.Enums.UserType;

namespace RealTest.Application.Authentication;

internal static class UserMappingExtensions
{
    public static DomainUserType ToDomain(this ContractsUserType userType) => userType switch
    {
        ContractsUserType.Tester => DomainUserType.Tester,
        ContractsUserType.CompanyUser => DomainUserType.CompanyUser,
        ContractsUserType.Administrator => DomainUserType.Administrator,
        _ => throw new ArgumentOutOfRangeException(nameof(userType), userType, "Unsupported user type.")
    };

    public static ContractsUserType ToContract(this DomainUserType userType) => userType switch
    {
        DomainUserType.Tester => ContractsUserType.Tester,
        DomainUserType.CompanyUser => ContractsUserType.CompanyUser,
        DomainUserType.Administrator => ContractsUserType.Administrator,
        _ => throw new ArgumentOutOfRangeException(nameof(userType), userType, "Unsupported user type.")
    };

    public static ContractsAccountStatus ToContract(this DomainAccountStatus status) => status switch
    {
        DomainAccountStatus.PendingVerification => ContractsAccountStatus.PendingVerification,
        DomainAccountStatus.Active => ContractsAccountStatus.Active,
        DomainAccountStatus.Suspended => ContractsAccountStatus.Suspended,
        DomainAccountStatus.Disabled => ContractsAccountStatus.Disabled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported account status.")
    };

    public static UserSummaryResponse ToSummary(this ApplicationUser user) => new(
        user.Id,
        user.Email ?? string.Empty,
        user.FirstName,
        user.LastName,
        user.DisplayName,
        user.UserType.ToContract(),
        user.AccountStatus.ToContract());
}
