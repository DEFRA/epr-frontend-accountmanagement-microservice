using System;
using System.IO;
using System.Net;
using System.Text.Json;
using AutoMapper;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Services.Dto.CompaniesHouse;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.Profiles;
using FrontendAccountManagement.Web.Utilities.Interfaces;
using FrontendAccountManagement.Web.ViewModels;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.Identity.Web;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using ServiceRole = FrontendAccountManagement.Core.Enums.ServiceRole;

namespace FrontendAccountManagement.Web.Controllers.AccountManagement;

[Authorize(Policy = PolicyConstants.AccountManagementPolicy)]
public class AccountManagementController : Controller
{
    private const string RolesNotFoundException = "Could not retrieve service roles or none found";
    private const string CheckYourOrganisationDetailsKey = "CheckYourOrganisationDetails";
    private const string OrganisationDetailsUpdatedTimeKey = "OrganisationDetailsUpdatedTime";
    private const string AmendedUserDetailsKey = "AmendedUserDetails";
    private const string NewUserDetailsKey = "NewUserDetails";
    private const string ServiceKey = "Packaging";
    private const string PostcodeLookupFailedKey = "PostcodeLookupFailed";
    private readonly ISessionManager<JourneySession> _sessionManager;
    private readonly IFacadeService _facadeService;
    private readonly ILogger<AccountManagementController> _logger;
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly DeploymentRoleOptions _deploymentRoleOptions;
    private readonly IClaimsExtensionsWrapper _claimsExtensionsWrapper;
    private readonly IMapper _mapper;
    private readonly IFeatureManager _featureManager;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Its Allowed for now in this case")]
    public AccountManagementController(
        ISessionManager<JourneySession> sessionManager,
        IFacadeService facadeService,
        IOptions<ExternalUrlsOptions> urlOptions,
        IOptions<DeploymentRoleOptions> deploymentRoleOptions,
        ILogger<AccountManagementController> logger,
        IClaimsExtensionsWrapper claimsExtensionsWrapper,
        IFeatureManager featureManager,
        IMapper mapper)
    {
        _sessionManager = sessionManager;
        _facadeService = facadeService;
        _logger = logger;
        _urlOptions = urlOptions.Value;
        _deploymentRoleOptions = deploymentRoleOptions.Value;
        _claimsExtensionsWrapper = claimsExtensionsWrapper;
        _featureManager = featureManager;
        _mapper = mapper;
    }

    [HttpGet]
    [Route("")]
    [Route(PagePath.ManageAccount)]
    public async Task<IActionResult> ManageAccount(ManageAccountViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        session ??= new JourneySession();

        var userAccount = User.GetUserData();

        if (!HasPermissionToView(userAccount))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }

        session.AccountManagementSession.AddUserJourney = null;
        session.AccountManagementSession.IsUpdateCompanyName = null;
        session.AccountManagementSession.IsUpdateCompanyAddress = null;
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
            model.InvitedUserEmail = session.AccountManagementSession.InviteeEmailAddress;
            session.AccountManagementSession.InviteeEmailAddress = null;
            session.AccountManagementSession.AddUserStatus = null;
        }

        model.PersonUpdated = TempData["PersonUpdated"] == null ? null : TempData["PersonUpdated"].ToString();

        if (userAccount is null)
        {
            _logger.LogInformation("User authenticated but account could not be found");
        }
        else
        {
            var userOrg = userAccount.Organisations.FirstOrDefault();
            
            session.AccountManagementSession.OrganisationName = userOrg?.Name;
            session.AccountManagementSession.OrganisationType = userOrg?.OrganisationType;
            session.AccountManagementSession.BusinessAddress = new Address { Postcode = userOrg?.Postcode };

            model.UserName = $"{userAccount.FirstName} {userAccount.LastName}";
            model.Telephone = userAccount.Telephone;
            model.JobTitle = userAccount.JobTitle;
            model.CompanyName = userOrg?.Name;
            model.OrganisationAddress = string.Join(", ", new[] {
                userOrg?.SubBuildingName,
                userOrg?.BuildingNumber,
                userOrg?.BuildingName,
                userOrg?.Street,
                userOrg?.Town,
                userOrg?.County,
                userOrg?.Postcode,
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
            model.EnrolmentStatus = userAccount.EnrolmentStatus;
            
            var serviceRoleEnum = (ServiceRole)userAccount.ServiceRoleId;
            
            model.ServiceRoleKey = $"{serviceRoleEnum.ToString()}.{userAccount.RoleInOrganisation}";
            model.OrganisationType = userOrg?.OrganisationType;
            model.HasPermissionToChangeCompany = HasPermissionToChangeCompany(userAccount);
            model.IsRegulatorUser = IsRegulatorUser(userAccount);
            model.IsChangeRequestPending = userAccount.IsChangeRequestPending;
            model.IsAdmin = userAccount.RoleInOrganisation == PersonRole.Admin.ToString();
            model.ShowManageUserDetailChanges = await _featureManager.IsEnabledAsync(FeatureFlags.ManageUserDetailChanges);
        }

        await SaveSessionAndJourney(session, PagePath.ManageAccount, PagePath.ManageAccount);

        SetCustomBackLink(_urlOptions.LandingPageUrl);

        return View(nameof(ManageAccount), model);
    }

    [HttpGet]
    [Route(PagePath.CompanyDetailsCheck)]
    public async Task<ActionResult> CheckData()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var isDataMatching = await CompareDataAsync(session);

        if (isDataMatching)
        {
            return RedirectToAction(nameof(CompanyDetailsHaveNotChanged));
        }

        return RedirectToAction(nameof(ConfirmCompanyDetails));
    }

    [HttpGet]
    [Route(PagePath.CompanyDetailsHaveNotChanged)]
    public async Task<IActionResult> CompanyDetailsHaveNotChanged()
    {
        var userData = User.GetUserData();

        if (IsEmployeeUser(userData) || IsBasicAdmin(userData))
        {
            return NotFound();
        }

        if (!(User.IsApprovedPerson() || User.IsDelegatedPerson()))
        {
            return Unauthorized();
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var companiesHouseData = session.CompaniesHouseSession.CompaniesHouseData;

        session.AccountManagementSession.Journey.AddIfNotExists(PagePath.CompanyDetailsHaveNotChanged);

        SetBackLink(session, PagePath.CompanyDetailsHaveNotChanged);

        var companiesHouseChangeDetailsUrl = _urlOptions.CompanyHouseChangeRequestLink;

        var model = _mapper.Map<CompanyDetailsHaveNotChangedViewModel>(
            companiesHouseData,
            opts =>
                opts.Items["CompaniesHouseChangeDetailsUrl"] = companiesHouseChangeDetailsUrl);

        return View(model);
    }

    [HttpGet]
    [Route(PagePath.TeamMemberEmail)]
    public async Task<IActionResult> TeamMemberEmail()
    {
        var userData = User.GetUserData();

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (IsEmployeeUser(userData))
        {
            return NotFound();
        }

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
        var userData = User.GetUserData();

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (IsEmployeeUser(userData))
        {
            return NotFound();
        }

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
        var userData = User.GetUserData();

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (IsEmployeeUser(userData))
        {
            return NotFound();
        }

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
        return RedirectToAction("RemoveTeamMemberConfirmation", "AccountManagement");
    }

    [HttpGet]
    [Route(PagePath.RemoveTeamMember)]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    public async Task<IActionResult> RemoveTeamMemberConfirmation()
    {
        var userData = User.GetUserData();

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (IsEmployeeUser(userData))
        {
            return NotFound();
        }

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
        var result = await _facadeService.RemoveUserForOrganisation(personExternalId, organisationId, serviceRoleId);
        session.AccountManagementSession.RemoveUserStatus = result;

        return await SaveSessionAndRedirect(session, nameof(ManageAccount), PagePath.RemoveTeamMember, PagePath.ManageAccount);
    }

    /// <summary>
    /// Displays the "Declaration" page.
    /// </summary>
    /// <remarks>
    /// The page is only displayed when arriving from the "Check your details" page.
    /// If navigated to directly, the user is forwarded to an error page.
    /// </remarks>
    /// <param name="navigationToken">
    /// A value used to verify that the user came from the "Check your details" page.
    /// Its specific value doesn't matter, but it's compared to a copy stored in the session data as an extra layer of validation.
    /// </param>
    [HttpGet]
    [Route(PagePath.Declaration)]
    public async Task<IActionResult> Declaration()
    {
        var userData = User.GetUserData();

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (IsEmployeeUser(userData) || IsBasicAdmin(userData))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var editUserDetailsViewModel = new EditUserDetailsViewModel();

        if (userData.IsChangeRequestPending)
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }

        if (TempData[AmendedUserDetailsKey] != null)
        {
            try
            {
                editUserDetailsViewModel = JsonSerializer.Deserialize<EditUserDetailsViewModel>(TempData[AmendedUserDetailsKey] as string);
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception, "Deserialising NewUserDetails Failed.");
            }
        }

        SaveSessionAndJourney(session, PagePath.CheckYourDetails, PagePath.Declaration);
        SetBackLink(session, PagePath.Declaration);

        TempData.Keep(AmendedUserDetailsKey);
        TempData.Keep(NewUserDetailsKey);

        return View(nameof(Declaration), editUserDetailsViewModel);
    }


    [HttpPost]
    [Route(PagePath.Declaration, Name = "Declaration")]
    public async Task<IActionResult> DeclarationPost(EditUserDetailsViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var userData = User.GetUserData();


        var userDetailsDto = _mapper.Map<UpdateUserDetailsRequest>(model);
        var userOrg = userData.Organisations?.FirstOrDefault();
        Guid userOrgId = userOrg?.Id.Value ?? Guid.Empty;

        // check if change is only phone number
        if (IsApprovedOrDelegatedUser(userData))
        {
            var reponse = await _facadeService.UpdateUserDetailsAsync(userData.Id.Value, userOrgId, ServiceKey, userDetailsDto);
            if (reponse.HasApprovedOrDelegatedUserDetailsSentForApproval)
            {
                if (TempData[AmendedUserDetailsKey] != null) TempData.Remove(AmendedUserDetailsKey);
                // refresh the user data from the database
                var userAccount = await _facadeService.GetUserAccount();
                session.UserData = userAccount.User;
                await SaveSession(session);
                // update cookie  with the latest data
                await _claimsExtensionsWrapper.UpdateUserDataClaimsAndSignInAsync(userAccount.User);
                return RedirectToAction(nameof(DetailsChangeRequested));
            }
        }
        SaveSessionAndJourney(session, PagePath.CheckYourDetails, PagePath.Declaration);
        SetBackLink(session, PagePath.Declaration);
        return View(model);
    }

    [HttpGet]
    [Route(PagePath.WhatAreYourDetails)]
    public async Task<IActionResult> EditUserDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var userData = User.GetUserData();
        var model = _mapper.Map<EditUserDetailsViewModel>(userData);
        if (userData.IsChangeRequestPending)
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }

        if (TempData[AmendedUserDetailsKey] != null)
        {
            try
            {
                model = JsonSerializer.Deserialize<EditUserDetailsViewModel>(TempData[AmendedUserDetailsKey] as string);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Deserialising NewUserDetails Failed.");
            }
        }

        SaveSessionAndJourney(session, PagePath.ManageAccount, PagePath.WhatAreYourDetails);
        SetBackLink(session, PagePath.WhatAreYourDetails);

        return View(model);
    }

    [HttpPost]
    [Route(PagePath.WhatAreYourDetails)]
    public async Task<IActionResult> EditUserDetails(EditUserDetailsViewModel editUserDetailsViewModel)
    {
        // is there a modelstate error for job title, but job title never existed
        if (ModelState.ContainsKey(nameof(EditUserDetailsViewModel.JobTitle)) &&
            ModelState.FirstOrDefault(ms => ms.Key == nameof(EditUserDetailsViewModel.JobTitle)).Value.Errors.Any() &&
            !editUserDetailsViewModel.PropertyExists(m => m.OriginalJobTitle))
        {
            // if so, remove the error
            ModelState.Remove(nameof(EditUserDetailsViewModel.JobTitle));
        }

        // is there a modelstate error for telephone, but telephone never existed
        if (ModelState.ContainsKey(nameof(EditUserDetailsViewModel.Telephone)) &&
            ModelState.FirstOrDefault(ms => ms.Key == nameof(EditUserDetailsViewModel.Telephone)).Value.Errors.Any() &&
            !editUserDetailsViewModel.PropertyExists(m => m.OriginalTelephone))
        {
            // if so, remove the error
            ModelState.Remove(nameof(EditUserDetailsViewModel.Telephone));
        }

        if (!ModelState.IsValid)
        {
            await SetBackLink(PagePath.WhatAreYourDetails);
            return View(editUserDetailsViewModel);
        }

        //tries to add new temp data and if its already exists replace it with the new one
        if (!TempData.TryAdd(NewUserDetailsKey, JsonSerializer.Serialize(editUserDetailsViewModel)))
        {
            TempData[NewUserDetailsKey] = JsonSerializer.Serialize(editUserDetailsViewModel);
        }

        return RedirectToAction(nameof(PagePath.CheckYourDetails));
    }

    [HttpGet]
    [Route(PagePath.CheckYourDetails)]
    public async Task<IActionResult> CheckYourDetails()
    {

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var userData = User.GetUserData();

        if (userData.IsChangeRequestPending)
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }

        bool isUpdatable = false;
        var serviceRole = userData.ServiceRole ?? string.Empty;
        var roleInOrganisation = userData.RoleInOrganisation ?? string.Empty;

        var editUserDetailsViewModel = new EditUserDetailsViewModel();

        if (TempData[NewUserDetailsKey] != null)
        {
            try
            {
                editUserDetailsViewModel = JsonSerializer.Deserialize<EditUserDetailsViewModel>(TempData[NewUserDetailsKey] as string);
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception, "Deserialising NewUserDetails Failed.");
            }
        }

        var model = new EditUserDetailsViewModel
        {
            FirstName = editUserDetailsViewModel.FirstName ?? userData.FirstName,
            LastName = editUserDetailsViewModel.LastName ?? userData.LastName,
            JobTitle = editUserDetailsViewModel.JobTitle ?? userData.JobTitle,
            Telephone = editUserDetailsViewModel.Telephone ?? userData.Telephone,
            OriginalFirstName = editUserDetailsViewModel.OriginalFirstName ?? string.Empty,
            OriginalLastName = editUserDetailsViewModel.OriginalLastName ?? string.Empty,
            OriginalJobTitle = editUserDetailsViewModel.OriginalJobTitle ?? string.Empty,
            OriginalTelephone = editUserDetailsViewModel.OriginalTelephone ?? string.Empty
        };

        isUpdatable = SetUpdatableValue(isUpdatable, serviceRole, roleInOrganisation, model);

        SaveSessionAndJourney(session, PagePath.WhatAreYourDetails, PagePath.CheckYourDetails);
        SetBackLink(PagePath.CheckYourDetails);

        if (TempData[AmendedUserDetailsKey] == null)
        {
            TempData.Add(AmendedUserDetailsKey, JsonSerializer.Serialize(editUserDetailsViewModel));
        }

        ViewBag.IsUpdatable = isUpdatable;

        TempData.Keep(AmendedUserDetailsKey);
        TempData.Keep(NewUserDetailsKey);

        return View(model);
    }

    [HttpPost]
    [Route(PagePath.CheckYourDetails)]
    public async Task<IActionResult> CheckYourDetails(EditUserDetailsViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var userData = User.GetUserData();
        var serviceRole = userData.ServiceRole ?? string.Empty;
        var roleInOrganisation = userData.RoleInOrganisation ?? string.Empty;
        bool isUpdatable = false;
        var userDetailsDto = _mapper.Map<UpdateUserDetailsRequest>(model);
        var userOrg = userData.Organisations?.FirstOrDefault();

        // check if change is only phone number
        if (
            model.FirstName == model.OriginalFirstName &&
            model.LastName == model.OriginalLastName &&
            model.JobTitle == model.OriginalJobTitle &&
            model.Telephone != model.OriginalTelephone)
        {
            var reponse = await _facadeService.UpdateUserDetailsAsync(
                userData.Id.Value,
                userOrg?.Id.Value ?? Guid.Empty,
                ServiceKey,
                userDetailsDto);

            if (reponse.HasTelephoneOnlyUpdated)
            {
                if (TempData[AmendedUserDetailsKey] != null) TempData.Remove(AmendedUserDetailsKey);
                // refresh the user data from the database
                var userAccount = await _facadeService.GetUserAccount();
                session.UserData = userAccount.User;
                await SaveSession(session);
                // update cookie  with the latest data
                await _claimsExtensionsWrapper.UpdateUserDataClaimsAndSignInAsync(userAccount.User);
                return RedirectToAction(nameof(PagePath.UpdateDetailsConfirmation));
            }
        }
        else if (IsBasicUser(userData))
        {
            var reponse = await _facadeService.UpdateUserDetailsAsync(
                userData.Id.Value,
                userOrg?.Id.Value ?? Guid.Empty,
                ServiceKey,
                userDetailsDto);

            if (reponse.HasBasicUserDetailsUpdated)
            {
                if (TempData[AmendedUserDetailsKey] != null) TempData.Remove(AmendedUserDetailsKey);
                // refresh the user data from the database
                var userAccount = await _facadeService.GetUserAccount();
                session.UserData = userAccount.User;
                await SaveSession(session);
                // update cookie  with the latest data
                await _claimsExtensionsWrapper.UpdateUserDataClaimsAndSignInAsync(userAccount.User);
                return RedirectToAction(nameof(PagePath.UpdateDetailsConfirmation));
            }
        }
        else if (IsApprovedOrDelegatedUser(userData))
        {
            if (TempData[AmendedUserDetailsKey] == null)
            {
                TempData.Add(AmendedUserDetailsKey, JsonSerializer.Serialize(model));
            }
            return RedirectToAction(nameof(PagePath.Declaration));
        }

        SaveSessionAndJourney(session, PagePath.WhatAreYourDetails, PagePath.CheckYourDetails);
        SetBackLink(PagePath.CheckYourDetails);
        isUpdatable = SetUpdatableValue(isUpdatable, serviceRole, roleInOrganisation, model);
        ViewBag.IsUpdatable = isUpdatable;
        return View(model);
    }

    [HttpGet]
    [Route(PagePath.UpdateDetailsConfirmation)]
    public async Task<IActionResult> UpdateDetailsConfirmation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var userData = User.GetUserData();

        if (userData.IsChangeRequestPending)
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.Forbidden
            });
        }

        var changedDateAt = DateTime.UtcNow;
        var model = new UpdateDetailsConfirmationViewModel
        {
            Username = $"{session.UserData.FirstName} {session.UserData.LastName}",
            UpdatedDatetime = changedDateAt.UtcToGmt()
        };

        return View(model);
    }

    [HttpGet]
    [Route(PagePath.DetailsChangeRequestedNotification)]
    public async Task<IActionResult> DetailsChangeRequested()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var changedDateAt = DateTime.UtcNow;
        var model = new DetailsChangeRequestedViewModel
        {
            Username = $"{session.UserData.FirstName} {session.UserData.LastName}",
            UpdatedDatetime = changedDateAt.UtcToGmt()
        };

        return View(nameof(DetailsChangeRequested), model);
    }

    [HttpGet]
    [Route(PagePath.ConfirmCompanyDetails)]
    public async Task<IActionResult> ConfirmCompanyDetails()
    {
        var userData = User.GetUserData();

        if (IsEmployeeUser(userData) || IsBasicAdmin(userData))
        {
            return NotFound();
        }

        if (!(User.IsApprovedPerson() || User.IsDelegatedPerson()))
        {
            return Unauthorized();
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SaveSessionAndJourney(session, PagePath.ManageAccount, PagePath.ConfirmCompanyDetails);
        SetBackLink(session, PagePath.ConfirmCompanyDetails);

        var companiesHouseData = session.CompaniesHouseSession.CompaniesHouseData;

        if (companiesHouseData?.Organisation?.RegisteredOffice is null)
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.NotFound
            });
        }

        var viewModel = _mapper.Map<ConfirmCompanyDetailsViewModel>(companiesHouseData);
        viewModel.ExternalCompanyHouseChangeRequestLink = _urlOptions.CompanyHouseChangeRequestLink;

        return View(viewModel);
    }

    [HttpGet]
    [Route(PagePath.CompanyDetailsUpdated)]
    public async Task<IActionResult> CompanyDetailsUpdated()
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        TempData.TryGetValue("OrganisationDetailsUpdatedTime", out var changeDate);
        var model = new CompanyDetailsUpdatedViewModel
        {
            UserName = $"{session.UserData.FirstName} {session.UserData.LastName}",
            ChangeDate = (DateTime)changeDate,
        };

        TempData.Keep("OrganisationDetailsUpdatedTime");
        return View(nameof(CompanyDetailsUpdated), model);
    }

    [HttpGet]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress()
    {
        if (IsCompaniesHouseUser() || !await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        session.AccountManagementSession.Journey.AddIfNotExists(PagePath.BusinessAddressPostcode);
        session.AccountManagementSession.Journey.AddIfNotExists(PagePath.BusinessAddress);
        session.AccountManagementSession.Journey.RemoveAll(x => x == PagePath.SelectBusinessAddress);
        SetBackLink(session, PagePath.BusinessAddress, LocalizerName.UpdateBusinessAddressBackAriaLabel);

        var model = new BusinessAddressViewModel
        {
            SubBuildingName = session.AccountManagementSession.BusinessAddress?.SubBuildingName,
            BuildingName = session.AccountManagementSession.BusinessAddress?.BuildingName,
            BuildingNumber = session.AccountManagementSession.BusinessAddress?.BuildingNumber,
            Street = session.AccountManagementSession.BusinessAddress?.Street,
            Town = session.AccountManagementSession.BusinessAddress?.Town,
            County = session.AccountManagementSession.BusinessAddress?.County,
            Postcode = session.AccountManagementSession.BusinessAddress?.Postcode
        };

        if (TempData.ContainsKey(PostcodeLookupFailedKey))
        {
            model.ShowWarning = true;
            TempData[PostcodeLookupFailedKey] = true;
        }
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        return View(nameof(BusinessAddress), model);
    }

    [HttpPost]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.BusinessAddress)]
    public async Task<IActionResult> BusinessAddress(BusinessAddressViewModel model)
    {
        if (IsCompaniesHouseUser() || !await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.BusinessAddress, LocalizerName.UpdateBusinessAddressBackAriaLabel);

            if (TempData.ContainsKey(PostcodeLookupFailedKey))
            {
                model.ShowWarning = true;
                TempData[PostcodeLookupFailedKey] = true;
            }

            return View(model);
        }

        session.AccountManagementSession.BusinessAddress = new Address
        {
            SubBuildingName = model.SubBuildingName,
            BuildingName = model.BuildingName,
            BuildingNumber = model.BuildingNumber,
            Street = model.Street,
            Town = model.Town,
            County = model.County,
            Postcode = model.Postcode
        };
        TempData.Remove(PostcodeLookupFailedKey);

        return await SaveSessionAndRedirect(session, nameof(NonCompaniesHouseUkNation), PagePath.BusinessAddress, PagePath.NonCompaniesHouseUkNation);
    }

    [HttpGet]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.NonCompaniesHouseUkNation)]
    public async Task<IActionResult> NonCompaniesHouseUkNation()
    {
        if (IsCompaniesHouseUser() || !await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }
    
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetBackLink(session, PagePath.UpdateCompanyAddress, LocalizerName.NonCompaniesHouseUkNation);

        var viewModel = new NonCompaniesHouseUkNationViewModel
        {
            UkNation = (Constants.Enums.UkNation?)session.AccountManagementSession.UkNation
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.NonCompaniesHouseUkNation)]
    public async Task<IActionResult> NonCompaniesHouseUkNation(NonCompaniesHouseUkNationViewModel model)
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        SetCustomBackLink(PagePath.BusinessAddress, false);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        session.AccountManagementSession.UkNation = (Core.Enums.Nation)model.UkNation;

        return await SaveSessionAndRedirect(session, nameof(CheckCompanyDetails), PagePath.NonCompaniesHouseUkNation, PagePath.CheckCompanyDetails);
    }

    /// <summary>
    /// Displays a page with the updated data from the "choose your nation" page
    /// for the user to confirm they have entered the correct information
    /// </summary>
    /// <returns>async IActionResult containing the view</returns>
    [HttpGet]
    [Route(PagePath.CheckCompaniesHouseDetails)]
    public async Task<IActionResult> CheckCompaniesHouseDetails()
    {
        var userData = User.GetUserData();

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (IsEmployeeUser(userData) || IsBasicAdmin(userData))
        {
            return NotFound();
        }

        // must be approved or delegated user
        if (!(User.IsApprovedPerson() || User.IsDelegatedPerson()))
        {
            return Unauthorized();
        }

        // deserialize data from TempStorage
        var viewModel = JsonSerializer.Deserialize<CheckYourOrganisationDetailsViewModel>(
            TempData[CheckYourOrganisationDetailsKey] as string);

        // keep the data for one more request cycle, just in case the page is refreshed
        TempData.Keep(CheckYourOrganisationDetailsKey);

        await SaveSessionAndJourney(
            session,
            PagePath.UkNation,
            PagePath.CheckCompaniesHouseDetails);
        SetBackLink(session, PagePath.CheckCompaniesHouseDetails);

        return View(viewModel);
    }

    /// <summary>
    /// Displays a page with the updated data from the "choose your nation" page
    /// for the user to confirm they have entered the correct information
    /// </summary>
    /// <returns>async IActionResult containing the redirect to the next page</returns>
    [HttpPost]
    [Route(PagePath.CheckCompaniesHouseDetails)]
    public async Task<IActionResult> CheckCompaniesHouseDetails(CheckYourOrganisationDetailsViewModel viewModel)
    {
        // must be approved or delegated user
        if (!(User.IsApprovedPerson() || User.IsDelegatedPerson()))
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            TempData[CheckYourOrganisationDetailsKey] = JsonSerializer.Serialize(viewModel);
            return View(viewModel);
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        // map to a dto that can be used to update organisation details, including the nation id
        var organisation = _mapper.Map<OrganisationUpdateDto>(
            session.CompaniesHouseSession.CompaniesHouseData.Organisation,
            options =>
                options.Items[CompaniesHouseResponseProfile.NationIdKey] = (int)viewModel.UkNation);

        await _facadeService.UpdateOrganisationDetails(viewModel.OrganisationId, organisation);

        TempData.Remove(CheckYourOrganisationDetailsKey);

        // refresh the user data from the database
        var userAccount = await _facadeService.GetUserAccount();
        session.UserData = userAccount.User;
        await SaveSession(session);

        // need to do this so that the cookie updates with the latest data
        await _claimsExtensionsWrapper.UpdateUserDataClaimsAndSignInAsync(userAccount.User);

        // save the date/time that the update was performed for the next page
        var changedDateAt = DateTime.UtcNow;
        TempData[OrganisationDetailsUpdatedTimeKey] = changedDateAt.UtcToGmt();

        return RedirectToAction(nameof(CompanyDetailsUpdated));
    }

    [HttpPost]
    [Route(PagePath.ConfirmCompanyDetails)]
    public async Task<IActionResult> ConfirmDetailsOfTheCompany()
    {
        return RedirectToAction(nameof(UkNation));
    }

    [HttpGet]
    [Route(PagePath.UkNation)]
    public async Task<IActionResult> UkNation()
    {
        var userData = User.GetUserData();

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (IsEmployeeUser(userData) || IsBasicAdmin(userData))
        {
            return NotFound();
        }

        await SaveSessionAndJourney(session, PagePath.ConfirmCompanyDetails, PagePath.UkNation);
        SetBackLink(session, PagePath.UkNation);

        var viewModel = new UkNationViewModel();

        // see if there has been previously stored data
        if (TempData[CheckYourOrganisationDetailsKey] is string checkYourOrgDetails &&
            !string.IsNullOrWhiteSpace(TempData[CheckYourOrganisationDetailsKey] as string))
        {
            viewModel.UkNation = JsonSerializer.Deserialize<CheckYourOrganisationDetailsViewModel>(checkYourOrgDetails).UkNation;
            TempData.Keep(CheckYourOrganisationDetailsKey);
        }

        return View(viewModel);
    }

    [HttpPost]
    [Route(PagePath.UkNation)]
    public async Task<IActionResult> UkNation(UkNationViewModel model)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var userData = User.GetUserData();

        SetCustomBackLink(PagePath.ConfirmCompanyDetails, false);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var addressDto = session.CompaniesHouseSession.CompaniesHouseData.Organisation
            .RegisteredOffice;

        var address = _mapper.Map<AddressViewModel>(addressDto);

        var checkYourOrganisationModel = new CheckYourOrganisationDetailsViewModel
        {
            OrganisationId = userData.Organisations.FirstOrDefault()?.Id ?? Guid.Empty,
            Address = string.Join(", ", address.AddressFields.Where(field => !string.IsNullOrWhiteSpace(field))),
            TradingName = session.CompaniesHouseSession?.CompaniesHouseData?.Organisation?.Name,
            UkNation = model.UkNation.Value
        };
        TempData[CheckYourOrganisationDetailsKey] = JsonSerializer.Serialize(checkYourOrganisationModel);

        return RedirectToAction(nameof(CheckCompaniesHouseDetails));
    }

    [HttpGet]
    [Route(PagePath.ChangeCompanyDetails)]
    public async Task<IActionResult> ChangeCompanyDetails()
    {
        SetCustomBackLink(PagePath.ManageAccount, false);
        return View();
    }

    [HttpGet]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    [Route(PagePath.UpdateCompanyName)]
    public async Task<IActionResult> UpdateCompanyName()
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        YesNoAnswer? isUpdateName = null;
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (session != null)
        {
            session.AccountManagementSession.Journey.AddIfNotExists(PagePath.ManageAccount);

            await SaveSessionAndJourney(session, PagePath.ManageAccount, PagePath.UpdateCompanyName);
            SetBackLink(session, PagePath.UpdateCompanyName, LocalizerName.UpdateOrgNameBackAriaLabel);

            if (session.AccountManagementSession?.OrganisationType == OrganisationType.CompaniesHouseCompany)
            {
                return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
                {
                    statusCode = (int)HttpStatusCode.NotFound
                });
            }

            isUpdateName = session.AccountManagementSession.IsUpdateCompanyName switch
            {
                true => YesNoAnswer.Yes,
                false => YesNoAnswer.No,
                _ => null
            };
        }

        return View(new UpdateCompanyNameViewModel
        {
            IsUpdateCompanyName = isUpdateName
        });
    }

    [HttpPost]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.UpdateCompanyName)]
    public async Task<IActionResult> UpdateCompanyName(UpdateCompanyNameViewModel model)
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UpdateCompanyName, LocalizerName.UpdateOrgNameBackAriaLabel);
            return View(model);
        }

        session.AccountManagementSession.IsUpdateCompanyName = model.IsUpdateCompanyName == YesNoAnswer.Yes;

        if (session.AccountManagementSession.IsUpdateCompanyName == true)
        {
            return await SaveSessionAndRedirect(session, nameof(CompanyName), PagePath.UpdateCompanyName, PagePath.CompanyName);
        }
        else
        {
            return await SaveSessionAndRedirect(session, nameof(UpdateCompanyAddress), PagePath.UpdateCompanyName, PagePath.UpdateCompanyAddress);
        }
    }

    [HttpGet]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    [Route(PagePath.UpdateCompanyAddress)]
    public async Task<IActionResult> UpdateCompanyAddress()
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        YesNoAnswer? isUpdateAddress = null;
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (session != null)
        {
            if (session.AccountManagementSession?.OrganisationType == OrganisationType.CompaniesHouseCompany)
            {
                return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
                {
                    statusCode = (int)HttpStatusCode.NotFound
                });
            }

            isUpdateAddress = session.AccountManagementSession.IsUpdateCompanyAddress switch
            {
                true => YesNoAnswer.Yes,
                false => YesNoAnswer.No,
                _ => null
            };

            SetBackLink(session, PagePath.UpdateCompanyAddress, LocalizerName.UpdateOrgAddressBackAriaLabel);
        }

        return View(new UpdateCompanyAddressViewModel
        {
            IsUpdateCompanyAddress = isUpdateAddress
        });
    }

    [HttpPost]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.UpdateCompanyAddress)]
    public async Task<IActionResult> UpdateCompanyAddress(UpdateCompanyAddressViewModel model)
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.UpdateCompanyAddress, LocalizerName.UpdateOrgAddressBackAriaLabel);
            return View(model);
        }

        session.AccountManagementSession.IsUpdateCompanyAddress = model.IsUpdateCompanyAddress == YesNoAnswer.Yes;

        if (session.AccountManagementSession.IsUpdateCompanyAddress == true)
        {
            return await SaveSessionAndRedirect(session, nameof(BusinessAddressPostcode), PagePath.UpdateCompanyAddress, PagePath.BusinessAddressPostcode);
        }
        else
        {
            if (session.AccountManagementSession.IsUpdateCompanyName == true)
            {
                return await SaveSessionAndRedirect(session, nameof(CheckCompanyDetails), PagePath.UpdateCompanyAddress, PagePath.CheckCompanyDetails);
            }
            else
            {
                return await SaveSessionAndRedirect(session, nameof(ManageAccount), PagePath.UpdateCompanyAddress, string.Empty);
            }
        }
    }

    [HttpGet]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    [Route(PagePath.BusinessAddressPostcode)]
    public async Task<IActionResult> BusinessAddressPostcode()
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (session != null)
        {
            if (session.AccountManagementSession?.OrganisationType == OrganisationType.CompaniesHouseCompany)
            {
                return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
                {
                    statusCode = (int)HttpStatusCode.NotFound
                });
            }
            SetBackLink(session, PagePath.BusinessAddressPostcode, LocalizerName.BusinessPostcodeBackAriaLabel);
        }

        var viewModel = new BusinessAddressPostcodeViewModel()
        {
            Postcode = session?.AccountManagementSession?.BusinessAddress?.Postcode,
        };
        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.BusinessAddressPostcode)]
    public async Task<IActionResult> BusinessAddressPostcode(BusinessAddressPostcodeViewModel model)
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.BusinessAddressPostcode, LocalizerName.BusinessPostcodeBackAriaLabel);

            return View(model);
        }

        session.AccountManagementSession.BusinessAddress = new Address { Postcode = model.Postcode };

        AddressList? addressList = null;

        try
        {
            addressList = await _facadeService.GetAddressListByPostcodeAsync(session.AccountManagementSession.BusinessAddress.Postcode);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve addresses for postcode: {BusinessAddressPostcode}", session.AccountManagementSession.BusinessAddress.Postcode);
        }

        if (addressList == null)
        {
            TempData[PostcodeLookupFailedKey] = true;
            return await SaveSessionAndRedirect(session, nameof(BusinessAddress), PagePath.BusinessAddressPostcode, PagePath.BusinessAddress);
        }

        return await SaveSessionAndRedirect(session, nameof(SelectBusinessAddress), PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress);
    }

    [HttpGet]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.SelectBusinessAddress)]
    public async Task<IActionResult> SelectBusinessAddress()
    {
        if (IsCompaniesHouseUser() || !await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (session == null)
        {
            TempData[PostcodeLookupFailedKey] = true;
            _logger.LogWarning("Session is null, redirecting to BusinessAddress page.");
            return RedirectToAction(nameof(BusinessAddress));
        }

        var viewModel = new SelectBusinessAddressViewModel()
        {
            Postcode = session?.AccountManagementSession?.BusinessAddress?.Postcode,
        };

        SetBackLink(session, PagePath.SelectBusinessAddress, LocalizerName.BusinessAddressBackAriaLabel);
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);

        AddressList? addressList = null;
        var addressLookupFailed = false;

        try
        {
            if (string.IsNullOrWhiteSpace(session.AccountManagementSession?.BusinessAddress?.Postcode))
            {
                _logger.LogWarning("Postcode is missing in session, redirecting to BusinessAddress page.");
                addressLookupFailed = true;
            }
            else
            {
                addressList = await _facadeService.GetAddressListByPostcodeAsync(session.AccountManagementSession.BusinessAddress.Postcode);

                if (addressList == null || !addressList.Addresses.Any())
                {
                    _logger.LogWarning("No addresses found for postcode: {Postcode}", session.AccountManagementSession.BusinessAddress.Postcode);
                    addressLookupFailed = true;
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve addresses for postcode: {Postcode}",
                             session?.AccountManagementSession?.BusinessAddress?.Postcode);
            addressLookupFailed = true;
        }

        if (addressLookupFailed)
        {
            TempData[PostcodeLookupFailedKey] = true;
            return RedirectToAction(nameof(BusinessAddress));
        }

        viewModel.SetAddressItems(addressList.Addresses, viewModel.SelectedListIndex!);
        session.AccountManagementSession?.AddressesForPostcode.Clear();
        session.AccountManagementSession?.AddressesForPostcode.AddRange(addressList.Addresses);
        await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        TempData.Remove(PostcodeLookupFailedKey);

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    [Route(PagePath.SelectBusinessAddress)]
    public async Task<IActionResult> SelectBusinessAddress(SelectBusinessAddressViewModel model)
    {
        if (IsCompaniesHouseUser() || !await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var indexParseResult = int.TryParse(model.SelectedListIndex, out var index);
        if (ModelState.IsValid &&
            (!indexParseResult || index < 0 || index >= session?.AccountManagementSession?.AddressesForPostcode.Count))
        {
            ModelState.AddModelError(nameof(model.SelectedListIndex), "SelectBusinessAddress.ErrorMessage");
        }

        if (!ModelState.IsValid || session == null)
        {
            if (session == null)
            {
                TempData[PostcodeLookupFailedKey] = true;
                return RedirectToAction(nameof(BusinessAddress));
            }

            SetBackLink(session, PagePath.SelectBusinessAddress, LocalizerName.BusinessAddressBackAriaLabel);
            model.Postcode = session.AccountManagementSession?.BusinessAddress?.Postcode;

            AddressList? addressList = null;
            var addressLookupFailed = false;

            try
            {
                addressList = await _facadeService.GetAddressListByPostcodeAsync(session.AccountManagementSession.BusinessAddress.Postcode);

                if (addressList == null || !addressList.Addresses.Any())
                {
                    addressLookupFailed = true;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to retrieve addresses for postcode: {BusinessAddressPostcode}",
                                 session?.AccountManagementSession?.BusinessAddress?.Postcode);
                addressLookupFailed = true;
            }

            if (addressLookupFailed)
            {
                await SaveSession(session);
                TempData[PostcodeLookupFailedKey] = true;
                return RedirectToAction(nameof(BusinessAddress));
            }

            model.SetAddressItems(addressList.Addresses, model.SelectedListIndex);
            return View(model);
        }

        model.SetAddressItems(session.AccountManagementSession?.AddressesForPostcode, model.SelectedListIndex!);
        session.AccountManagementSession ??= new AccountManagementSession();
        session.AccountManagementSession.BusinessAddress = session.AccountManagementSession?.AddressesForPostcode[index];

        return await SaveSessionAndRedirect(session, nameof(NonCompaniesHouseUkNation), PagePath.SelectBusinessAddress, PagePath.NonCompaniesHouseUkNation);
    }

    [HttpGet]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    [Route(PagePath.CompanyName)]
    public async Task<IActionResult> CompanyName()
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (session != null)
        {
            if (session.UserData?.Organisations.Count == 0) session.UserData = User.GetUserData();
            if (session.UserData?.Organisations[0]?.OrganisationType == OrganisationType.CompaniesHouseCompany)
            {
                return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
                {
                    statusCode = (int)HttpStatusCode.NotFound
                });
            }
            else
            {
                session.AccountManagementSession.Journey.AddIfNotExists(PagePath.UpdateCompanyName);

                await SaveSessionAndJourney(session, PagePath.UpdateCompanyName, PagePath.CompanyName);

                SetBackLink(session, PagePath.CompanyName, LocalizerName.CompanyNameBackAriaLabel);
            }
        }
        var viewModel = new OrganisationNameViewModel()
        {
            OrganisationName = session?.AccountManagementSession?.OrganisationName ?? session?.UserData?.Organisations[0].Name,
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
    [Route(PagePath.CompanyName)]
    public async Task<IActionResult> CompanyName(OrganisationNameViewModel model)
    {
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

        if (!ModelState.IsValid)
        {
            SetBackLink(session, PagePath.CompanyName, LocalizerName.CompanyNameBackAriaLabel);
            return View(model);
        }
        else
        {
            session.AccountManagementSession.OrganisationName = model.OrganisationName;

            return await SaveSessionAndRedirect(session, nameof(UpdateCompanyAddress), PagePath.CompanyName, PagePath.UpdateCompanyAddress);
        }
    }

    [HttpGet]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.CheckCompanyDetails)]
    public async Task<IActionResult> CheckCompanyDetails()
    {
        if (IsCompaniesHouseUser() || !await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        if (session?.AccountManagementSession == null || !session.AccountManagementSession.Journey.Any())
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.NotFound
            });
        }

        var lastJourneyPage = session.AccountManagementSession.Journey[session.AccountManagementSession.Journey.Count - 2];

        switch (lastJourneyPage)
        {
            case PagePath.UpdateCompanyAddress:
            case PagePath.NonCompaniesHouseUkNation:
                SetBackLink(session, PagePath.CheckCompanyDetails, "Back");
                break;
            default:
                return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
                {
                    statusCode = (int)HttpStatusCode.NotFound
                });
                break;
        }

        var model = new CheckCompanyDetailsViewModel
        {
            IsUpdateCompanyName = session.AccountManagementSession.IsUpdateCompanyName == true,
            IsUpdateCompanyAddress = session.AccountManagementSession.IsUpdateCompanyAddress == true,
            Name = session.AccountManagementSession.OrganisationName,
            Address = session.AccountManagementSession.BusinessAddress?.SingleLineAddress,
            UKNation = session.AccountManagementSession.UkNation.ToString()
        };

        return View(nameof(CheckCompanyDetails), model);
    }

    [HttpPost]
    [Authorize(Policy = "IsEmployeeOrBasicAdmin")]
    [Route(PagePath.CheckCompanyDetails)]
    public async Task<IActionResult> CheckCompanyDetailsPost()
    {
        if (IsCompaniesHouseUser() || !await _featureManager.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        if (session == null)
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.NotFound
            });
        }

        var userData = User.GetUserData();

        var organisationData = userData.Organisations.FirstOrDefault();
        if (organisationData == null)
        {
            throw new InvalidOperationException(nameof(organisationData));
        }

        try
        {
            OrganisationUpdateDto organisation;

            if (session.AccountManagementSession.IsUpdateCompanyName == true && session.AccountManagementSession.IsUpdateCompanyAddress == true)
            {
                organisation = new OrganisationUpdateDto
                {
                    Name = session.AccountManagementSession.OrganisationName,
                    BuildingName = session.AccountManagementSession.BusinessAddress.BuildingName,
                    BuildingNumber = session.AccountManagementSession.BusinessAddress.BuildingNumber,
                    Country = session.AccountManagementSession.BusinessAddress.BuildingNumber,
                    County = session.AccountManagementSession.BusinessAddress.County,
                    DependentLocality = session.AccountManagementSession.BusinessAddress.DependentLocality,
                    Locality = session.AccountManagementSession.BusinessAddress.Locality,
                    Postcode = session.AccountManagementSession.BusinessAddress.Postcode,
                    Street = session.AccountManagementSession.BusinessAddress.Street,
                    SubBuildingName = session.AccountManagementSession.BusinessAddress.SubBuildingName,
                    Town = session.AccountManagementSession.BusinessAddress.Town,
                    NationId = (int)session.AccountManagementSession.UkNation,
                };
            }
            else if (session.AccountManagementSession.IsUpdateCompanyName == true)
            {
                organisation = new OrganisationUpdateDto
                {
                    Name = session.AccountManagementSession.OrganisationName,

                    BuildingName = userData.Organisations[0].BuildingName,
                    BuildingNumber = userData.Organisations[0].BuildingNumber,
                    Country = userData.Organisations[0].BuildingNumber,
                    County = userData.Organisations[0].County,
                    DependentLocality = userData.Organisations[0].DependentLocality,
                    Locality = userData.Organisations[0].Locality,
                    Postcode = userData.Organisations[0].Postcode,
                    Street = userData.Organisations[0].Street,
                    SubBuildingName = userData.Organisations[0].SubBuildingName,
                    Town = userData.Organisations[0].Town,
                    NationId = userData.Organisations[0].NationId.Value,
                };
            }
            else if (session.AccountManagementSession.IsUpdateCompanyAddress == true)
            {
                organisation = new OrganisationUpdateDto
                {
                    Name = userData.Organisations[0].Name,
                    BuildingName = session.AccountManagementSession.BusinessAddress.BuildingName,
                    BuildingNumber = session.AccountManagementSession.BusinessAddress.BuildingNumber,
                    Country = session.AccountManagementSession.BusinessAddress.BuildingNumber,
                    County = session.AccountManagementSession.BusinessAddress.County,
                    DependentLocality = session.AccountManagementSession.BusinessAddress.DependentLocality,
                    Locality = session.AccountManagementSession.BusinessAddress.Locality,
                    Postcode = session.AccountManagementSession.BusinessAddress.Postcode,
                    Street = session.AccountManagementSession.BusinessAddress.Street,
                    SubBuildingName = session.AccountManagementSession.BusinessAddress.SubBuildingName,
                    Town = session.AccountManagementSession.BusinessAddress.Town,
                    NationId = (int)session.AccountManagementSession.UkNation,
                };
            }
            else
            {
                throw new InvalidOperationException(nameof(session.AccountManagementSession));
            }


            await _facadeService.UpdateOrganisationDetails(organisationData.Id.Value, organisation);

        }
        catch (Exception)
        {
            return RedirectToAction(PagePath.Error, nameof(ErrorController.Error), new
            {
                statusCode = (int)HttpStatusCode.InternalServerError
            });
        }

        TempData.Remove(CheckYourOrganisationDetailsKey);

        // refresh the user data from the database
        var userAccount = await _facadeService.GetUserAccount();
        session.UserData = userAccount.User;

        // Clear Session Data
        session.AccountManagementSession.OrganisationName = null;
        session.AccountManagementSession.BusinessAddress = null;
        session.AccountManagementSession.UkNation = null;

        // need to do this so that the cookie updates with the latest data
        await _claimsExtensionsWrapper.UpdateUserDataClaimsAndSignInAsync(userAccount.User);

        // save the date/time that the update was performed for the next page
        var changedDateAt = DateTime.UtcNow;
        TempData[OrganisationDetailsUpdatedTimeKey] = changedDateAt.UtcToGmt();

        return await SaveSessionAndRedirect(session, nameof(CompanyDetailsUpdated), PagePath.CheckCompanyDetails, PagePath.CompanyDetailsUpdated);
    }

    private bool IsCompaniesHouseUser()
    {
        var userData = User.GetUserData();

        var organisationData = userData.Organisations.FirstOrDefault();

        if (organisationData == null)
        {
            throw new InvalidOperationException(nameof(organisationData));
        }

        return organisationData.CompaniesHouseNumber != null;
    }

    private async Task<bool> CompareDataAsync(JourneySession session)
    {
        var userData = User.GetUserData();

        var organisationData = userData.Organisations.FirstOrDefault();

        if (organisationData == null)
        {
            throw new InvalidOperationException(nameof(organisationData));
        }

        var companiesHouseData = await _facadeService.GetCompaniesHouseResponseAsync(organisationData.CompaniesHouseNumber);
        session.CompaniesHouseSession.CompaniesHouseData = companiesHouseData;

        await SaveSession(session);

        return companiesHouseData != null &&
               organisationData.Name == companiesHouseData.Organisation.Name &&
               organisationData.TradingName == companiesHouseData.Organisation.TradingName &&
               organisationData.SubBuildingName == companiesHouseData.Organisation.RegisteredOffice.SubBuildingName &&
               organisationData.BuildingName == companiesHouseData.Organisation.RegisteredOffice.BuildingName &&
               organisationData.BuildingNumber == companiesHouseData.Organisation.RegisteredOffice.BuildingNumber &&
               organisationData.Street == companiesHouseData.Organisation.RegisteredOffice.Street &&
               organisationData.Locality == companiesHouseData.Organisation.RegisteredOffice.Locality &&
               organisationData.DependentLocality == companiesHouseData.Organisation.RegisteredOffice.DependentLocality &&
               organisationData.Town == companiesHouseData.Organisation.RegisteredOffice.Town &&
               organisationData.County == companiesHouseData.Organisation.RegisteredOffice.County &&
               organisationData.Country == companiesHouseData.Organisation.RegisteredOffice.Country.Name &&
               organisationData.Postcode == companiesHouseData.Organisation.RegisteredOffice.Postcode;
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

        session.AccountManagementSession.Journey.AddIfNotExists(destinationPagePath);

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

    private async Task SetBackLink(string currentPagePath)
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        ViewBag.BackLinkToDisplay = session.AccountManagementSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
    }

    private void SetBackLink(JourneySession session, string currentPagePath)
    {
        ViewBag.BackLinkToDisplay = session.AccountManagementSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;

    }

    private void SetBackLink(JourneySession session, string currentPagePath, string backAriaLocalizerName)
    {
        ViewBag.BackLinkToDisplay = session.AccountManagementSession.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
        ViewBag.BackAriaLocalizerName = backAriaLocalizerName;
    }

    private void SetCustomBackLink(string pagePath, bool showCustomBackLabel = true)
    {
        if (showCustomBackLabel)
        {
            ViewBag.CustomBackLinkToDisplay = pagePath;
        }
        else
        {
            ViewBag.BackLinkToDisplay = pagePath;
        }
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

    private static bool IsBasicUser(UserData userData) =>
       userData.ServiceRoleId == (int)Core.Enums.ServiceRole.Basic &&
        (userData.RoleInOrganisation == PersonRole.Employee.ToString() ||
        userData.RoleInOrganisation == PersonRole.Admin.ToString());

    private static bool HasPermissionToChangeCompany(UserData userData)
    {
        return userData.ServiceRoleId is (int)ServiceRole.Approved or (int)ServiceRole.Delegated;
    }

    private static bool IsApprovedOrDelegatedUser(UserData userData)
    {
        var roleInOrganisation = userData.RoleInOrganisation;
        if ((userData.ServiceRoleId == (int)Core.Enums.ServiceRole.Approved || userData.ServiceRoleId == (int)Core.Enums.ServiceRole.Delegated)
            && !string.IsNullOrEmpty(roleInOrganisation) && (roleInOrganisation == PersonRole.Admin.ToString() || roleInOrganisation == PersonRole.Employee.ToString()))
        {
            return true;
        }
        return false;
    }

    private static bool SetUpdatableValue(bool isUpdatable, string serviceRole, string roleInOrganisation, EditUserDetailsViewModel model)
    {
        if (serviceRole.ToLower() == ServiceRoles.BasicUser.ToLower()
           && (roleInOrganisation == RoleInOrganisation.Admin || roleInOrganisation == RoleInOrganisation.Employee))
        {
            isUpdatable = true;
        }
        else
        {
            if (
                model.FirstName == model.OriginalFirstName &&
                model.LastName == model.OriginalLastName &&
                model.JobTitle == model.OriginalJobTitle &&
                model.Telephone != model.OriginalTelephone)
            {
                isUpdatable = true;
            }
        }
        return isUpdatable;
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

    private static bool IsBasicAdmin(UserData userData)
    {
        var roleInOrganisation = userData.RoleInOrganisation.ToString();

        var serviceRoleId = userData.ServiceRoleId;

        if (string.IsNullOrEmpty(roleInOrganisation))
        {
            throw new InvalidOperationException("Unknown role in organisation.");
        }
        if (serviceRoleId == default)
        {
            throw new InvalidOperationException("Unknown service role.");
        }

        return roleInOrganisation == PersonRole.Admin.ToString() &&
            serviceRoleId == (int)ServiceRole.Basic;
    }
}