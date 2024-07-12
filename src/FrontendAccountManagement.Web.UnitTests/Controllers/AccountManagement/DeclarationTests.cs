using System;
using System.Text;
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
using Newtonsoft.Json.Linq;

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

        private IDictionary<string, byte[]> SessionData { get; set; }

        delegate void SubmitCallback(string key, out byte[] value);

        [TestInitialize]
        public void SetUp()
        {
            this.SessionManager = new Mock<ISessionManager<JourneySession>>();
            this.SessionManager.Setup(session => session.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.Run(() => new Mock<JourneySession>().Object));

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
            
            var mockSession = new Mock<ISession>();
            this.SessionData = new Dictionary<string, byte[]>();
            bool valueExists = false;
            mockSession.Setup(session => session.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Callback(new SubmitCallback((string key, out byte[] value) =>
                {
                    byte[] v;
                    SessionData.TryGetValue(key, out v);
                    value = v;
                    valueExists = value is not null;
                }))
                .Returns(valueExists);

            var context = new Mock<HttpContext>();
            context.SetupGet(x => x.Request.Headers).Returns(this.RequestHeaders);
            context.SetupGet(x => x.Session).Returns(mockSession.Object);
            this.TestClass.ControllerContext.HttpContext = context.Object;
        }

        /// <summary>
        /// Check that the declaration page can be accessed when reaching it via the "Check your details" page.
        /// </summary>
        [TestMethod]
        public async Task CanCallDeclarationFromCheckYourDetails()
        {
            var navigationToken = Guid.NewGuid();

            // Arrange
            this.SessionData["NavigationToken"] = Encoding.UTF8.GetBytes(navigationToken.ToString());

            // Act
            ViewResult result = (ViewResult)await this.TestClass.Declaration(navigationToken.ToString());

            // Assert
            Assert.AreEqual("Declaration", result.ViewName);
        }

        /// <summary>
        /// Check that the declaration page can't be accessed when accessing it directly.
        /// </summary>
        /// <remarks>
        /// The test cases are various combinations of the IDs that the "Check your details" page would send being missing,
        /// or not matching.
        /// </remarks>
        [TestMethod]
        [DataRow(null,null)]
        [DataRow("A", null)]
        [DataRow(null, "B")]
        [DataRow("A", "B")]
        public async Task CannotCallDeclarationDirectly(string sessionToken, string requestToken)
        {
            // Arrange
            if (sessionToken is not null)
            {
                this.SessionData["NavigationToken"] = Encoding.UTF8.GetBytes(sessionToken);
            }

            // Act
            ViewResult result = (ViewResult)await this.TestClass.Declaration(requestToken);

            // Assert
            Assert.AreEqual("Problem", result.ViewName);
        }
    }
}