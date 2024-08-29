using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.ViewComponents;
using FrontendAccountManagement.Web.ViewModels.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.ViewComponents
{
    [TestClass]
    public class CookieBannerViewComponentTests
    {
        [TestMethod]
        [DataRow(null, "Cookies", null, false, false, false)]
        [DataRow(null, "test", "True", false, false, false)]
        [DataRow(null, "test", null, true, false, false)]
        [DataRow(CookieAcceptance.Accept, "test", null, false, true, true)]
        [DataRow(CookieAcceptance.Reject, "test", "True", false, true, false)]
        public void Invoke(
            string cookieAcceptance,
            string controller,
            string consentCookie,
            bool expectedShowBanner,
            bool expectedShowAcknowledgement,
            bool expectedAcceptAnalytics)
        {
            // Arrange

            const string ReturnUrl = "/return";
            const string RequestPath = "/request-path";
            const string RequestQueryString = "?test=1";

            var options = new EprCookieOptions { CookiePolicyCookieName = "test" };

            var viewContextMock = new ViewContext();
            var httpContextMock = new Mock<HttpContext>();
            var viewComponentContextMock = new ViewComponentContext();

            viewContextMock.HttpContext = httpContextMock.Object;
            viewContextMock.TempData = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>())
            {
                { CookieAcceptance.CookieAcknowledgement, cookieAcceptance }
            };
            viewContextMock.RouteData = new RouteData(new RouteValueDictionary
            {
                { "controller", controller }
            });

            httpContextMock.Setup(x => x.Request.Path).Returns(RequestPath);
            httpContextMock.Setup(x => x.Request.Cookies[options.CookiePolicyCookieName]).Returns(consentCookie);
            httpContextMock.Setup(x => x.Request.QueryString).Returns(new QueryString(RequestQueryString));

            viewComponentContextMock.ViewContext = viewContextMock;

            var systemUnderTest = new CookieBannerViewComponent(Options.Create(options));
            systemUnderTest.ViewComponentContext = viewComponentContextMock;

            // Act

            var result = systemUnderTest.Invoke(ReturnUrl) as ViewViewComponentResult;
            var model = result.ViewData.Model as CookieBannerModel;

            // Assert

            Assert.IsNotNull(result);
            Assert.IsNotNull(model);

            Assert.AreEqual(RequestPath, model.CurrentPage);
            Assert.AreEqual(expectedShowBanner, model.ShowBanner);
            Assert.AreEqual(expectedShowAcknowledgement, model.ShowAcknowledgement);
            Assert.AreEqual(expectedAcceptAnalytics, model.AcceptAnalytics);
            Assert.AreEqual($"{ReturnUrl}{RequestQueryString}", model.ReturnUrl);
        }
    }
}
