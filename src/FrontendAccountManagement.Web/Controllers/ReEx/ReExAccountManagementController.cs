using System.Diagnostics.CodeAnalysis;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.ReExAccountManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceRole = FrontendAccountManagement.Core.Enums.ServiceRole;

namespace FrontendAccountManagement.Web.Controllers.ReEx;

[Authorize(Policy = PolicyConstants.ReExAccountManagementPolicy)]
[ExcludeFromCodeCoverage]
[Route(PagePath.ReExManageAccount)]
public class ReExAccountManagementController(
    ISessionManager<JourneySession> sessionManager, 
    IFacadeService facadeService, 
    ILogger<ReExAccountManagementController> logger,
    IOptions<ExternalUrlsOptions> urlOptions) : Controller
{
    private const string RolesNotFoundException = "Could not retrieve service roles or none found";

    [HttpGet]
    [Route("organisation/{organisationId}/person/{personId}/enrolment/{enrolmentId}")]
    public async Task<IActionResult> ViewDetails([FromRoute] Guid organisationId, [FromRoute] Guid personId, [FromRoute] int enrolmentId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var session = await sessionManager.GetSessionAsync(HttpContext.Session);
        session ??= new JourneySession();
        var userAccount = User.GetUserData();

        ViewDetailsViewModel model = new()
        {
            AccountRole = "Approved User, Administrator"
            //AccountPermissions = (serviceRoles.Where(r => r.Key.StartsWith("Re-Ex.ApprovedPerson") ||
            //                                              r.Key.StartsWith("Re-Ex.StandardUser") ||
            //                                              r.Key.StartsWith("Re-Ex.BasicUser")).ToList()).ToString(),
        };

        if (userAccount is null)
        {
            logger.LogInformation("User authenticated but account could not be found");
        }
        else
        {
            model.AddedBy = $"{userAccount.FirstName} {userAccount.LastName}";
            model.Email = userAccount.Email;
            model.FirstName = userAccount.FirstName;
            model.LastName = userAccount.LastName;

            var serviceRoleEnum = (ServiceRole)userAccount.ServiceRoleId;

            model.AccountPermissions = $"{serviceRoleEnum}.{userAccount.RoleInOrganisation}";
        }

        session.ReExAccountManagementSession.OrganisationId = organisationId;
        session.ReExAccountManagementSession.PersonId = personId;

        session.SelectedOrganisationId = organisationId;

        await SaveSessionAndJourney(session, PagePath.ManageAccount, PagePath.ManageAccount);

        SetBackLink(session, PagePath.ReExManageAccount);

        return View(nameof(ViewDetails), model);
    }

    [HttpGet]
    //[Authorize(Policy = PolicyConstants.ReExAddTeamMemberPolicy)]
    [Route($"{PagePath.TeamMemberEmail}/organisation/{{organisationId}}")]
    public async Task<IActionResult> TeamMemberEmail([FromRoute] Guid organisationId)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(ViewDetails));
        }

        var session = await sessionManager.GetSessionAsync(HttpContext.Session);
        session ??= new JourneySession();

        session.ReExAccountManagementSession.Journey.AddIfNotExists(PagePath.ReExManageAccount);
        session.ReExAccountManagementSession.AddUserJourney ??= new AddUserJourneyModel();
        session.ReExAccountManagementSession.OrganisationId = organisationId;

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
        var session = await sessionManager.GetSessionAsync(HttpContext.Session);

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
        var userData = User.GetUserData();

        var session = await sessionManager.GetSessionAsync(HttpContext.Session);
        SetBackLink(session, PagePath.TeamMemberPermissions);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.ReExManageAccount);
            return View(nameof(ViewDetails));
        }

        var serviceRoles = await facadeService.GetAllServiceRolesAsync() ?? throw new InvalidOperationException(RolesNotFoundException);

        var reExRoles = serviceRoles.Where(r => r.Key.StartsWith("Re-Ex.ApprovedPerson") ||
                                                r.Key.StartsWith("Re-Ex.StandardUser") ||
                                                r.Key.StartsWith("Re-Ex.BasicUser")).ToList();
        
        var isStandardUser = userData.Organisations.Any(org =>
            org.Id == session.ReExAccountManagementSession.OrganisationId &&
            org.Enrolments.Any(e => e.ServiceRoleKey.Contains("Re-Ex.BasicUser")));

        var model = new TeamMemberPermissionsViewModel
        {
            OrganisationId = session.ReExAccountManagementSession.OrganisationId,
            ServiceRoles = reExRoles,
            IsStandardUser = isStandardUser,
            SavedUserRole = session.ReExAccountManagementSession.AddUserJourney.UserRole
        };

        return View(nameof(TeamMemberPermissions), model);
    }

    [HttpPost]
    [Route(PagePath.TeamMemberPermissions)]
    public async Task<IActionResult> TeamMemberPermissions(TeamMemberPermissionsViewModel model)
    {
        var session = await sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TeamMemberPermissions);
            var serviceRoles = await facadeService.GetAllServiceRolesAsync();

            model.ServiceRoles = serviceRoles.Where(r => r.Key.StartsWith("Re-Ex.ApprovedPerson") ||
                                                         r.Key.StartsWith("Re-Ex.StandardUser") ||
                                                         r.Key.StartsWith("Re-Ex.BasicUser")).ToList();

            return View(nameof(TeamMemberPermissions), model);
        }

        session.ReExAccountManagementSession.RoleKey = model.SelectedUserRole;
        session.ReExAccountManagementSession.AddUserJourney.UserRole = model.SelectedUserRole;

        return await SaveSessionAndRedirect(session, nameof(ViewDetails), PagePath.TeamMemberPermissions, PagePath.TeamMemberDetails); // TODO: change to the next view
    }

    [HttpGet]
    [Route(PagePath.PreRemoveTeamMember)]
    public async Task<IActionResult> RemoveTeamMemberPreConfirmation(ViewDetailsViewModel model)
    {
        var session = await sessionManager.GetSessionAsync(HttpContext.Session);

        session.ReExAccountManagementSession.ReExRemoveUserJourney = new ReExRemoveUserJourneyModel
        {
            PersonId = model.PersonId,
            OrganisationId = model.OrganisationId,
            EnrolmentId = model.EnrolmentId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Role = model.AccountRole
        };
        
        await SaveSession(session);
        return RedirectToAction("RemoveTeamMemberConfirmation", "ReExAccountManagement");
    }

    [HttpGet]
    [Route(PagePath.RemoveTeamMember)]
    public async Task<IActionResult> RemoveTeamMemberConfirmation()
    {
        var session = await sessionManager.GetSessionAsync(HttpContext.Session);

        session.ReExAccountManagementSession.Journey.AddIfNotExists(PagePath.ReExManageAccount);
        session.ReExAccountManagementSession.Journey.AddIfNotExists(PagePath.RemoveTeamMember);

        SetBackLink(session, PagePath.RemoveTeamMember);
        var model = new RemoveReExTeamMemberConfirmationViewModel
        {
            FirstName = session.ReExAccountManagementSession.ReExRemoveUserJourney.FirstName,
            LastName = session.ReExAccountManagementSession.ReExRemoveUserJourney.LastName,
            PersonId = session.ReExAccountManagementSession.ReExRemoveUserJourney.PersonId,
            OrganisationId = session.ReExAccountManagementSession.ReExRemoveUserJourney.OrganisationId,
            Role = session.ReExAccountManagementSession.ReExRemoveUserJourney.Role,
            EnrolmentId = session.ReExAccountManagementSession.ReExRemoveUserJourney.EnrolmentId
            
        };

        await SaveSessionAndJourney(session, PagePath.ReExManageAccount, PagePath.RemoveTeamMember);

        return View(nameof(RemoveTeamMemberConfirmation), model);
    }
    
    [HttpPost]
    [Route(PagePath.RemoveTeamMember)]
    public async Task<IActionResult> RemoveTeamMemberConfirmation(RemoveReExTeamMemberConfirmationViewModel model)
    {
        var session = await sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.RemoveTeamMember);
            return View(nameof(RemoveTeamMemberConfirmation));
        }

        var personExternalId = model.PersonId.ToString();
        var organisationId = model.OrganisationId.ToString();
    
        var result = await facadeService.DeletePersonConnectionAndEnrolment(personExternalId, organisationId, model.EnrolmentId);
        session.ReExAccountManagementSession.RemoveUserStatus = result;
        
        session.ReExAccountManagementSession.ReExRemoveUserJourney = new ReExRemoveUserJourneyModel
        {
            IsRemoved = true,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Role = model.Role
        };

        await SaveSessionAndJourney(session, PagePath.RemoveTeamMember, PagePath.ReExManageAccount);
        
        return Redirect(urlOptions.Value.EprPrnManageOrganisationLink); 
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
        await sessionManager.SaveSessionAsync(HttpContext.Session, session);
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
