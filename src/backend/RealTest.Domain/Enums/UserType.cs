namespace RealTest.Domain.Enums;

/// <summary>
/// The kind of participant a user is in the RealTest marketplace.
/// Administrator accounts can never be created through public registration.
/// </summary>
public enum UserType
{
    Tester = 1,
    CompanyUser = 2,
    Administrator = 3
}
