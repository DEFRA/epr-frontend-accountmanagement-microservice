using AutoMapper;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.Profiles;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System;
using System.Net;
using System.Text.Json;
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
    private readonly ISessionManager<JourneySession> _sessionManager;
    private readonly IFacadeService _facadeService;
    private readonly ILogger<AccountManagementController> _logger;
    private readonly ExternalUrlsOptions _urlOptions;
    private readonly DeploymentRoleOptions _deploymentRoleOptions;
    private readonly IMapper _mapper;

    public AccountManagementController(
        ISessionManager<JourneySession> sessionManager,
        IFacadeService facadeService,
        IOptions<ExternalUrlsOptions> urlOptions,
        IOptions<DeploymentRoleOptions> deploymentRoleOptions,
        ILogger<AccountManagementController> logger,
        IMapper mapper)
    {
        _sessionManager = sessionManager;
        _facadeService = facadeService;
        _logger = logger;
        _urlOptions = urlOptions.Value;
        _deploymentRoleOptions = deploymentRoleOptions.Value;
        _mapper = mapper;
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
            model.InvitedUserEmail = session.AccountManagementSession.InviteeEmailAddress;
            session.AccountManagementSession.InviteeEmailAddress = null;
            session.AccountManagementSession.AddUserStatus = null;
        }

        model.PersonUpdated = TempData["PersonUpdated"] == null ? null : TempData["PersonUpdated"].ToString();

        await SaveSessionAndJourney(session, PagePath.ManageAccount, PagePath.ManageAccount);

        SetCustomBackLink(_urlOptions.LandingPageUrl);

        var userAccount = User.GetUserData();

        if (userAccount is null)
        {
            _logger.LogInformation("User authenticated but account could not be found");
        }
        else
        {
            var organisation = userAccount.Organisations.First();
            model.UserName = string.Format("{0} {1}", userAccount.FirstName, userAccount.LastName);
            model.Telephone = userAccount.Telephone;
            var userOrg = userAccount.Organisations?.FirstOrDefault();
            model.JobTitle = userAccount.JobTitle;
            model.CompanyName = userOrg.Name;
            model.OrganisationAddress = string.Join(", ", new[] {
                organisation.SubBuildingName,
                organisation.BuildingNumber,
                organisation.BuildingName,
                organisation.Street,
                organisation.Town,
                organisation.County,
                organisation.Postcode,
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
            model.EnrolmentStatus = userAccount.EnrolmentStatus;
            var serviceRoleId = userAccount.ServiceRoleId;
            var serviceRoleEnum = (ServiceRole)serviceRoleId;
            var roleInOrganisation = userAccount.RoleInOrganisation;
            model.ServiceRoleKey = $"{serviceRoleEnum.ToString()}.{roleInOrganisation}";
            model.OrganisationType = userOrg.OrganisationType;
            model.HasPermissionToChangeCompany = HasPermissionToChangeCompany(session.UserData);
            model.IsBasicUser = IsBasicUser(session.UserData);
        }
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
        return RedirectToAction("RemoveTeamMemberConfirmation", "AccountManagement");
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
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        SaveSessionAndJourney(session, PagePath.CheckYourDetails, PagePath.Declaration);
        SetBackLink(session, PagePath.Declaration);
        return View(nameof(Declaration));
    }


    [HttpPost]
    [Route(PagePath.Declaration, Name = "Declaration")]
    public async Task<IActionResult> DeclarationPost()
    {
        return RedirectToAction(nameof(DetailsChangeRequested));
    }

    [HttpGet]
    [Route(PagePath.WhatAreYourDetails)]
    public async Task<IActionResult> EditUserDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var model = _mapper.Map<EditUserDetailsViewModel>(User.GetUserData());

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

        if (TempData[NewUserDetailsKey] == null)
            TempData.Add(NewUserDetailsKey, JsonSerializer.Serialize(editUserDetailsViewModel));

        return RedirectToAction(nameof(PagePath.CheckYourDetails));
    }

    [HttpGet]
    [Route(PagePath.CheckYourDetails)]
    public async Task<IActionResult> CheckYourDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
        var userData = User.GetUserData();

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

        SaveSessionAndJourney(session, PagePath.WhatAreYourDetails, PagePath.CheckYourDetails);
        SetBackLink(PagePath.CheckYourDetails);

        if (TempData[AmendedUserDetailsKey] == null)
        {
            TempData.Add(AmendedUserDetailsKey, JsonSerializer.Serialize(editUserDetailsViewModel));
        }

        return View(model);
    }

    [HttpPost]
    [Route(PagePath.CheckYourDetails)]
    public async Task<IActionResult> CheckYourDetails(EditUserDetailsViewModel model)
    {
        var userData = User.GetUserData();

        var serviceRole = userData.ServiceRole ?? string.Empty;
        var roleInOrganisation = userData.RoleInOrganisation ?? string.Empty;

        // User has a service role of "basic" And an organisation role of "Admin"
        if (serviceRole.ToLower() == ServiceRoles.BasicUser.ToLower() && roleInOrganisation == RoleInOrganisation.Admin)
        {
            //TODO: save data to db

            return RedirectToAction(nameof(PagePath.UpdateDetailsConfirmation));
        }
        else //Approved or Delegated users
        {
            //TODO: if only Telephone updated then save to db
           // redirect to: PagePath.UpdateDetailsConfirmation

            return RedirectToAction(nameof(PagePath.Declaration));
        }
    }

    [HttpGet]
    [Route(PagePath.UpdateDetailsConfirmation)]
    public async Task<IActionResult> UpdateDetailsConfirmation()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var model = new UpdateDetailsConfirmationViewModel
        {
            Username = $"{session.UserData.FirstName} {session.UserData.LastName}",
            UpdatedDatetime = DateTime.Now
        };

        return View(model);
    }

    [HttpGet]
    [Route(PagePath.DetailsChangeRequestedNotification)]
    public async Task<IActionResult> DetailsChangeRequested()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

        var model = new DetailsChangeRequestedViewModel
        {
            Username = $"{session.UserData.FirstName} {session.UserData.LastName}",
            UpdatedDatetime = DateTime.Now
        };

        return View(nameof(DetailsChangeRequested), model);
    }

    [HttpGet]
    [Route(PagePath.ConfirmCompanyDetails)]
    public async Task<IActionResult> ConfirmCompanyDetails()
    {
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

    /// <summary>
    /// Displays a page with the updated data from the "choose your nation" page
    /// for the user to confirm they have entered the correct information
    /// </summary>
    /// <returns>async IActionResult containing the view</returns>
    [HttpGet]
    [Route(PagePath.CheckCompaniesHouseDetails)]
    public async Task<IActionResult> CheckCompaniesHouseDetails()
    {
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

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

        await _facadeService.UpdateOrganisationDetails(
            viewModel.OrganisationId,
            organisation);

        TempData.Remove(CheckYourOrganisationDetailsKey);
        // save the date/time that the update was performed for the next page
        TempData[OrganisationDetailsUpdatedTimeKey] = DateTime.Now;

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
        var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

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
            OrganisationId = session.UserData.Organisations.FirstOrDefault()?.Id ?? Guid.Empty,
            Address = string.Join(", ", address.AddressFields.Where(field => !string.IsNullOrWhiteSpace(field))),
            TradingName = session.CompaniesHouseSession?.CompaniesHouseData?.Organisation?.Name,
            UkNation = model.UkNation.Value
        };
        TempData[CheckYourOrganisationDetailsKey] = JsonSerializer.Serialize(checkYourOrganisationModel);

        return RedirectToAction(nameof(CheckCompaniesHouseDetails));
    }

    private async Task<bool> CompareDataAsync(JourneySession session)
    {
        var userData = User.GetUserData();

        var organisationData = userData.Organisations.FirstOrDefault();

        if (organisationData == null)
        {
            throw new ArgumentNullException(nameof(organisationData));
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
        return !IsRegulatorUser(userData) || IsBasicUser(userData);
    }

    private static bool IsRegulatorAdmin(UserData userData) =>
        userData.ServiceRoleId == (int)Core.Enums.ServiceRole.RegulatorAdmin;

    private static bool IsRegulatorBasic(UserData userData) =>
        userData.ServiceRoleId == (int)Core.Enums.ServiceRole.RegulatorBasic;

    private static bool IsRegulatorUser(UserData userData) =>
        IsRegulatorAdmin(userData) || IsRegulatorBasic(userData);

    private static bool IsBasicUser(UserData userData) =>
       userData.ServiceRoleId == (int)Core.Enums.ServiceRole.Basic;

    private static bool HasPermissionToChangeCompany(UserData userData)
    {
        var serviceRoleId = userData.ServiceRoleId;
        var serviceRoleEnum = (ServiceRole)serviceRoleId;
        var roleInOrganisation = userData.RoleInOrganisation;
        if ((serviceRoleEnum == ServiceRole.Approved || serviceRoleEnum == ServiceRole.Delegated)
            && !string.IsNullOrEmpty(roleInOrganisation) && roleInOrganisation == PersonRole.Admin.ToString())
        {
            return true;
        }
        return false;
    }

}
