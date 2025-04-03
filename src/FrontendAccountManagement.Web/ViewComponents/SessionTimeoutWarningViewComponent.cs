using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewComponents;

[ExcludeFromCodeCoverage]
public class SessionTimeoutWarningViewComponent : ViewComponent
{
    private readonly IFeatureManager _featureManager;

    public SessionTimeoutWarningViewComponent(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var sessionTimeoutWarningModel = new SessionTimeoutWarningModel
        {
            ShowSessionTimeoutWarning = await _featureManager.IsEnabledAsync(FeatureFlags.ShowSessionTimeoutWarning)
            
        };

        return View(sessionTimeoutWarningModel);
    }
}
