using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewComponents;

[ExcludeFromCodeCoverage]
public class LanguageSwitcherViewComponent : ViewComponent
{
    private readonly IOptions<RequestLocalizationOptions> _localizationOptions;
    private readonly IFeatureManager _featureManager;

    public LanguageSwitcherViewComponent(IOptions<RequestLocalizationOptions> localizationOptions, IFeatureManager featureManager)
    {
        _localizationOptions = localizationOptions;
        _featureManager = featureManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var cultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();

        var returnPath = string.IsNullOrEmpty(Request.Path.Value) ? "/" : Request.Path.Value;

        var languageSwitcherModel = new LanguageSwitcherModel
        {
            SupportedCultures = _localizationOptions.Value.SupportedCultures!.ToList(),
            CurrentCulture = cultureFeature!.RequestCulture.Culture,
            ReturnUrl = $"~{returnPath}",
            ShowLanguageSwitcher = await _featureManager.IsEnabledAsync(FeatureFlags.ShowLanguageSwitcher)
            
        };

        return View(languageSwitcherModel);
    }
}
