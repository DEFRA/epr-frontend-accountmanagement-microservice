using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.ReExAccountManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using ServiceRole = FrontendAccountManagement.Core.Enums.ServiceRole;

namespace FrontendAccountManagement.Web.Controllers.ReEx;

//[Authorize(Policy = PolicyConstants.ReExAccountManagementPolicy)]
[AllowAnonymous]
[ExcludeFromCodeCoverage]
[Route(PagePath.ReExManageAccount)]
public class ReExAccountManagementController(ISessionManager<JourneySession> sessionManager, IFacadeService facadeService, ILogger<ReExAccountManagementController> logger) : Controller
{
    private const string RolesNotFoundException = "Could not retrieve service roles or none found";

    [HttpGet]
    [Route("organisation/{organisationId}/person/{personId}")]
    public async Task<IActionResult> ViewDetails([FromRoute] Guid organisationId, [FromRoute] Guid personId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var session = await sessionManager.GetSessionAsync(HttpContext.Session);
        session ??= new JourneySession();
        var userAccount = User.GetUserData();

        //var userDetails = await facadeService.GetUserDetailsByIdAsync(personId);

        //var serviceRoles = await facadeService.GetAllServiceRolesAsync();

        ViewDetailsViewModel model = new()
        {
            AddedBy = TempData["PersonUpdated"]?.ToString(),
            Email = "Test@Test.com", //userDetails?.ContactEmail,
            AccountPermissions = "Approved User, Administrator"
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
            var userOrg = userAccount.Organisations.FirstOrDefault();

            session.ReExAccountManagementSession.OrganisationName = userOrg?.Name;
            session.ReExAccountManagementSession.OrganisationType = userOrg?.OrganisationType;
            session.ReExAccountManagementSession.BusinessAddress = new Address { Postcode = userOrg?.Postcode };

            model.AddedBy = $"{userAccount.FirstName} {userAccount.LastName}";
            model.Email = userAccount.Email;

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
    [AllowAnonymous]
    [Route(PagePath.TeamMemberPermissions)]
    [Route($"{PagePath.TeamMemberPermissions}/organisation/{{organisationId}}")]
    public async Task<IActionResult> TeamMemberPermissions([FromRoute] Guid organisationId)
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

        var isStandardUser = userData.Organisations.Exists(org => org.Id == organisationId && org.Enrolments.Exists(e => e.ServiceRoleKey.Contains("Re-Ex.BasicUser")));

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

        return await SaveSessionAndRedirect(session, nameof(ViewDetails), PagePath.TeamMemberPermissions, PagePath.TeamMemberDetails);
    }

    [HttpPost]
    //[AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    [Route($"{PagePath.PreRemoveTeamMember}/organisation/{{organisationId}}/person/{{personId}}/firstName/{{firstName}}/lastName/{{lastName}}")]
    public async Task<IActionResult> RemoveTeamMemberPreConfirmation([FromRoute] Guid organisationId, [FromRoute] Guid personId, [FromRoute] string firstName, [FromRoute] string lastName)
    {
        var session = await sessionManager.GetSessionAsync(HttpContext.Session);
        session ??= new JourneySession();

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.RemoveTeamMember);
            return View(nameof(ViewDetails));
        }

        SetRemoveUserJourneyValues(session, firstName, lastName, personId, organisationId);
        await SaveSession(session);
        return RedirectToAction("RemoveTeamMemberConfirmation", "ReExAccountManagement");
    }

    [HttpGet]
    //[AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    [Route(PagePath.RemoveTeamMember)]
    public async Task<IActionResult> RemoveTeamMemberConfirmation()
    {
        var userData = User.GetUserData();

        var session = await sessionManager.GetSessionAsync(HttpContext.Session);

        if (IsEmployeeUser(userData))
        {
            return NotFound();
        }

        session.ReExAccountManagementSession.Journey.AddIfNotExists(PagePath.ReExManageAccount);
        session.ReExAccountManagementSession.Journey.AddIfNotExists(PagePath.RemoveTeamMember);

        SetBackLink(session, PagePath.RemoveTeamMember);
        var model = new RemoveReExTeamMemberConfirmationViewModel
        {
            FirstName = session.ReExAccountManagementSession.ReExRemoveUserJourney.FirstName,
            LastName = session.ReExAccountManagementSession.ReExRemoveUserJourney.LastName,
            PersonId = session.ReExAccountManagementSession.ReExRemoveUserJourney.PersonId,
            OrganisationId = session.ReExAccountManagementSession.ReExRemoveUserJourney.OrganisationId
        };

        await SaveSessionAndJourney(session, PagePath.ReExManageAccount, PagePath.RemoveTeamMember);

        return View(nameof(RemoveTeamMemberConfirmation), model);
    }

    [HttpPost]
    [Route(PagePath.RemoveTeamMember)]
    public async Task<IActionResult> RemoveTeamMemberConfirmation(RemoveTeamMemberConfirmationViewModel model)
    {
        var session = await sessionManager.GetSessionAsync(HttpContext.Session);
        var userData = User.GetUserData();

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TeamMemberPermissions);
            return View(model);
        }

        var personExternalId = model.PersonId.ToString();
        var organisation = userData.Organisations.FirstOrDefault();
        if (organisation?.Id == null)
        {
            return RedirectToAction(nameof(ViewDetails));
        }
        var organisationId = organisation!.Id.ToString();
        var serviceRoleId = userData.ServiceRoleId;
        var result = await facadeService.RemoveUserForOrganisation(personExternalId, organisationId, serviceRoleId);
        session.ReExAccountManagementSession.RemoveUserStatus = result;

        return await SaveSessionAndRedirect(session, nameof(ViewDetails), PagePath.RemoveTeamMember, PagePath.ReExManageAccount);
    }

    private static void SetRemoveUserJourneyValues(JourneySession session, string firstName, string lastName, Guid personId, Guid organisationId)
    {
        if (session.ReExAccountManagementSession.ReExRemoveUserJourney == null)
        {
            session.ReExAccountManagementSession.ReExRemoveUserJourney = new ReExRemoveUserJourneyModel
            {
                FirstName = firstName,
                LastName = lastName,
                PersonId = personId,
                OrganisationId = organisationId
            };
        }

        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && personId != Guid.Empty && organisationId != Guid.Empty)
        {
            session.ReExAccountManagementSession.ReExRemoveUserJourney.FirstName = firstName;
            session.ReExAccountManagementSession.ReExRemoveUserJourney.LastName = lastName;
            session.ReExAccountManagementSession.ReExRemoveUserJourney.PersonId = personId;
            session.ReExAccountManagementSession.ReExRemoveUserJourney.OrganisationId = organisationId;
        }
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

    private static bool IsEmployeeUser(UserData userData)
    {
        var roleInOrganisation = userData.RoleInOrganisation;

        if (string.IsNullOrEmpty(roleInOrganisation))
        {
            throw new InvalidOperationException("Unknown role in organisation.");
        }

        return roleInOrganisation == PersonRole.Employee.ToString();
    }
}
