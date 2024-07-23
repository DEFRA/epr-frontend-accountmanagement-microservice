using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Web.ManageTeam.Rules;
using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace FrontendAccountManagement.Web.ViewComponents;

public class ManageTeamViewComponent : ViewComponent
{
    private readonly IFacadeService _facadeService;
    private readonly ILogger<ManageTeamViewComponent> _logger;
    private readonly IHttpContextAccessor _contextAccessor;

    public ManageTeamViewComponent(IFacadeService facadeService, ILogger<ManageTeamViewComponent> logger, IHttpContextAccessor contextAccessor)
    {
        _facadeService = facadeService;
        _logger = logger;
        _contextAccessor = contextAccessor;
    }
    
    public async Task<ViewViewComponentResult> InvokeAsync()
    {
        var httpContext = _contextAccessor.HttpContext;
        var userData = httpContext.User.GetUserData();
        var organisationId = userData.Organisations.First().Id.ToString();
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