using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.ViewModels.Shared;

namespace FrontendAccountManagement.Web.Navigation;

public class PrimaryNavigationPages
{
    private readonly bool _isEnrolledAdmin;
    private readonly NavigationModel _home;
    private readonly NavigationModel _manageAccount;

    public PrimaryNavigationPages(ExternalUrlsOptions options, bool isEnrolledAdmin, string pathBase)
    {
        _isEnrolledAdmin = isEnrolledAdmin;
        _home = new()
        {
            LinkValue = options.LandingPageUrl,
            LocalizerKey = "PrimaryNavigation.Home"
        };
        _manageAccount = new()
        {
            
            LinkValue = $"{pathBase}/{PagePath.ManageAccount}",
            LocalizerKey = "PrimaryNavigation.ManageAccount",
            IsActive = true
        };
    }
    
    public List<NavigationModel> GetPrimaryNavigationPages()
    {
        var pagesToReturn = new List<NavigationModel> { _home };

        if (_isEnrolledAdmin)
        {
            pagesToReturn.Add(_manageAccount);
        }

        return pagesToReturn;
    }
}