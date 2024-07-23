using System.Threading.Tasks;
using AutoMapper;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
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
        private Mock<IMapper> Mapper { get; set; }
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
            this.Mapper = new Mock<IMapper>();
            this.TestClass = new AccountManagementController(
                this.SessionManager.Object,
                this.FacadeService.Object,
                this.UrlOptions.Object,
                this.DeploymentRoleOptions.Object,
                this.Logger.Object,
                this.Mapper.Object);

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
        public async Task DeclarationGet_CanCall()
        {
            // Act
            var result = (ViewResult)await this.TestClass.Declaration();

            // Assert
            Assert.AreEqual("Declaration", result.ViewName);
        }

        /// <summary>
        /// Checks that a bad request returned if the model is not valid.
        /// </summary>
        [TestMethod]
        public async Task DeclarationGet_ErrorsWhenModelIsBad()
        {
            // Arrange
            this.TestClass.ModelState.AddModelError("Error", "Something went wrong.");

            // Act
            IActionResult result = await this.TestClass.Declaration();

            // Assert
            Assert.IsInstanceOfType<BadRequestResult>(result);
        }
        
        /// <summary>
        /// Checks that the declaration page's post action redirects to the "Details change requested" page.
        /// </summary>
        [TestMethod]
        public async Task DeclarationPost_CanCall()
        {
            // Act
            var result = (RedirectToActionResult)await this.TestClass.DeclarationPost();

            // Assert
            Assert.AreEqual("DetailsChangeRequested", result.ActionName);
        }
    }
}