using System;
using System.Threading.Tasks;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    /// <summary>
    /// Tests the Declaration page fuctionality.
    /// </summary>
    [TestClass]
    public class DeclarationTests
    {
        private AccountManagementController TestClass { get; set; }
        private Mock<ISessionManager<JourneySession>> SessionManager { get; set; }
        private Mock<IFacadeService> FacadeService { get; set; }
        private Mock<IOptions<ExternalUrlsOptions>> UrlOptions { get; set; }
        private Mock<IOptions<DeploymentRoleOptions>> DeploymentRoleOptions { get; set; }
        private Mock<ILogger<AccountManagementController>> Logger { get; set; }
        private HeaderDictionary RequestHeaders { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            this.SessionManager = new Mock<ISessionManager<JourneySession>>();
            this.FacadeService = new Mock<IFacadeService>();
            this.UrlOptions = new Mock<IOptions<ExternalUrlsOptions>>();
            this.DeploymentRoleOptions = new Mock<IOptions<DeploymentRoleOptions>>();
            this.Logger = new Mock<ILogger<AccountManagementController>>();
            this.TestClass = new AccountManagementController(
                this.SessionManager.Object,
                this.FacadeService.Object,
                this.UrlOptions.Object,
                this.DeploymentRoleOptions.Object,
                this.Logger.Object);

            // Mock the HTTP context so that we can use it to set the headers.
            this.RequestHeaders = new HeaderDictionary();
            var context = new Mock<HttpContext>();
            context.SetupGet(x => x.Request.Headers).Returns(this.RequestHeaders);
            this.TestClass.ControllerContext.HttpContext = context.Object;
        }

        /// <summary>
        /// Check that the declaration page can be accessed when reaching it via the "Check your details" page.
        /// </summary>
        [TestMethod]
        public async Task CanCallDeclarationFromCheckYourDetails()
        {
            // Arrange
            this.RequestHeaders["Referer"] = "http://some-host/manage-account/check-your-details";

            // Act
            ViewResult result = (ViewResult)await this.TestClass.Declaration();

            // Assert
            Assert.AreEqual("Declaration", result.ViewName);
        }

        /// <summary>
        /// Check that the declaration page can't be accessed when accessing it directly.
        /// </summary>
        [TestMethod]
        public async Task CannotCallDeclarationDirectly()
        {
            // Arrange
            this.RequestHeaders["Referer"] = string.Empty;

            // Act
            RedirectResult result = (RedirectResult)await this.TestClass.Declaration();

            // Assert
            Assert.IsInstanceOfType<RedirectResult>(result);
            Assert.AreEqual("/manage-account/this-page-cannot-be-accessed-directly", result.Url);
        }
    }
}