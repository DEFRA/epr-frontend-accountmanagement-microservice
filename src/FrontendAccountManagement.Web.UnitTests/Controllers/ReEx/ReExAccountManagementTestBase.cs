using System.Security.Claims;
using System.Text.Json;
using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.ReEx;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.ReEx;

public class ReExAccountManagementTestBase
{
    protected const string ModelErrorKey = "Error";
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    protected Mock<HttpContext> HttpContextMock;
    protected Mock<ClaimsPrincipal> UserMock;
    protected Mock<ClaimsIdentity> ClaimsIdentityMock;
    public Mock<ISessionManager<JourneySession>> SessionManagerMock;
    protected Mock<IFacadeService> FacadeServiceMock = new();
    protected Mock<IFeatureManager> FeatureManagerMock;
    protected Mock<IOptions<ExternalUrlsOptions>> UrlsOptionMock;
    protected Mock<ILogger<ReExAccountManagementController>> LoggerMock;
    protected ITempDataDictionary TempDataDictionary;
    protected ReExAccountManagementController SystemUnderTest;

    protected JourneySession JourneySessionMock { get; set; }
    protected Guid OrganisationId { get; set; } = new Guid("ad521fbb-f255-4829-8f50-e74738d52a00");

    protected void SetupBase(UserData userData = null)
    {
        HttpContextMock = new Mock<HttpContext>();
        UserMock = new Mock<ClaimsPrincipal>();
        ClaimsIdentityMock = new Mock<ClaimsIdentity>();
        SessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        FeatureManagerMock = new Mock<IFeatureManager>();
        UrlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        TempDataDictionary = new TempDataDictionary(this.HttpContextMock.Object, new Mock<ITempDataProvider>().Object);

        FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
            .ReturnsAsync(true);

        SetUpUserData(userData);
        
        JourneySessionMock = new JourneySession
        {
            ReExAccountManagementSession = new ReExAccountManagementSession
            {
                Journey = new List<string>
                {
                    PagePath.ManageAccount,
                    PagePath.TeamMemberEmail,
                    PagePath.TeamMemberPermissions
                },
                AddUserJourney = new AddUserJourneyModel(),
                OrganisationId = OrganisationId
            }
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(JourneySessionMock);

        UrlsOptionMock.Setup(options => options.Value)
            .Returns(new ExternalUrlsOptions { LandingPageUrl = "/back/to/home" });

        LoggerMock = new Mock<ILogger<ReExAccountManagementController>>();

        SystemUnderTest = new ReExAccountManagementController(
            SessionManagerMock.Object,
            FacadeServiceMock.Object);

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