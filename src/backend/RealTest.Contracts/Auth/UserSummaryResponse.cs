using RealTest.Contracts.Common;

namespace RealTest.Contracts.Auth;

public sealed record UserSummaryResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    UserType UserType,
    AccountStatus AccountStatus);
