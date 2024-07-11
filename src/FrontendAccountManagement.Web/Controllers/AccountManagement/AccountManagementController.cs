using System.Net;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.Sessions;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using FrontendAccountManagement.Web.Extensions;
using ServiceRole = FrontendAccountManagement.Core.Enums.ServiceRole;

namespace FrontendAccountManagement.Web.Controllers.AccountManagement;

[Authorize(Policy = PolicyConstants.AccountManagementPolicy)]
public class AccountManagementController : Controller
{
    private const string RolesNotFoundException = "Could not retrieve service roles or none found";
    private readonly ISessionManager<JourneySession> _sessionManager;
    private readonly IFacadeService _facadeService;    
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly DeploymentRoleOptions _deploymentRoleOptions;

    public AccountManagementController(
        ISessionManager<JourneySession> sessionManager,
        IFacadeService facadeService,
        IOptions<ExternalUrlsOptions> urlOptions,
        IOptions<DeploymentRoleOptions> deploymentRoleOptions)
    {
        _sessionManager = sessionManager;
        _facadeService = facadeService;        
        _urlOptions = urlOptions.Value;
        _deploymentRoleOptions = deploymentRoleOptions.Value;
    }
    
    [HttpGet]
    [Route("")]
    [Route(PagePath.ManageAccount)]
    public async Task<IActionResult> ManageAccount(ManageAccountViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session ??= new JourneySession();

        if (!HasPermissionToView(session.UserData))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }
        
        session.AccountManagementSession.AddUserJourney = null;
        if (session.AccountManagementSession.RemoveUserStatus != null)
        {
            model.UserRemovedStatus = session.AccountManagementSession.RemoveUserStatus;
            model.RemovedUsersName =
                $"{session.AccountManagementSession.RemoveUserJourney.FirstName} {session.AccountManagementSession.RemoveUserJourney.LastName}";
            session.AccountManagementSession.RemoveUserStatus = null;
            session.AccountManagementSession.RemoveUserJourney = null;
        }
        
        if (session.AccountManagementSession.AddUserStatus != null)
        {
            model.InviteStatus = session.AccountManagementSession.AddUserStatus;
            model.InvitedUserEmail =session.AccountManagementSession.InviteeEmailAddress;
            session.AccountManagementSession.InviteeEmailAddress = null;
            session.AccountManagementSession.AddUserStatus = null;
        }

        model.PersonUpdated = TempData["PersonUpdated"] == null ? null : TempData["PersonUpdated"].ToString();

        await SaveSessionAndJourney(session, PagePath.ManageAccount, PagePath.ManageAccount);

        SetCustomBackLink(_urlOptions.LandingPageUrl);
        
        return View(nameof(ManageAccount), model);
    }
    
    [HttpGet]
    [Route(PagePath.TeamMemberEmail)]
    public async Task<IActionResult> TeamMemberEmail()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session.AccountManagementSession.Journey.AddIfNotExists(PagePath.ManageAccount);
        session.AccountManagementSession.AddUserJourney ??= new AddUserJourneyModel();

        await SaveSessionAndJourney(session, PagePath.ManageAccount, PagePath.TeamMemberEmail);
        SetBackLink(session, PagePath.TeamMemberEmail);

        var model = new TeamMemberEmailViewModel
        {
            SavedEmail = session.AccountManagementSession.AddUserJourney.Email
        };

        return View(nameof(TeamMemberEmail), model);
    }
    
    [HttpPost]
    [Route(PagePath.TeamMemberEmail)]
    public async Task<IActionResult> TeamMemberEmail(TeamMemberEmailViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TeamMemberEmail);
            return View(nameof(TeamMemberEmail), model);
        }

        session.AccountManagementSession.InviteeEmailAddress = model.Email;
        session.AccountManagementSession.AddUserJourney.Email = model.Email;

        return await SaveSessionAndRedirect(session, nameof(TeamMemberPermissions), PagePath.TeamMemberEmail, PagePath.TeamMemberPermissions);
    }
    
    [HttpGet]
    [Route(PagePath.TeamMemberPermissions)]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    public async Task<IActionResult> TeamMemberPermissions()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        
        SetBackLink(session, PagePath.TeamMemberPermissions);
        
        var serviceRoles = await _facadeService.GetAllServiceRolesAsync();
        if (serviceRoles == null || !serviceRoles.Any())
        {
            throw new InvalidOperationException(RolesNotFoundException);
        }

        var model = new TeamMemberPermissionsViewModel();

        if (!_deploymentRoleOptions.IsRegulator())
        {
            var basicRoleId = (int)ServiceRole.Basic;
            // temporarily set this to basic users only until next theme
            model.ServiceRoles = serviceRoles
                .Where(x => x.ServiceRoleId == basicRoleId)
                .OrderByDescending(x => x.Key).ToList();
            model.SavedUserRole = session.AccountManagementSession.AddUserJourney.UserRole;
        }
        else
        {
            var regulatorAdminRoleId = (int)ServiceRole.RegulatorAdmin;
            var regulatorBasicRoleId = (int)ServiceRole.RegulatorBasic;
            model.ServiceRoles = serviceRoles
                .Where(x => x.ServiceRoleId >= regulatorAdminRoleId && x.ServiceRoleId <= regulatorBasicRoleId)
                .OrderByDescending(x => x.Key).ToList();
            model.SavedUserRole = session.AccountManagementSession.AddUserJourney.UserRole;
        }
        
        return View(nameof(TeamMemberPermissions), model);
    }
    
    [HttpPost]
    [Route(PagePath.TeamMemberPermissions)]
    public async Task<IActionResult> TeamMemberPermissions(TeamMemberPermissionsViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.TeamMemberPermissions);
            var serviceRoles = await _facadeService.GetAllServiceRolesAsync();
            // temporarily set this to basic users only until next theme
            model.ServiceRoles = serviceRoles
                .Where(x => x.ServiceRoleId == 3)
                .OrderByDescending(x => x.Key).ToList();
            
            return View(nameof(TeamMemberPermissions), model);
        }

        session.AccountManagementSession.RoleKey = model.SelectedUserRole;
        session.AccountManagementSession.AddUserJourney.UserRole = model.SelectedUserRole;

        return await SaveSessionAndRedirect(session, nameof(TeamMemberDetails), PagePath.TeamMemberPermissions, PagePath.TeamMemberDetails);
    }
    
    [HttpGet]
    [AllowAnonymous]
    [Route(PagePath.TeamMemberDetails)]
    public async Task<IActionResult> TeamMemberDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        
        SetBackLink(session, PagePath.TeamMemberDetails);
        var model = new TeamMemberDetailsViewModel
        {
            Email = session.AccountManagementSession.InviteeEmailAddress,
            SelectedUserRole = session.AccountManagementSession.RoleKey,
        };

        return View(nameof(TeamMemberDetails), model);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route(PagePath.TeamMemberDetails)]
    public async Task<IActionResult> TeamMemberDetailsSubmission()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var userData = User.GetUserData();

        var organisation = userData.Organisations.FirstOrDefault();
        if (organisation?.Id == null || organisation.Name == null)
        {
            return RedirectToAction(nameof(ManageAccount));
        }

        var request = new InviteUserRequest
        {
            InvitingUser = new InvitingUser
            {
                FirstName = userData.FirstName,
                LastName = userData.LastName
            },
            InvitedUser = new InvitedUser
            {
                Email = session.AccountManagementSession.InviteeEmailAddress,
                OrganisationId = organisation.Id,
                OrganisationName = organisation.Name,
                RoleKey = session.AccountManagementSession.RoleKey
            }
        };
        
        session.AccountManagementSession.AddUserStatus = await _facadeService.SendUserInvite(request);
        
        return await SaveSessionAndRedirect(session, nameof(ManageAccount), PagePath.TeamMemberDetails, PagePath.ManageAccount);
    }
    
    [HttpPost]
    [Route(PagePath.PreRemoveTeamMember)]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    public async Task<IActionResult> RemoveTeamMemberPreConfirmation(
        string firstName, string lastName, Guid personId)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SetRemoveUserJourneyValues(session, firstName, lastName, personId);
        await SaveSession(session);
        return RedirectToAction("RemoveTeamMemberConfirmation","AccountManagement");
    }
    
    [HttpGet]
    [Route(PagePath.RemoveTeamMember)]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    public async Task<IActionResult> RemoveTeamMemberConfirmation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session.AccountManagementSession.Journey.AddIfNotExists(PagePath.ManageAccount);
        session.AccountManagementSession.Journey.AddIfNotExists(PagePath.RemoveTeamMember);

        SetBackLink(session, PagePath.RemoveTeamMember);
        var model = new RemoveTeamMemberConfirmationViewModel
        {
            FirstName = session.AccountManagementSession.RemoveUserJourney.FirstName,
            LastName = session.AccountManagementSession.RemoveUserJourney.LastName,
            PersonId = session.AccountManagementSession.RemoveUserJourney.PersonId
        };

        await SaveSessionAndJourney(session, PagePath.ManageAccount, PagePath.RemoveTeamMember);

        return View(nameof(RemoveTeamMemberConfirmation), model);
    }
    
    [HttpPost]
    [Route(PagePath.RemoveTeamMember)]
    public async Task<IActionResult> RemoveTeamMemberConfirmation(RemoveTeamMemberConfirmationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
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
            return RedirectToAction(nameof(ManageAccount));
        }
        var organisationId = organisation!.Id.ToString();
        var serviceRoleId = userData.ServiceRoleId;
        var result = await _facadeService.RemoveUserForOrganisation(personExternalId, organisationId, serviceRoleId );
        session.AccountManagementSession.RemoveUserStatus = result;

        return await SaveSessionAndRedirect(session, nameof(ManageAccount), PagePath.RemoveTeamMember, PagePath.ManageAccount);
    }

    private static void SetRemoveUserJourneyValues(JourneySession session, string firstName, string lastName, Guid personId)
    {
        if (session.AccountManagementSession.RemoveUserJourney == null)
        {
            session.AccountManagementSession.RemoveUserJourney = new RemoveUserJourneyModel
            {
                FirstName = firstName,
                LastName = lastName,
                PersonId = personId
            };
        }

        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && personId != Guid.Empty)
        {
            session.AccountManagementSession.RemoveUserJourney.FirstName = firstName;
            session.AccountManagementSession.RemoveUserJourney.LastName = lastName;
            session.AccountManagementSession.RemoveUserJourney.PersonId = personId;
        }
    }
    
    private async Task<RedirectToActionResult> SaveSessionAndRedirect(
        JourneySession session,
        string actionName,
        string currentPagePath,
        string? nextPagePath)
    {
        await SaveSessionAndJourney(session, currentPagePath, nextPagePath);

        return RedirectToAction(actionName);
    }

    private async Task SaveSessionAndJourney(JourneySession session, string currentPagePath, string? nextPagePath)
    {
        ClearRestOfJourney(session, currentPagePath);

        session.AccountManagementSession.Journey.AddIfNotExists(nextPagePath);

        await SaveSession(session);
    }
    
    private async Task SaveSession(JourneySession session)
    {
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
    }

    private static void ClearRestOfJourney(JourneySession session, string currentPagePath)
    {
        var index = session.AccountManagementSession.Journey.IndexOf(currentPagePath);

        // this also cover if current page not found (index = -1) then it clears all pages
        session.AccountManagementSession.Journey = session.AccountManagementSession.Journey.Take(index + 1).ToList();
    }

    private void SetBackLink(JourneySession session, string currentPagePath)
    {
        ViewBag.BackLinkToDisplay = session.AccountManagementSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
    }

    private void SetCustomBackLink(string pagePath)
    {
        ViewBag.CustomBackLinkToDisplay = pagePath;
    }

    private bool HasPermissionToView(UserData userData)
    {
        // only regulator admin can view if regulator deployment
        if (_deploymentRoleOptions.IsRegulator())
        {
            return IsRegulatorAdmin(userData);
        }

        // regulator users cannot view if producer deployment
        return !IsRegulatorUser(userData);
    }

    private static bool IsRegulatorAdmin(UserData userData) =>
        userData.ServiceRoleId == (int)Core.Enums.ServiceRole.RegulatorAdmin;

    private static bool IsRegulatorBasic(UserData userData) =>
        userData.ServiceRoleId == (int)Core.Enums.ServiceRole.RegulatorBasic;

    private static bool IsRegulatorUser(UserData userData) =>
        IsRegulatorAdmin(userData) || IsRegulatorBasic(userData);
}
