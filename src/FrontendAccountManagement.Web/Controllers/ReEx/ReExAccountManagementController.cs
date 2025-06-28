using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Resources.Views.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Diagnostics.CodeAnalysis;
using ServiceRole = FrontendAccountManagement.Core.Enums.ServiceRole;

namespace FrontendAccountManagement.Web.Controllers.AccountManagement;

//[Authorize(Policy = PolicyConstants.AccountManagementPolicy)]
[ExcludeFromCodeCoverage]
[Route(PagePath.ReExManageAccount)]
public class ReExAccountManagementController(ISessionManager<JourneySession> sessionManager) : Controller
{
    private readonly ISessionManager<JourneySession> _sessionManager = sessionManager;

    [HttpGet]
    [Route("organisation/{organisationId}/person/{personId}")]
    public async Task<string> ViewDetails([FromRoute] Guid organisationId, [FromRoute] Guid personId)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        return $"It worked! {session.IsComplianceScheme} ";
    }

    [HttpGet]
    [Route($"{PagePath.TeamMemberEmail}/organisation/{{organisationId}}")]
    public async Task<IActionResult> TeamMemberEmail([FromRoute] Guid organisationId)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(ViewDetails));
        }

        var userData = User.GetUserData();
        if (IsEmployeeUser(userData))
        {
            return NotFound();
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session.ReExAccountManagementSession.OrganisationId = organisationId;
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
    [Route($"{PagePath.TeamMemberEmail}/organisation/{{organisationId}}")]
    public async Task<IActionResult> TeamMemberEmail([FromRoute] Guid organisationId, TeamMemberEmailViewModel model)
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
    [Route(PagePath.TeamMemberPermissions)]
    public async Task<IActionResult> TeamMemberPermissions()
    {
        //var userData = User.GetUserData();
        //var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        //if (IsEmployeeUser(userData))
        //{
        //    return NotFound();
        //}

        //SetBackLink(session, PagePath.TeamMemberPermissions);

        //var serviceRoles = await _facadeService.GetAllServiceRolesAsync();
        //if (serviceRoles == null || !serviceRoles.Any())
        //{
        //    throw new InvalidOperationException(RolesNotFoundException);
        //}

        //var model = new TeamMemberPermissionsViewModel();

        //if (!_deploymentRoleOptions.IsRegulator())
        //{
        //    var basicRoleId = (int)ServiceRole.Basic;
        //    // temporarily set this to basic users only until next theme
        //    model.ServiceRoles = serviceRoles
        //        .Where(x => x.ServiceRoleId == basicRoleId)
        //        .OrderByDescending(x => x.Key).ToList();
        //    model.SavedUserRole = session.AccountManagementSession.AddUserJourney.UserRole;
        //}
        //else
        //{
        //    var regulatorAdminRoleId = (int)ServiceRole.RegulatorAdmin;
        //    var regulatorBasicRoleId = (int)ServiceRole.RegulatorBasic;
        //    model.ServiceRoles = serviceRoles
        //        .Where(x => x.ServiceRoleId >= regulatorAdminRoleId && x.ServiceRoleId <= regulatorBasicRoleId)
        //        .OrderByDescending(x => x.Key).ToList();
        //    model.SavedUserRole = session.AccountManagementSession.AddUserJourney.UserRole;
        //}

        //return View(nameof(TeamMemberPermissions), model);

        return View();
    }

    #region Helper Methods

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

    private static bool IsEmployeeUser(UserData userData)
    {
        var roleInOrganisation = userData.RoleInOrganisation;

        if (string.IsNullOrEmpty(roleInOrganisation))
        {
            throw new InvalidOperationException("Unknown role in organisation.");
        }

        return roleInOrganisation == PersonRole.Employee.ToString();
    }

    private void SetBackLink(JourneySession session, string currentPagePath)
    {
        ViewBag.BackLinkToDisplay = session.ReExAccountManagementSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
    }

    #endregion Helper Methods
}
