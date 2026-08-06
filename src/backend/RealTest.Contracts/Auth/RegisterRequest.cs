using RealTest.Contracts.Common;

namespace RealTest.Contracts.Auth;

public sealed class RegisterRequest
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>Only <see cref="UserType.Tester"/> and <see cref="UserType.CompanyUser"/> are accepted here.</summary>
    public UserType UserType { get; init; }

    public bool AcceptTerms { get; init; }
}
