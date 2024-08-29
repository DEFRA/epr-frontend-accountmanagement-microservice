using System.Security.Claims;
using System.Text.Json;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Core.Sessions.Mappings;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Attributes;
using FrontendAccountManagement.Web.ViewModels;
using FrontendAccountManagement.Web.ViewModels.PermissionManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.Identity.Web;

namespace FrontendAccountManagement.Web.Controllers.PermissionManagement
{
    [FeatureGate(FeatureFlags.ManageUserPermissions)]
    [Authorize(Policy = PolicyConstants.AccountPermissionManagementPolicy)]
    public class PermissionManagementController : Controller
    {
        private readonly ISessionManager<JourneySession> _sessionManager;
        private readonly IFacadeService _facadeService;
        private readonly ServiceSettingsOptions _serviceSettings;

        public PermissionManagementController(ISessionManager<JourneySession> sessionManager, IFacadeService facadeService, IOptions<ServiceSettingsOptions> serviceSettingsOptions)
        {
            _sessionManager = sessionManager;
            _facadeService = facadeService;
            _serviceSettings = serviceSettingsOptions.Value;
        }

        [HttpGet]
        [Route(PagePath.ChangeAccountPermissions + "/{id:guid}")]
        [JourneyAccess(PagePath.ChangeAccountPermissions, JourneyName.ManagePermissionsStart)]
        [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
        public async Task<IActionResult> ChangeAccountPermissions(Guid id)
        {   
            var pagePath = $"{PagePath.ChangeAccountPermissions}/{id}";
            var nextPagePath = $"{PagePath.ManageAccount}";

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

            session.PermissionManagementSession ??= new PermissionManagementSession { Items = new List<PermissionManagementSessionItem>() };

            await ClearSessionItemIfStartOfJourney(session, id);

            var userDataClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData);
            if(userDataClaim == null)
            {
                return RedirectHome();
            }
            var userData = JsonSerializer.Deserialize<UserData>(userDataClaim.Value);
            session.IsComplianceScheme = userData?.Organisations?.FirstOrDefault()?.OrganisationRole == OrganisationRoles.ComplianceScheme;

            var organisationId = User.GetOrganisationId();

            if (organisationId == null)
            {
                return RedirectHome();
            }

            var currentPermissionTypeResult = await _facadeService.GetPermissionTypeFromConnectionAsync(organisationId.Value, id, _serviceSettings.ServiceKey);

            if (currentPermissionTypeResult.UserId == User.UserId() || currentPermissionTypeResult.PermissionType == null || currentPermissionTypeResult.PermissionType == PermissionType.Approved)
            {
                return RedirectHome();
            }

            if (!session.PermissionManagementSession.Items.Exists(i => i.Id == id))
            {
                session.PermissionManagementSession.Items.Add(new PermissionManagementSessionItem { Id = id, Journey = new List<string> { pagePath }});
            }

            var currentSessionItem = session.PermissionManagementSession.Items.Find(i => i.Id == id);

            var model = new ChangeAccountPermissionViewModel
            {
                Id = id,
                PermissionType = currentSessionItem?.PermissionType ?? currentPermissionTypeResult.PermissionType.Value,
                ShowDelegatedContent = User.IsApprovedPerson() && _serviceSettings.ServiceKey == ServiceKey.Packaging,
                ServiceKey = _serviceSettings.ServiceKey
            };

            SetBackLink(currentSessionItem, pagePath);
            await SaveSession(session, pagePath, nextPagePath, id);

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.ChangeAccountPermissions + "/{id:guid}")]
        [JourneyAccess(PagePath.ChangeAccountPermissions, JourneyName.ManagePermissionsStart)]
        [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
        public async Task<IActionResult> ChangeAccountPermissions(ChangeAccountPermissionViewModel model, Guid id)
        {
            var pagePath = $"{PagePath.ChangeAccountPermissions}/{id}";

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            if(session == null)
            {
                return RedirectHome();
            }

            session.PermissionManagementSession ??= new PermissionManagementSession { Items = new List<PermissionManagementSessionItem>() };

            var currentSessionItem = session.PermissionManagementSession.Items.Find(i => i.Id == id);
            if(currentSessionItem == null)
            {
                currentSessionItem = new PermissionManagementSessionItem { Id = id, Journey = new List<string> { pagePath } };
                session.PermissionManagementSession.Items.Add(currentSessionItem);
            }

            var organisationId = User.GetOrganisationId();

            if (organisationId == null)
            {
                return RedirectHome();
            }

            var currentPermissionTypeResult = await _facadeService.GetPermissionTypeFromConnectionAsync(organisationId.Value, id, _serviceSettings.ServiceKey);

            if (currentPermissionTypeResult.PermissionType == null || currentPermissionTypeResult.PermissionType == PermissionType.Approved)
            {
                return RedirectHome();
            }

            if (!ModelState.IsValid)
            {
                model.Id = id;
                model.PermissionType = currentSessionItem.PermissionType ?? currentPermissionTypeResult.PermissionType.Value;
                model.ShowDelegatedContent = User.IsApprovedPerson() && _serviceSettings.ServiceKey == ServiceKey.Packaging;
                model.ServiceKey = _serviceSettings.ServiceKey;

                SetBackLink(currentSessionItem, pagePath);

                return View(model);
            }

            currentSessionItem.PermissionType = model.PermissionType;

            (string nextPagePath, string actionName, PersonRole? personRole) = GetChangeAccountPermissionDataToDecideNextAction(
                model,
                currentPermissionTypeResult.PermissionType,
                id);

            if (personRole != null)
            {
                await _facadeService.UpdatePersonRoleAdminOrEmployee(id, personRole.Value, organisationId.Value, _serviceSettings.ServiceKey);
                var personUpdated = await _facadeService.GetPersonDetailsFromConnectionAsync(organisationId.Value, id, _serviceSettings.ServiceKey);
                TempData["PersonUpdated"] = $"{personUpdated.FirstName} {personUpdated.LastName}";

                return await RemoveSessionItemAndRedirectHomeAsync(session, id);
            }
            else if(nextPagePath == PagePath.ManageAccount)
            {
                return await RemoveSessionItemAndRedirectHomeAsync(session, id);
            }

            return await SaveSessionAndRedirect(session, actionName, pagePath, nextPagePath, id);
        }

        [HttpGet]
        [Route(PagePath.RelationshipWithOrganisation + "/{id:guid}")]
        [JourneyAccess(PagePath.RelationshipWithOrganisation, JourneyName.ManagePermissions)]
        public async Task<IActionResult> RelationshipWithOrganisation(Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var currentSessionItem = session?.PermissionManagementSession.Items.Find(i => i.Id == id);
            if (currentSessionItem == null)
            {
                return RedirectHome();
            }
            
            var userDataClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData);
            if (userDataClaim == null)
            {
                return RedirectHome();
            }
                          
            var model = new RelationshipWithOrganisationViewModel
            {
                Id = id,
                IsComplianceScheme = session.IsComplianceScheme,
                SelectedRelationshipWithOrganisation = currentSessionItem.RelationshipWithOrganisation,
                AdditionalRelationshipInformation = currentSessionItem.AdditionalRelationshipInformation
            };

            var pagePath = $"{PagePath.RelationshipWithOrganisation}/{id}";
            SetBackLink(currentSessionItem, pagePath);

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.RelationshipWithOrganisation + "/{id:guid}")]
        [JourneyAccess(PagePath.RelationshipWithOrganisation, JourneyName.ManagePermissions)]
        public async Task<IActionResult> RelationshipWithOrganisation(RelationshipWithOrganisationViewModel model, Guid id)
        {
            var pagePath = $"{PagePath.RelationshipWithOrganisation}/{id}";
            var nextPagePath = String.Empty;

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            model.IsComplianceScheme = session.IsComplianceScheme;

            var currentSessionItem = session.PermissionManagementSession.Items.Find(i => i.Id == id);
            if(currentSessionItem == null)
            {
                return RedirectHome();
            }

            SetBackLink(currentSessionItem, pagePath);
        
            if (model.SelectedRelationshipWithOrganisation == Core.Sessions.RelationshipWithOrganisation.NotSet)
            {
                ModelState.AddModelError(nameof(model.SelectedRelationshipWithOrganisation), "RelationshipWithOrganisation.SelectHowTheyWork");
            
                return View(nameof(RelationshipWithOrganisation), model);
            }

            if (model.SelectedRelationshipWithOrganisation == Core.Sessions.RelationshipWithOrganisation.SomethingElse)
            {
                if (string.IsNullOrEmpty(model.AdditionalRelationshipInformation))
                {
                    ModelState.AddModelError(nameof(model.AdditionalRelationshipInformation), "RelationshipWithOrganisation.EnterTheirRelationship");
                    return View(nameof(RelationshipWithOrganisation), model);
                }
                if (!string.IsNullOrEmpty(model.AdditionalRelationshipInformation) && model.AdditionalRelationshipInformation.Length > 450)
                {
                    ModelState.AddModelError(nameof(model.AdditionalRelationshipInformation), "RelationshipWithOrganisation.MaxLengthError");
                    return View(nameof(RelationshipWithOrganisation), model);
                }
            }
            
            currentSessionItem.RelationshipWithOrganisation = model.SelectedRelationshipWithOrganisation;
            
            switch(model.SelectedRelationshipWithOrganisation) 
            {
                case Core.Sessions.RelationshipWithOrganisation.Employee:
                    nextPagePath = $"{PagePath.JobTitle}/{id}";
                    currentSessionItem.AdditionalRelationshipInformation = null;
                    break;
                case Core.Sessions.RelationshipWithOrganisation.Consultant:
                    nextPagePath = $"{PagePath.NameOfConsultancy}/{id}";
                    currentSessionItem.AdditionalRelationshipInformation = null;
                    break;
                case Core.Sessions.RelationshipWithOrganisation.ConsultantFromComplianceScheme:
                    nextPagePath = $"{PagePath.NameOfComplianceScheme}/{id}";
                    currentSessionItem.AdditionalRelationshipInformation = null;
                    break;
                case Core.Sessions.RelationshipWithOrganisation.SomethingElse:
                    nextPagePath = $"{PagePath.NameOfOrganisation}/{id}";
                    currentSessionItem.AdditionalRelationshipInformation = model.AdditionalRelationshipInformation;
                    break;
            }

            return await SaveSessionAndRedirect(session, nameof(JobTitle), pagePath, nextPagePath, id);
        }

        [HttpGet]
        [Route(PagePath.JobTitle + "/{id:guid}")]
        [JourneyAccess(PagePath.JobTitle, JourneyName.ManagePermissions)]
        public async Task<IActionResult> JobTitle(Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            var currentSessionItem = session?.PermissionManagementSession.Items.Find(item => item.Id == id);

            if (currentSessionItem is not { RelationshipWithOrganisation: Core.Sessions.RelationshipWithOrganisation.Employee })
            {
                return RedirectHome();
            }

            var model = new JobTitleViewModel
            {
                Id = id,
                JobTitle = currentSessionItem.JobTitle
            };

            SetBackLink(currentSessionItem, $"{PagePath.JobTitle}/{id}");

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.JobTitle + "/{id:guid}")]
        [JourneyAccess(PagePath.JobTitle, JourneyName.ManagePermissions)]
        public async Task<IActionResult> JobTitle(JobTitleViewModel model, Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            var currentSessionItem = session?.PermissionManagementSession.Items.Find(item => item.Id == id);

            if (currentSessionItem is not { RelationshipWithOrganisation: Core.Sessions.RelationshipWithOrganisation.Employee })
            {
                return RedirectHome();
            }

            var currentPagePath = $"{PagePath.JobTitle}/{id}";

            if (!ModelState.IsValid)
            {
                SetBackLink(currentSessionItem, currentPagePath);
                return View(model);
            }

            currentSessionItem.JobTitle = model.JobTitle;
            currentSessionItem.NameOfConsultancy = null;
            currentSessionItem.NameOfComplianceScheme = null;
            currentSessionItem.NameOfOrganisation = null;

            var nextPagePath = $"{PagePath.CheckDetailsSendInvite}/{id}";

            return await SaveSessionAndRedirect(session!, nameof(CheckDetailsSendInvite), currentPagePath, nextPagePath, id);
        }

        [HttpGet]
        [Route(PagePath.NameOfConsultancy + "/{id:guid}")]
        [JourneyAccess(PagePath.NameOfConsultancy, JourneyName.ManagePermissions)]
        public async Task<IActionResult> NameOfConsultancy(Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var currentSessionItem = session?.PermissionManagementSession.Items.Find(i => i.Id == id);

            if (currentSessionItem is not { RelationshipWithOrganisation: Core.Sessions.RelationshipWithOrganisation.Consultant })
            {
                return RedirectHome();
            }

            var model = new NameOfConsultancyViewModel
            {
                Id = id,
                Name = currentSessionItem.NameOfConsultancy ?? string.Empty
            };

            SetBackLink(currentSessionItem, $"{PagePath.NameOfConsultancy}/{id}");

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.NameOfConsultancy + "/{id:guid}")]
        [JourneyAccess(PagePath.NameOfConsultancy, JourneyName.ManagePermissions)]
        public async Task<IActionResult> NameOfConsultancy(NameOfConsultancyViewModel model, Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var currentSessionItem = session?.PermissionManagementSession.Items.Find(i => i.Id == id);

            if (currentSessionItem is not { RelationshipWithOrganisation: Core.Sessions.RelationshipWithOrganisation.Consultant })
            {
                return RedirectHome();
            }

            var pagePath = $"{PagePath.NameOfConsultancy}/{id}";

            if (!ModelState.IsValid)
            {
                SetBackLink(currentSessionItem, pagePath);
                return View(model);
            }

            currentSessionItem.NameOfConsultancy = model.Name;
            currentSessionItem.JobTitle = null;
            currentSessionItem.NameOfComplianceScheme = null;
            currentSessionItem.NameOfOrganisation = null;

            var nextPagePath = $"{PagePath.CheckDetailsSendInvite}/{id}";

            return await SaveSessionAndRedirect(session, nameof(CheckDetailsSendInvite), pagePath, nextPagePath, id);
        }

        [HttpGet]
        [Route(PagePath.NameOfOrganisation + "/{id:guid}")]
        [JourneyAccess(PagePath.NameOfOrganisation, JourneyName.ManagePermissions)]
        public async Task<IActionResult> NameOfOrganisation(Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var currentSessionItem = session?.PermissionManagementSession.Items.Find(i => i.Id == id);

            if (currentSessionItem is not { RelationshipWithOrganisation: Core.Sessions.RelationshipWithOrganisation.SomethingElse })
            {
                return RedirectHome();
            }

            var model = new NameOfOrganisationViewModel
            {
                Id = id,
                Name = currentSessionItem.NameOfOrganisation ?? string.Empty
            };

            SetBackLink(currentSessionItem, $"{PagePath.NameOfOrganisation}/{id}");

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.NameOfOrganisation + "/{id:guid}")]
        [JourneyAccess(PagePath.NameOfOrganisation, JourneyName.ManagePermissions)]
        public async Task<IActionResult> NameOfOrganisation(NameOfOrganisationViewModel model, Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var currentSessionItem = session?.PermissionManagementSession.Items.Find(i => i.Id == id);

            if(currentSessionItem is not { RelationshipWithOrganisation: Core.Sessions.RelationshipWithOrganisation.SomethingElse })
            {
                return RedirectHome();
            }

            var pagePath = $"{PagePath.NameOfOrganisation}/{id}";

            if (!ModelState.IsValid)
            {
                SetBackLink(currentSessionItem, pagePath);
                return View(model);
            }

            currentSessionItem.NameOfOrganisation = model.Name;
            currentSessionItem.NameOfConsultancy = null;
            currentSessionItem.NameOfComplianceScheme = null;
            currentSessionItem.JobTitle = null;

            var nextPagePath = $"{PagePath.CheckDetailsSendInvite}/{id}";

            return await SaveSessionAndRedirect(session, nameof(CheckDetailsSendInvite), pagePath, nextPagePath, id);
        }

        [HttpGet]
        [Route(PagePath.NameOfComplianceScheme + "/{id:guid}")]
        [JourneyAccess(PagePath.NameOfComplianceScheme, JourneyName.ManagePermissions)]
        public async Task<IActionResult> NameOfComplianceScheme(Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var currentSessionItem = session?.PermissionManagementSession.Items.Find(i => i.Id == id);

            if (currentSessionItem is not { RelationshipWithOrganisation: Core.Sessions.RelationshipWithOrganisation.ConsultantFromComplianceScheme })
            {
                return RedirectHome();
            }

            var model = new NameOfComplianceSchemeViewModel
            {
                Id = id,
                Name = currentSessionItem.NameOfComplianceScheme ?? string.Empty
            };

            SetBackLink(currentSessionItem, $"{PagePath.NameOfComplianceScheme}/{id}");

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.NameOfComplianceScheme + "/{id:guid}")]
        [JourneyAccess(PagePath.NameOfComplianceScheme, JourneyName.ManagePermissions)]
        public async Task<IActionResult> NameOfComplianceScheme(NameOfComplianceSchemeViewModel model, Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);
            var currentSessionItem = session?.PermissionManagementSession.Items.Find(i => i.Id == id);

            if (currentSessionItem is not { RelationshipWithOrganisation: Core.Sessions.RelationshipWithOrganisation.ConsultantFromComplianceScheme })
            {
                return RedirectHome();
            }

            var pagePath = $"{PagePath.NameOfComplianceScheme}/{id}";

            if (!ModelState.IsValid)
            {
                SetBackLink(currentSessionItem, pagePath);
                return View(model);
            }

            currentSessionItem.NameOfComplianceScheme = model.Name;
            currentSessionItem.NameOfConsultancy = null;
            currentSessionItem.NameOfOrganisation = null;
            currentSessionItem.JobTitle = null;

            var nextPagePath = $"{PagePath.CheckDetailsSendInvite}/{id}";

            return await SaveSessionAndRedirect(session, nameof(CheckDetailsSendInvite), pagePath, nextPagePath, id);
        }

        [HttpGet]
        [Route(PagePath.CheckDetailsSendInvite + "/{id:guid}")]
        [JourneyAccess(PagePath.CheckDetailsSendInvite, JourneyName.ManagePermissions)]
        public async Task<IActionResult> CheckDetailsSendInvite(Guid id)
        {
            var pagePath = $"{PagePath.CheckDetailsSendInvite}/{id}";
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            var currentSessionItem = session?.PermissionManagementSession.Items.Find(i => i.Id == id);

            if (currentSessionItem == null)
            {
                return RedirectHome();
            }

            var inviteeFullName = await GetPersonFullNameFromConnectionAsync(id);
            
            if (string.IsNullOrEmpty(inviteeFullName))
            {
                return RedirectHome();
            }
            
            var model = new CheckDetailsSendInviteViewModel
            {
                Id = id,
                PermissionType = currentSessionItem.PermissionType,
                SelectedRelationshipWithOrganisation = currentSessionItem.RelationshipWithOrganisation,
                AdditionalRelationshipInformation = currentSessionItem.AdditionalRelationshipInformation,
                JobTitle = currentSessionItem.JobTitle,
                NameOfConsultancy = currentSessionItem.NameOfConsultancy,
                NameOfComplianceScheme = currentSessionItem.NameOfComplianceScheme,
                NameOfOrganisation = currentSessionItem.NameOfOrganisation,
                InviteeFullname = inviteeFullName,
            };

            SetBackLink(currentSessionItem, pagePath);

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.CheckDetailsSendInvite + "/{id:guid}")]
        [JourneyAccess(PagePath.CheckDetailsSendInvite, JourneyName.ManagePermissions)]
        public async Task<IActionResult> CheckDetailsSendInvite(CheckDetailsSendInviteViewModel model, Guid id)
        { 
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session);

            var currentSessionItem = session?.PermissionManagementSession.Items.Find(item => item.Id == id);

            if (currentSessionItem == null)
            {
                return RedirectHome();
            }

            var organisationId = User.GetOrganisationId();

            if (organisationId == null)
            {
                return RedirectHome();
            }

            var currentPagePath = $"{PagePath.CheckDetailsSendInvite}/{id}";

            if (!ModelState.IsValid)
            {
                var inviteeFullName = await GetPersonFullNameFromConnectionAsync(id);

                if (string.IsNullOrEmpty(inviteeFullName))
                {
                    return RedirectHome();
                }

                model = new CheckDetailsSendInviteViewModel
                {
                    Id = id,
                    PermissionType = currentSessionItem.PermissionType,
                    SelectedRelationshipWithOrganisation = currentSessionItem.RelationshipWithOrganisation,
                    AdditionalRelationshipInformation = currentSessionItem.AdditionalRelationshipInformation,
                    JobTitle = currentSessionItem.JobTitle,
                    NameOfConsultancy = currentSessionItem.NameOfConsultancy,
                    NameOfComplianceScheme = currentSessionItem.NameOfComplianceScheme,
                    NameOfOrganisation = currentSessionItem.NameOfOrganisation,
                    InviteeFullname = inviteeFullName
                };
                
                SetBackLink(currentSessionItem, currentPagePath);

                return View(model);
            }

            currentSessionItem.Fullname = model.Fullname;
            
            var journey = currentSessionItem.Journey;

            session.PermissionManagementSession.Items.Clear();
            session.PermissionManagementSession.Items.Add(new PermissionManagementSessionItem { Id = id, Journey = journey});

            var nextPagePath = $"{PagePath.InvitationToChangeSent}/{id}";

            var nominationRequest = PermissionSessionManagementSessionMappings.MapToDelegatedPersonNominationRequest(currentSessionItem);

            await _facadeService.NominateToDelegatedPerson(id, organisationId.Value, _serviceSettings.ServiceKey, nominationRequest);

            return await SaveSessionAndRedirect(session, nameof(InvitationToChangeSent), currentPagePath, nextPagePath, id);
        }

        private async Task<string> GetPersonFullNameFromConnectionAsync(Guid connectionId)
        {
            var userDataClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData);

            if (userDataClaim == null)
            {
                return string.Empty;
            }

            var userData = JsonSerializer.Deserialize<UserData>(userDataClaim.Value);
            var organisationId = userData?.Organisations[0].Id;

            if (organisationId == null)
            {
                return string.Empty;
            }

            var connectionPerson = await _facadeService.GetPersonDetailsFromConnectionAsync(organisationId.Value, connectionId, _serviceSettings.ServiceKey);

            if (connectionPerson == null)
            {
                return string.Empty;
            }

            return $"{connectionPerson.FirstName} {connectionPerson.LastName}";
        }

        [HttpGet]
        [Route(PagePath.InvitationToChangeSent + "/{id:guid}")]
        [JourneyAccess(PagePath.InvitationToChangeSent, JourneyName.ManagePermissions)]
        public async Task<IActionResult> InvitationToChangeSent(Guid id)
        {
            var organisationId = User.GetOrganisationId();

            if (organisationId == null)
            {
                return RedirectHome();
            }

            var connectionPerson = await _facadeService.GetPersonDetailsFromConnectionAsync(organisationId.Value, id, _serviceSettings.ServiceKey);

            var model = new InvitationToChangeSentViewModel
            {
                UserDisplayName = $"{connectionPerson?.FirstName} {connectionPerson?.LastName}"
            };

            return View(model);
        }

        [HttpGet]
        [AuthorizeForScopes(ScopeKeySection = "FacadeAPI:DownstreamScope")]
        [Route(PagePath.ConfirmChangePermission + "/{id:guid}")]
        [JourneyAccess(PagePath.ConfirmChangePermission, JourneyName.ManagePermissions)]
        public async Task<IActionResult> ConfirmChangePermission(Guid id)
        {
            var organisationId = User.GetOrganisationId();

            if (organisationId == null)
            {
                return RedirectHome();
            }

            var connectionPerson = await _facadeService.GetPersonDetailsFromConnectionAsync(organisationId.Value, id, _serviceSettings.ServiceKey);
            var enrolmentStatus = await _facadeService.GetEnrolmentStatus(organisationId.Value, id, _serviceSettings.ServiceKey, Core.Constants.ServiceRoles.Packaging.DelegatedPerson);

            var model = new ConfirmChangePermissionViewModel
            {
                Id = id,
                ApprovedByRegulator = enrolmentStatus == Core.Enums.EnrolmentStatus.Approved,
                DisplayName = $"{connectionPerson.FirstName} {connectionPerson.LastName}",
                NationIds = await _facadeService.GetNationIds(organisationId.Value)
            };

            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();
            var currentSessionItem = session.PermissionManagementSession.Items.Find(i => i.Id == id);
            var pagePath = $"{PagePath.ConfirmChangePermission}/{id}";

            SetBackLink(currentSessionItem, pagePath);

            return View(model);
        }

        [HttpPost]
        [Route(PagePath.ConfirmChangePermission + "/{id:guid}")]
        [JourneyAccess(PagePath.ConfirmChangePermission, JourneyName.ManagePermissions)]
        public async Task<IActionResult> ConfirmChangePermission(ConfirmChangePermissionViewModel model, Guid id)
        {
            var session = await _sessionManager.GetSessionAsync(HttpContext.Session) ?? new JourneySession();

            var pagePath = $"{PagePath.ConfirmChangePermission}/{id}";

            var organisationId = User.GetOrganisationId();
            var currentSessionItem = session.PermissionManagementSession.Items.Find(i => i.Id == id);

            if (organisationId == null || currentSessionItem == null)
            {
                return RedirectHome();
            }

            if (model.ConfirmAnswer == null)
            {
                var connectionPerson = await _facadeService.GetPersonDetailsFromConnectionAsync(organisationId.Value, id, _serviceSettings.ServiceKey);
                var enrolmentStatus = await _facadeService.GetEnrolmentStatus(organisationId.Value, id, _serviceSettings.ServiceKey, Core.Constants.ServiceRoles.Packaging.DelegatedPerson);

                model.Id = id;
                model.ApprovedByRegulator = enrolmentStatus == Core.Enums.EnrolmentStatus.Approved;
                model.DisplayName = $"{connectionPerson.FirstName} {connectionPerson.LastName}";
                model.NationIds = await _facadeService.GetNationIds(organisationId.Value);

                ModelState.AddModelError(nameof(model.ConfirmAnswer), model.ApprovedByRegulator ? "ConfirmChangePermission.Approved.ValidationMessage" : "ConfirmChangePermission.Unapproved.ValidationMessage");

                SetBackLink(currentSessionItem, pagePath);

                return View(model);
            }

            if (model.ConfirmAnswer == YesNoAnswer.Yes)
            {
                var personRole = currentSessionItem.PermissionType == PermissionType.Admin ? PersonRole.Admin : PersonRole.Employee;
                await _facadeService.UpdatePersonRoleAdminOrEmployee(id, personRole, organisationId.Value, _serviceSettings.ServiceKey);
            }

            return await RemoveSessionItemAndRedirectHomeAsync(session, id);
        }

        private void SetBackLink(PermissionManagementSessionItem? sessionItem, string currentPagePath)
        {
            if(sessionItem != null)
            {
                var previousPage = sessionItem.Journey.PreviousOrDefault(currentPagePath) ?? string.Empty;
                ViewBag.BackLinkToDisplay = $"{HttpContext?.Request.PathBase.ToString()}/{previousPage}";
            }
        }

        private async Task<RedirectToActionResult> SaveSessionAndRedirect(
            JourneySession? session,
            string actionName,
            string currentPagePath,
            string nextPagePath,
            Guid id)
        {
            if(session != null) 
            { 
                await SaveSession(session, currentPagePath, nextPagePath, id);
            }

            return RedirectToAction(actionName, new { id });
        }

        private async Task SaveSession(JourneySession session, string currentPagePath, string nextPagePath, Guid id)
        {
            var currentSessionItem = session.PermissionManagementSession.Items.Find(i => i.Id == id);

            if(currentSessionItem == null) 
            { 
                return; 
            }

            ClearRestOfJourney(currentSessionItem, currentPagePath);

            currentSessionItem.Journey.AddIfNotExists(nextPagePath);

            await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
        }

        private static void ClearRestOfJourney(PermissionManagementSessionItem sessionItem, string currentPagePath)
        {
            var index = sessionItem.Journey.IndexOf(currentPagePath);
            sessionItem.Journey = sessionItem.Journey.Take(index + 1).ToList();
        }

        private RedirectToActionResult RedirectHome()
        {
            return RedirectToAction("ManageAccount", "AccountManagement");
        }

        private async Task<RedirectToActionResult> RemoveSessionItemAndRedirectHomeAsync(JourneySession session, Guid id)
        {
            var currentSessionItem = session.PermissionManagementSession.Items.Find(i => i.Id == id);
            
            if(currentSessionItem != null)
            {
                session.PermissionManagementSession.Items.Remove(currentSessionItem);
                await _sessionManager.SaveSessionAsync(HttpContext.Session, session);
            }

            return RedirectToAction("ManageAccount", "AccountManagement");
        }

        private async Task ClearSessionItemIfStartOfJourney(JourneySession journeySession, Guid sessionItemId)
        {
            if (journeySession.PermissionManagementSession != null)
            {
                var permissionManagementSessionItem = journeySession.PermissionManagementSession.Items.Find(p => p.Id == sessionItemId);
                var refererRelativePath = Request.GetTypedHeaders().Referer?.PathAndQuery;

                if (permissionManagementSessionItem != null && refererRelativePath != null)
                {
                    refererRelativePath = refererRelativePath.Replace(Request.PathBase.Value, "").Replace("/", "");

                    if (refererRelativePath == string.Empty || refererRelativePath.Contains(PagePath.ManageAccount))
                    {
                        journeySession.PermissionManagementSession.Items.Remove(permissionManagementSessionItem);
                        await _sessionManager.SaveSessionAsync(HttpContext.Session, journeySession);
                    }
                }
            }
        }
    
        private (string, string, PersonRole?) GetChangeAccountPermissionDataToDecideNextAction(
            ChangeAccountPermissionViewModel model,
            PermissionType? currentPermissionTypeResult,
            Guid id)
        {
            var nextPagePath = $"{PagePath.ManageAccount}";
            var actionName = string.Empty;
            PersonRole? personRole = null;
            switch (currentPermissionTypeResult)
            {
                case PermissionType.Basic:
                    if (model.PermissionType == PermissionType.Admin)
                    {
                        personRole = PersonRole.Admin;
                    }
                    else if (model.PermissionType == PermissionType.Delegated)
                    {
                        nextPagePath = $"{PagePath.RelationshipWithOrganisation}/{id}";
                        actionName = nameof(RelationshipWithOrganisation);
                    }
                    break;
                case PermissionType.Admin:
                    if (model.PermissionType == PermissionType.Basic)
                    {
                        personRole = PersonRole.Employee;
                    }
                    else if (model.PermissionType == PermissionType.Delegated)
                    {
                        nextPagePath = $"{PagePath.RelationshipWithOrganisation}/{id}";
                        actionName = nameof(RelationshipWithOrganisation);
                    }
                    break;
                case PermissionType.Delegated:
                    if (model.PermissionType != PermissionType.Delegated)
                    {
                        nextPagePath = $"{PagePath.ConfirmChangePermission}/{id}";
                        actionName = nameof(ConfirmChangePermission);
                    }
                    break;
            }

            return (nextPagePath, actionName, personRole);
        }
    }
}
