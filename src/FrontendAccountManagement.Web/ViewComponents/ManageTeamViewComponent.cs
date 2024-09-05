using EPR.Common.Authorization.Extensions;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Web.ManageTeam.Rules;
using FrontendAccountManagement.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace FrontendAccountManagement.Web.ViewComponents;

public class ManageTeamViewComponent : ViewComponent
{
    private readonly IFacadeService _facadeService;
    private readonly IHttpContextAccessor _contextAccessor;

    public ManageTeamViewComponent(IFacadeService facadeService, IHttpContextAccessor contextAccessor)
    {
        _facadeService = facadeService;
        _contextAccessor = contextAccessor;
    }
    
    public async Task<ViewViewComponentResult> InvokeAsync()
    {
        var httpContext = _contextAccessor.HttpContext;
        var userData = httpContext.User.GetUserData();
        var organisationId = userData.Organisations[0].Id.ToString();
        var serviceRoleId = userData.ServiceRoleId;
        var serviceRoleEnum = (ServiceRole)serviceRoleId;
        var roleInOrganisation = userData.RoleInOrganisation;
        var serviceRoleKey = $"{serviceRoleEnum.ToString()}.{roleInOrganisation}";
        
        var users = await _facadeService.GetUsersForOrganisationAsync(organisationId, serviceRoleId);
        var manageTeam = new RemoveUserRules(serviceRoleKey, users.ToList());
        var updatedUsers = manageTeam.SetRemovableUsers();

        var model = new ManageTeamModel
        {
            Users = updatedUsers.ToList()
        };

        return View(model);
    }
}