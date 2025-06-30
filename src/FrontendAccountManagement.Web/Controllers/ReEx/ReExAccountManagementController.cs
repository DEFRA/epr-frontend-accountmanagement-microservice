using System.Diagnostics.CodeAnalysis;
using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ServiceRole = FrontendAccountManagement.Core.Enums.ServiceRole;

namespace FrontendAccountManagement.Web.Controllers.ReEx;

//[Authorize(Policy = PolicyConstants.ReExAccountManagementPolicy)]
[AllowAnonymous]
[ExcludeFromCodeCoverage]
[Route(PagePath.ReExManageAccount)]
public class ReExAccountManagementController(ISessionManager<JourneySession> sessionManager, IFacadeService facadeService) : Controller
{
    private readonly ISessionManager<JourneySession> _sessionManager = sessionManager;
    private readonly IFacadeService _facadeService = facadeService;
    private const string RolesNotFoundException = "Could not retrieve service roles or none found";

    [HttpGet]
    [Route("organisation/{organisationId}/person/{personId}")]
    public async Task<string> ViewDetails([FromRoute] Guid organisationId, [FromRoute] Guid personId)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session.ReExAccountManagementSession.OrganisationId = organisationId;
        session.ReExAccountManagementSession.PersonId = personId;

        SaveSession(session);

        return $"It worked! {session.IsComplianceScheme} ";
    }

    [HttpGet]
    //[Authorize(Policy = PolicyConstants.ReExAddTeamMemberPolicy)]
    [Route(PagePath.TeamMemberEmail)]
    public async Task<IActionResult> TeamMemberEmail()
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(ViewDetails));
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session.ReExAccountManagementSession.Journey.AddIfNotExists(PagePath.ReExManageAccount);
        session.ReExAccountManagementSession.AddUserJourney ??= new AddUserJourneyModel();

        await SaveSessionAndJourney(session, PagePath.ReExManageAccount, PagePath.TeamMemberEmail);
        SetBackLink(session, PagePath.TeamMemberEmail);

        var model = new TeamMemberEmailViewModel
        {
            SavedEmail = session.ReExAccountManagementSession.AddUserJourney.Email
        };

        return View(nameof(TeamMemberEmail), model);
    }

    [HttpPost]
    //[Authorize(Policy = PolicyConstants.ReExAddTeamMemberPolicy)]
    [Route(PagePath.TeamMemberEmail)]
    public async Task<IActionResult> TeamMemberEmail(TeamMemberEmailViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TeamMemberEmail);
            return View(nameof(TeamMemberEmail), model);
        }

        session.ReExAccountManagementSession.InviteeEmailAddress = model.Email;
        session.ReExAccountManagementSession.AddUserJourney.Email = model.Email;

        return await SaveSessionAndRedirect(session, nameof(TeamMemberPermissions), PagePath.TeamMemberEmail, PagePath.TeamMemberPermissions);
    }
    [HttpGet]
    [AllowAnonymous]
    [Route(PagePath.TeamMemberPermissions)]
    [Route($"{PagePath.TeamMemberPermissions}/organisation/{{organisationId}}")]
    public async Task<IActionResult> TeamMemberPermissions([FromRoute] Guid organisationId)
    {
        var userData = User.GetUserData();

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.TeamMemberPermissions);

        var serviceRoles = await _facadeService.GetAllServiceRolesAsync();

        if (serviceRoles == null)
        {
            throw new InvalidOperationException(RolesNotFoundException);
        }

        var reExRoles = serviceRoles.Where(r => r.Key.StartsWith("Re-Ex.ApprovedPerson") ||
                                                r.Key.StartsWith("Re-Ex.StandardUser") ||
                                                r.Key.StartsWith("Re-Ex.BasicUser")).ToList();

        var isStandardUser = userData.Organisations.Any(org =>
            org.Id == organisationId &&
            org.Enrolments.Any(e => e.ServiceRoleKey.Contains("Re-Ex.BasicUser")));

        var model = new TeamMemberPermissionsViewModel
        {
            OrganisationId = organisationId,
            ServiceRoles = reExRoles,
            IsStandardUser = isStandardUser,
            SavedUserRole = session.ReExAccountManagementSession.AddUserJourney.UserRole
        };

        return View(nameof(TeamMemberPermissions), model);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route(PagePath.TeamMemberPermissions)]
    public async Task<IActionResult> TeamMemberPermissions(TeamMemberPermissionsViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TeamMemberPermissions);
            var serviceRoles = await _facadeService.GetAllServiceRolesAsync();

            model.ServiceRoles = serviceRoles.Where(r => r.Key.StartsWith("Re-Ex.ApprovedPerson") ||
                                                         r.Key.StartsWith("Re-Ex.StandardUser") ||
                                                         r.Key.StartsWith("Re-Ex.BasicUser")).ToList();

            return View(nameof(TeamMemberPermissions), model);
        }

        session.ReExAccountManagementSession.RoleKey = model.SelectedUserRole;
        session.ReExAccountManagementSession.AddUserJourney.UserRole = model.SelectedUserRole;

        return await SaveSessionAndRedirect(session, nameof(ViewDetails), PagePath.TeamMemberPermissions, PagePath.TeamMemberDetails);
    }

    private async Task<RedirectToActionResult> SaveSessionAndRedirect(JourneySession session, string actionName, string currentPagePath, string? nextPagePath)
    {
        await SaveSessionAndJourney(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName);
    }

    /// <summary>
    /// Saves the session data and adds a step to the list detailing the user's journey through the site.
    /// </summary>
    /// <param name="session">The session data to save.</param>
    /// <param name="sourcePagePath">The page this step of the journey starts from (typically the page we've just come from.).</param>
    /// <param name="destinationPagePath">The page this step of the journey ends at (typically the current page.).</param>
    /// <returns>A <see cref="Task"/>.</returns>
    private async Task SaveSessionAndJourney(JourneySession session, string sourcePagePath, string? destinationPagePath)
    {
        ClearRestOfJourney(session, sourcePagePath);

        session.ReExAccountManagementSession.Journey.AddIfNotExists(destinationPagePath);

        await SaveSession(session);
    }

    private async Task SaveSession(JourneySession session)
    {
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
    }

    private static void ClearRestOfJourney(JourneySession session, string currentPagePath)
    {
        var index = session.ReExAccountManagementSession.Journey.IndexOf(currentPagePath);

        // this also cover if current page not found (index = -1) then it clears all pages
        session.ReExAccountManagementSession.Journey = session.ReExAccountManagementSession.Journey.Take(index + 1).ToList();
    }

    private void SetBackLink(JourneySession session, string currentPagePath)
    {
        ViewBag.BackLinkToDisplay = session.ReExAccountManagementSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
    }
}
