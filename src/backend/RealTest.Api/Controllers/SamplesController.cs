using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTest.Api.Common;
using RealTest.Contracts.Common;

namespace RealTest.Api.Controllers;

/// <summary>
/// Minimal endpoints that exist only to prove the TesterOnly/CompanyUserOnly/AdministratorOnly
/// authorization policies work end to end. Not a real feature — remove once real role-scoped
/// controllers exist.
/// </summary>
[Route("api/v1/samples")]
[Produces("application/json")]
public sealed class SamplesController : ApiControllerBase
{
    [HttpGet("tester-only")]
    [Authorize(Policy = "TesterOnly")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    public ActionResult<MessageResponse> TesterOnly() =>
        Ok(new MessageResponse($"Hello Tester {CurrentUserId}. This endpoint is only reachable by the Tester role."));

    [HttpGet("company-only")]
    [Authorize(Policy = "CompanyUserOnly")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    public ActionResult<MessageResponse> CompanyUserOnly() =>
        Ok(new MessageResponse($"Hello CompanyUser {CurrentUserId}. This endpoint is only reachable by the CompanyUser role."));

    [HttpGet("admin-only")]
    [Authorize(Policy = "AdministratorOnly")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    public ActionResult<MessageResponse> AdministratorOnly() =>
        Ok(new MessageResponse($"Hello Administrator {CurrentUserId}. This endpoint is only reachable by the Administrator role."));
}
