using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.Configs
{
    [ExcludeFromCodeCoverage]
    public static class FeatureFlags
    {
        public const string ManageUserPermissions = "ManageUserPermissions";
        public const string ShowLanguageSwitcher = "ShowLanguageSwitcher";
    }
}