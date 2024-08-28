using FluentAssertions;
using FrontendAccountManagement.Web.Controllers.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.Account
{
    [TestClass]
    public class AccountControllerTests
    {
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<HttpContext> _httpContextMock;
        private Mock<HttpRequest> _httpRequestMock;
        private Mock<IServiceProvider> _serviceProviderMock;

        [TestInitialize]
        public void SetUp()
        {
            _urlHelperMock = new Mock<IUrlHelper>();
            _httpContextMock = new Mock<HttpContext>();
            _httpRequestMock = new Mock<HttpRequest>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _httpContextMock.Setup(ctx => ctx.Request).Returns(_httpRequestMock.Object);
            _httpContextMock.Setup(ctx => ctx.RequestServices).Returns(_serviceProviderMock.Object);
        }

        [TestMethod]
        public void SignIn_ValidRedirectUri_ShouldReturnChallengeResult()
        {
            // Arrange
            var controller = new AccountController();
            controller.Url = _urlHelperMock.Object;
            string redirectUri = "/home";
            string scheme = "TestScheme";

            _urlHelperMock.Setup(x => x.IsLocalUrl(redirectUri)).Returns(true);
            _urlHelperMock.Setup(x => x.Content(redirectUri)).Returns(redirectUri);

            // Act
            var result = controller.SignIn(scheme, redirectUri);

            // Assert
            result.Should().BeOfType<ChallengeResult>();
            var challengeResult = result as ChallengeResult;
            challengeResult!.Properties.RedirectUri.Should().Be(redirectUri);
            challengeResult.AuthenticationSchemes.Should().Contain(scheme);
        }

        [TestMethod]
        public void SignIn_InvalidRedirectUri_ShouldReturnChallengeResultWithDefaultRedirect()
        {
            // Arrange
            var controller = new AccountController();
            controller.Url = _urlHelperMock.Object;
            string redirectUri = "http://malicious.com";
            string defaultRedirect = "~/";
            string scheme = "TestScheme";

            _urlHelperMock.Setup(x => x.IsLocalUrl(redirectUri)).Returns(false);
            _urlHelperMock.Setup(x => x.Content(defaultRedirect)).Returns(defaultRedirect);

            // Act
            var result = controller.SignIn(scheme, redirectUri);

            // Assert
            result.Should().BeOfType<ChallengeResult>();
            var challengeResult = result as ChallengeResult;
            challengeResult!.Properties.RedirectUri.Should().Be(defaultRedirect);
            challengeResult.AuthenticationSchemes.Should().Contain(scheme);
        }

        [TestMethod]
        public void SignOut_AppServicesAadAuthenticationDisabled_ShouldReturnSignOutResult()
        {
            // Arrange
            var controller = new AccountController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            };
            controller.Url = _urlHelperMock.Object;

            var callbackUrl = "/home/SignedOut";
            string scheme = "TestScheme";

            _urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns(callbackUrl);

            // Act
            var result = controller.SignOut(scheme);

            // Assert
            result.Should().BeOfType<SignOutResult>();
            var signOutResult = result as SignOutResult;
            signOutResult!.Properties.RedirectUri.Should().Be(callbackUrl);
            signOutResult.AuthenticationSchemes.Should().Contain(CookieAuthenticationDefaults.AuthenticationScheme);
            signOutResult.AuthenticationSchemes.Should().Contain(scheme);
        }
    }
}
