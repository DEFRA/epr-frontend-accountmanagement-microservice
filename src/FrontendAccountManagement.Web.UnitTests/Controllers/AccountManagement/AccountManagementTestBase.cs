using AutoMapper;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using System.Text.Json;
using FrontendAccountManagement.Web.Utilities.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

public abstract class AccountManagementTestBase
{
    protected const string ModelErrorKey = "Error";
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    protected Mock<HttpContext> HttpContextMock;
    protected Mock<ClaimsPrincipal> UserMock;
    protected Mock<ClaimsIdentity> ClaimsIdentityMock;
    public Mock<ISessionManager<JourneySession>> SessionManagerMock;
    protected Mock<IFacadeService> FacadeServiceMock = new Mock<IFacadeService>();
    protected Mock<IFeatureManager> FeatureManagerMock;
    protected Mock<IOptions<ExternalUrlsOptions>> UrlsOptionMock;
    protected Mock<IOptions<DeploymentRoleOptions>> DeploymentRoleOptionsMock;
    protected Mock<ILogger<AccountManagementController>> LoggerMock;
    protected ITempDataDictionary TempDataDictionary;
    protected Mock<IClaimsExtensionsWrapper> ClaimsExtensionsWrapperMock;
    protected Mock<IMapper> AutoMapperMock;
    protected Mock<ITempDataDictionary> TempDataDictionaryMock;
    protected AccountManagementController SystemUnderTest;

    protected JourneySession JourneySessionMock { get; set; }

    protected void SetupBase(UserData userData = null, string deploymentRole = "", int userServiceRoleId = 0
    , JourneySession journeySession = null)
    {
        HttpContextMock = new Mock<HttpContext>();
        UserMock = new Mock<ClaimsPrincipal>();
        ClaimsIdentityMock = new Mock<ClaimsIdentity>();
        SessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        FeatureManagerMock = new Mock<IFeatureManager>();
         UrlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        DeploymentRoleOptionsMock = new Mock<IOptions<DeploymentRoleOptions>>();
        TempDataDictionary = new TempDataDictionary(this.HttpContextMock.Object, new Mock<ITempDataProvider>().Object);
        ClaimsExtensionsWrapperMock = new Mock<IClaimsExtensionsWrapper>();
        AutoMapperMock = new Mock<IMapper>();

        SetUpUserData(userData);

        journeySession ??= new JourneySession
            { UserData = userData ?? new UserData { ServiceRoleId = userServiceRoleId } };

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult(journeySession));

        DeploymentRoleOptionsMock.Setup(options => options.Value)
            .Returns(new DeploymentRoleOptions { DeploymentRole = deploymentRole });

        UrlsOptionMock.Setup(options => options.Value)
            .Returns(new ExternalUrlsOptions { LandingPageUrl = "/back/to/home" });

        LoggerMock = new Mock<ILogger<AccountManagementController>>();

        SystemUnderTest = new AccountManagementController(
            SessionManagerMock.Object,
            FacadeServiceMock.Object,
            UrlsOptionMock.Object,
            DeploymentRoleOptionsMock.Object,
            LoggerMock.Object,
            ClaimsExtensionsWrapperMock.Object,
            FeatureManagerMock.Object,
            AutoMapperMock.Object);

        SystemUnderTest.ControllerContext.HttpContext = HttpContextMock.Object;
        SystemUnderTest.TempData = this.TempDataDictionary;
    }
    
    private void SetUpUserData(UserData userData)
    {
        var claims = new List<Claim>();
        if (userData != null)
        {
            claims.Add(new(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));
        }

        UserMock.Setup(u => u.Identity).Returns(ClaimsIdentityMock.Object);
        ClaimsIdentityMock.Setup(ci => ci.Claims).Returns(claims);

        HttpContextMock.Setup(x => x.User).Returns(UserMock.Object);
    }

    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
}
