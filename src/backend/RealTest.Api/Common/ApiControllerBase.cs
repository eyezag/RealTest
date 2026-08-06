using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace RealTest.Api.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated request is missing a user identifier claim."));

    protected string ClientIpAddress =>
        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
