using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Cookies;
using FrontendAccountManagement.Web.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.Cookies
{
    [TestClass]
    public class CookiesControllerTests
    {
        private readonly Mock<HttpContext> _httpContextMock = new();

        private readonly Mock<ICookieService> _cookieServiceMock = new();

        [TestMethod]
        [DataRow("test", CookieAcceptance.Accept)]
        [DataRow("test-2", CookieAcceptance.Reject)]
        public void UpdateAcceptance_UserSubmitsDecision_SavesAndRedirectsToReturnUrl(string returnUrl, string cookiesDecision)
        {
            // Arrange

            var requestCookies = null as IRequestCookieCollection;
            var responseCookies = null as IResponseCookies;

            _httpContextMock.Setup(x => x.Request.Cookies).Returns(requestCookies);
            _httpContextMock.Setup(x => x.Response.Cookies).Returns(responseCookies);

            var systemUnderTest = GetSystemUnderTest();

            // Act

            var localRedirectResult = systemUnderTest.UpdateAcceptance(returnUrl, cookiesDecision);

            // Assert

            Assert.IsNotNull(localRedirectResult);
            Assert.AreEqual(returnUrl, localRedirectResult.Url);
            Assert.AreEqual(systemUnderTest.TempData[CookieAcceptance.CookieAcknowledgement], cookiesDecision);

            _cookieServiceMock.Verify(
                x => x.SetCookieAcceptance(
                    cookiesDecision == CookieAcceptance.Accept, requestCookies, responseCookies), Times.Once);
        }

        [TestMethod]
        public void AcknowledgeAcceptance_UserSubmitsNotLocalUrl_RedirectsToHomePath()
        {
            // Arrange

            const string HOME_PATH = "home";
            const string RETURN_URL = "test";

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(HOME_PATH);
            urlHelperMock.Setup(x => x.IsLocalUrl(RETURN_URL)).Returns(false);

            var systemUnderTest = GetSystemUnderTest();
            systemUnderTest.Url = urlHelperMock.Object;

            // Act

            var localRedirectResult = systemUnderTest.AcknowledgeAcceptance(RETURN_URL);

            // Assert

            Assert.IsNotNull(localRedirectResult);
            Assert.AreEqual(HOME_PATH, localRedirectResult.Url);

            urlHelperMock.Verify(x => x.IsLocalUrl(RETURN_URL), Times.Once);
            urlHelperMock.Verify(x => x.Action(It.IsAny<UrlActionContext>()), Times.Once);
        }

        [TestMethod]
        public void AcknowledgeAcceptance_UserSubmitsLocalUrl_RedirectsReturnUrl()
        {
            // Arrange

            const string RETURN_URL = "test";

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(x => x.IsLocalUrl(RETURN_URL)).Returns(true);

            var systemUnderTest = GetSystemUnderTest();
            systemUnderTest.Url = urlHelperMock.Object;

            // Act

            var localRedirectResult = systemUnderTest.AcknowledgeAcceptance(RETURN_URL);

            // Assert

            Assert.IsNotNull(localRedirectResult);
            Assert.AreEqual(RETURN_URL, localRedirectResult.Url);

            urlHelperMock.Verify(x => x.IsLocalUrl(RETURN_URL), Times.Once);
        }

        private CookiesController GetSystemUnderTest()
        {
            var controller = new CookiesController(_cookieServiceMock.Object);

            controller.ControllerContext.HttpContext = _httpContextMock.Object;
            controller.TempData = new TempDataDictionary(
                controller.ControllerContext.HttpContext,
                Mock.Of<ITempDataProvider>());

            return controller;
        }
    }
}
