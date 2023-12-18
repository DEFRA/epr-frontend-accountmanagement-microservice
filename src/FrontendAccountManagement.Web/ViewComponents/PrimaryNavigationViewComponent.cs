using EPR.Common.Authorization.Constants;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Navigation;
using FrontendAccountManagement.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;

namespace FrontendAccountManagement.Web.ViewComponents;

public class PrimaryNavigationViewComponent : ViewComponent
{
    private readonly ExternalUrlsOptions _options;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _contextAccessor;
    
    public PrimaryNavigationViewComponent(IOptions<ExternalUrlsOptions> options, IAuthorizationService authorizationService, IHttpContextAccessor contextAccessor)
    {
        _options = options.Value;
        _authorizationService = authorizationService;
        _contextAccessor = contextAccessor;
    }

    public async Task<ViewViewComponentResult> InvokeAsync()
    {
        
        var authorizeResult = await _authorizationService.AuthorizeAsync(_contextAccessor.HttpContext.User, _contextAccessor.HttpContext, PolicyConstants.AccountManagementPolicy);
        var isEnrolledAdmin = authorizeResult.Succeeded;
        var primaryNavigationPages = new PrimaryNavigationPages(_options, isEnrolledAdmin, Request.PathBase.Value);
        var pages = primaryNavigationPages.GetPrimaryNavigationPages();
        
        var primaryNavigationModel = new PrimaryNavigationModel
        {
            Items = pages
        };
        
        return View(primaryNavigationModel);
    }
}