using AutoMapper;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

public abstract class AccountManagementTestBase
{
    protected const string ModelErrorKey = "Error";
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    protected Mock<HttpContext> HttpContextMock;
    protected Mock<ClaimsPrincipal> UserMock;
    public Mock<ISessionManager<JourneySession>> SessionManagerMock;
    protected Mock<IFacadeService> FacadeServiceMock;
    protected Mock<IOptions<ExternalUrlsOptions>> UrlsOptionMock;
    protected Mock<IOptions<DeploymentRoleOptions>> DeploymentRoleOptionsMock;
    protected Mock<ILogger<AccountManagementController>> LoggerMock;
    protected Mock<ITempDataDictionary> TempDataDictionaryMock;
    protected Mock<IMapper> AutoMapperMock;
    protected AccountManagementController SystemUnderTest;

    protected JourneySession JourneySessionMock { get; set; }

    protected void SetupBase(UserData userData = null, string deploymentRole = "", int userServiceRoleId = 0, UserOrganisationsListModelDto userDataToDispaly = null)
    {
        HttpContextMock = new Mock<HttpContext>();
        UserMock = new Mock<ClaimsPrincipal>();
        SessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        FacadeServiceMock = new Mock<IFacadeService>();
        UrlsOptionMock = new Mock<IOptions<ExternalUrlsOptions>>();
        DeploymentRoleOptionsMock = new Mock<IOptions<DeploymentRoleOptions>>();
        TempDataDictionaryMock = new Mock<ITempDataDictionary>();
        AutoMapperMock = new Mock<IMapper>();

        SetUpUserData(userData, userDataToDispaly);

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult(new JourneySession { UserData = { ServiceRoleId = userServiceRoleId } }));

        FacadeServiceMock.Setup(sm => sm.GetUserAccountForDispaly())
            .Returns(Task.FromResult(userDataToDispaly));

        DeploymentRoleOptionsMock.Setup(options => options.Value)
            .Returns(new DeploymentRoleOptions { DeploymentRole = deploymentRole });

        UrlsOptionMock.Setup(options => options.Value)
            .Returns(new ExternalUrlsOptions { LandingPageUrl = "/back/to/home" });

        LoggerMock = new Mock<ILogger<AccountManagementController>>();
        TempDataDictionaryMock = new Mock<ITempDataDictionary>();

        SystemUnderTest = new AccountManagementController(
            SessionManagerMock.Object,
            FacadeServiceMock.Object,
            UrlsOptionMock.Object,
            DeploymentRoleOptionsMock.Object,
            LoggerMock.Object,
            AutoMapperMock.Object);

        SystemUnderTest.ControllerContext.HttpContext = HttpContextMock.Object;
        SystemUnderTest.TempData = TempDataDictionaryMock.Object;
    }
    
    private void SetUpUserData(UserData userData, UserOrganisationsListModelDto userDataToDispaly)
    {
        var claims = new List<Claim>();
        if (userData != null)
        {
            claims.Add(new(ClaimTypes.UserData, Newtonsoft.Json.JsonConvert.SerializeObject(userData)));
        }
        
        UserMock.Setup(x => x.Claims).Returns(claims);
        HttpContextMock.Setup(x => x.User).Returns(UserMock.Object);
        if (userDataToDispaly == null)
        {
            userDataToDispaly = new UserOrganisationsListModelDto();
            userDataToDispaly.User = new UserDetailsModel() { FirstName = "Test First Name", LastName = "Test Last Name", Telephone = "07545822431"  };
            userDataToDispaly.User.Organisations = new List<OrganisationDetailModel>
            {
                new OrganisationDetailModel() { JobTitle = "Test Job Title", Id = Guid.NewGuid(), Name = "Test Organization Name", OrganisationType = "Companies House Company", OrgAddress = "Test Organisation Address" }
            };
        }
    }

    protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
}
