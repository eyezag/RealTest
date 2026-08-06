using Microsoft.AspNetCore.Identity;
using RealTest.Application.Common.Interfaces;
using RealTest.Domain.Users;

namespace RealTest.Infrastructure.Identity;

public sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        userManager.FindByEmailAsync(email);

    public Task<ApplicationUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        userManager.FindByIdAsync(userId.ToString());

    public async Task<IdentityOperationResult> CreateUserAsync(ApplicationUser user, string password, CancellationToken cancellationToken)
    {
        var result = await userManager.CreateAsync(user, password);
        return result.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task AddToRoleAsync(ApplicationUser user, string role, CancellationToken cancellationToken)
    {
        await userManager.AddToRoleAsync(user, role);
    }

    public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        userManager.GetRolesAsync(user);

    public Task<bool> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken cancellationToken) =>
        userManager.CheckPasswordAsync(user, password);

    public Task<bool> IsLockedOutAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        userManager.IsLockedOutAsync(user);

    public async Task RecordLoginFailureAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await userManager.AccessFailedAsync(user);
    }

    public async Task ResetLoginFailureCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (await userManager.GetAccessFailedCountAsync(user) > 0)
        {
            await userManager.ResetAccessFailedCountAsync(user);
        }
    }

    public Task<string> GenerateEmailVerificationTokenAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        userManager.GenerateEmailConfirmationTokenAsync(user);

    public async Task<bool> ConfirmEmailAsync(ApplicationUser user, string token, CancellationToken cancellationToken)
    {
        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<IdentityOperationResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword, CancellationToken cancellationToken)
    {
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await userManager.UpdateAsync(user);
    }
}
