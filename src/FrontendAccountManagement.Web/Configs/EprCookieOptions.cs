using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.Configs
{
    [ExcludeFromCodeCoverage]
    public class EprCookieOptions
    {
        public const string ConfigSection = "CookieOptions";

        public int AuthenticationExpiryInMinutes { get; set; }

        public int CookiePolicyDurationInMonths { get; set; }

        public string SessionCookieName { get; set; }

        public string CookiePolicyCookieName { get; set; }

        public string AntiForgeryCookieName { get; set; }

        public string TsCookieName { get; set; }

        public string AuthenticationCookieName { get; set; }

        public string TempDataCookie { get; set; }
    }
}