using RealTest.Domain.Users;

namespace RealTest.Application.Common.Interfaces;

/// <summary>
/// Wraps ASP.NET Core Identity (UserManager/SignInManager) behind an interface the Application
/// layer can depend on without taking a hard dependency on the Infrastructure implementation.
/// </summary>
public interface IIdentityService
{
    Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<ApplicationUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IdentityOperationResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken);

    Task AddToRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken);

    Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken);

    Task<bool> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken cancellationToken);

    Task<bool> IsLockedOutAsync(ApplicationUser user, CancellationToken cancellationToken);

    Task RecordLoginFailureAsync(ApplicationUser user, CancellationToken cancellationToken);

    Task ResetLoginFailureCountAsync(ApplicationUser user, CancellationToken cancellationToken);

    Task<string> GenerateEmailVerificationTokenAsync(ApplicationUser user, CancellationToken cancellationToken);

    Task<bool> ConfirmEmailAsync(ApplicationUser user, string token, CancellationToken cancellationToken);

    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken);

    Task<IdentityOperationResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword, CancellationToken cancellationToken);

    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken);
}

public sealed record IdentityOperationResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static IdentityOperationResult Success() => new(true, []);

    public static IdentityOperationResult Failure(IEnumerable<string> errors) => new(false, errors.ToArray());
}
