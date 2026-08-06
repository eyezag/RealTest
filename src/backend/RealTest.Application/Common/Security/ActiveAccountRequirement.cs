using Microsoft.AspNetCore.Authorization;
using RealTest.Domain.Enums;

namespace RealTest.Application.Common.Security;

/// <summary>
/// Authorization requirement proving that role-based policies can be composed with custom,
/// non-role rules. Rejects callers whose account is Suspended or Disabled even though their
/// access token is otherwise valid (tokens are short-lived, but this closes the gap).
/// </summary>
public sealed class ActiveAccountRequirement : IAuthorizationRequirement;

public sealed class ActiveAccountRequirementHandler : AuthorizationHandler<ActiveAccountRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveAccountRequirement requirement)
    {
        var statusClaim = context.User.FindFirst(AppClaimTypes.AccountStatus)?.Value;

        if (Enum.TryParse<AccountStatus>(statusClaim, out var status) &&
            status is AccountStatus.Active or AccountStatus.PendingVerification)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
