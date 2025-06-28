using System.Diagnostics.CodeAnalysis;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using Microsoft.AspNetCore.Mvc;
using ServiceRole = FrontendAccountManagement.Core.Enums.ServiceRole;

namespace FrontendAccountManagement.Web.Controllers.AccountManagement;

//[Authorize(Policy = PolicyConstants.AccountManagementPolicy)]
[ExcludeFromCodeCoverage]
[Route(PagePath.ReExManageAccount)]
public class ReExAccountManagementController : Controller
{
    private readonly ISessionManager<JourneySession> _sessionManager;

    public ReExAccountManagementController(
        ISessionManager<JourneySession> sessionManager
        )
    {
        _sessionManager = sessionManager;
    }

    [HttpGet]
    [Route("organisation/{organisationId}/person/{personId}")]
    public async Task<string> ViewDetails([FromRoute] Guid organisationId, [FromRoute] Guid personId)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        return $"It worked! {session.IsComplianceScheme} ";
    }
}
