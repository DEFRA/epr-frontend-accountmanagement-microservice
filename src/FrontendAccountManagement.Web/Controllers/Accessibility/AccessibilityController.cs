using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.ViewModels.Accessibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FrontendAccountManagement.Web.Controllers.Accessibility
{
    [AllowAnonymous]
    public class AccessibilityController : Controller
    {
        private readonly ExternalUrlsOptions _urlOptions;
        private readonly EmailAddressOptions _emailOptions;
        private readonly SiteDateOptions _siteDateOptions;

        public AccessibilityController(
            IOptions<ExternalUrlsOptions> urlOptions,
            IOptions<EmailAddressOptions> emailOptions,
            IOptions<SiteDateOptions> siteDateOptions)
        {
            _urlOptions = urlOptions?.Value;
            _emailOptions = emailOptions?.Value;
            _siteDateOptions = siteDateOptions?.Value;
        }

        [HttpGet]
        [Route(PagePath.Accessibility)]
        public IActionResult Detail(string returnUrl)
        {
            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.HomePath();
            }

            var model = new AccessibilityViewModel
            {
                AbilityNetUrl = _urlOptions.AccessibilityAbilityNet,
                ContactUsUrl = _urlOptions.AccessibilityContactUs,
                EqualityAdvisorySupportServiceUrl = _urlOptions.AccessibilityEqualityAdvisorySupportService,
                WebContentAccessibilityUrl = _urlOptions.AccessibilityWebContentAccessibility,
                DefraHelplineEmail = _emailOptions.DefraHelpline,
                SiteTestedDate = _siteDateOptions.AccessibilitySiteTested.ToString(_siteDateOptions.DateFormat),
                StatementPreparedDate = _siteDateOptions.AccessibilityStatementPrepared.ToString(_siteDateOptions.DateFormat),
                StatementReviewedDate = _siteDateOptions.AccessibilityStatementReviewed.ToString(_siteDateOptions.DateFormat)
            };

            ViewBag.BackLinkToDisplay = returnUrl;
            ViewBag.CurrentPage = returnUrl;

            return View(model);
        }
    }   
}