using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.Configs
{
    [ExcludeFromCodeCoverage]
    public class PhaseBannerOptions
    {
        public const string ConfigSection = "PhaseBannerOptions";

        public string ApplicationStatus { get; set; } = string.Empty;

        public string SurveyUrl { get; set; } = string.Empty;

        public bool Enabled { get; set; }
    }
}