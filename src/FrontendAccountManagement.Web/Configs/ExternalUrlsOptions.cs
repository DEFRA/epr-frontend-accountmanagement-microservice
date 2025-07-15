using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.Configs
{
    [ExcludeFromCodeCoverage]
    public class ExternalUrlsOptions
    {
        public const string ConfigSection = "ExternalUrls";

        public string AccessibilityAbilityNet { get; set; }

        public string AccessibilityEqualityAdvisorySupportService { get; set; }

        public string AccessibilityContactUs { get; set; }

        public string AccessibilityWebContentAccessibility { get; set; }

        public string EprGuidance { get; set; }

        public string GovUkHome { get; set; }

        public string LandingPageUrl { get; set; }

        public string PrivacyScottishEnvironmentalProtectionAgency { get; set; }

        public string PrivacyNationalResourcesWales { get; set; }

        public string PrivacyNorthernIrelandEnvironmentAgency { get; set; }

        public string PrivacyEnvironmentAgency { get; set; }

        public string PrivacyDataProtectionPublicRegister { get; set; }

        public string PrivacyDefrasPersonalInformationCharter { get; set; }

        public string PrivacyInformationCommissioner { get; set; }

        public string FrontEndCreationBaseUrl { get; set; }

        public string PrivacyLink { get; set; }

        public string CookiesLink { get; set; }

        public string CompanyHouseChangeRequestLink { get; set; }
        
        public string EprPrnManageOrganisationLink { get; set; }
    }
}