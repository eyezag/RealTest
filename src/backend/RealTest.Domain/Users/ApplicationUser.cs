using Microsoft.AspNetCore.Identity;
using RealTest.Domain.Enums;

namespace RealTest.Domain.Users;

/// <summary>
/// The RealTest application user. Extends ASP.NET Core Identity's user model with the
/// domain fields RealTest needs (name, marketplace role, account lifecycle state).
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public UserType UserType { get; set; }

    public AccountStatus AccountStatus { get; set; } = AccountStatus.PendingVerification;

    public DateTime? EmailVerifiedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsLoginAllowed() => AccountStatus is AccountStatus.PendingVerification or AccountStatus.Active;
}
