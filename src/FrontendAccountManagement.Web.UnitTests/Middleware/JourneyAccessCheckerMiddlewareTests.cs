using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Web.Configs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace FrontendAccountManagement.Web.UnitTests.Middleware;

using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Attributes;
using FrontendAccountManagement.Web.Middleware;
using FrontendAccountManagement.Web.Sessions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class JourneyAccessCheckerMiddlewareTests
{
    private Mock<HttpContext> _httpContextMock;
    private Mock<HttpResponse> _httpResponseMock;
    private Mock<HttpRequest> _httpRequestMock;
    private Mock<IFeatureCollection> _featureCollectionMock;
    private Mock<IEndpointFeature> _endpointFeatureMock;
    private Mock<ISessionManager<JourneySession>> _sessionManagerMock;
    private readonly Mock<ClaimsPrincipal> UserMock = new();
    private readonly List<Claim> _claims = new();

    private JourneyAccessCheckerMiddleware _middleware;

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _httpResponseMock = new Mock<HttpResponse>();
        _httpRequestMock = new Mock<HttpRequest>();
        _featureCollectionMock = new Mock<IFeatureCollection>();
        _endpointFeatureMock = new Mock<IEndpointFeature>();
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();

        var externalUrlsOptions = new ExternalUrlsOptions
        {
            FrontEndCreationBaseUrl = "/creat-account"
        };

        var options = Options.Create(externalUrlsOptions);
        var userData = new UserData
        {
            Id = Guid.NewGuid(),
            Organisations = new List<Organisation> { new() { Id = Guid.NewGuid() } },
            RoleInOrganisation = "Admin",
        };

        _claims.Add(new(ClaimTypes.UserData, Newtonsoft.Json.JsonConvert.SerializeObject(userData)));

        UserMock.Setup(x => x.Claims).Returns(_claims);
        _httpRequestMock.Setup(x => x.Scheme).Returns("http");
        _httpRequestMock.Setup(x => x.Host).Returns(HostString.FromUriComponent("http://localhost:8080"));
        _httpRequestMock.Setup(x => x.PathBase).Returns(PathString.FromUriComponent("/manage-account"));
        _httpContextMock.Setup(x => x.Response).Returns(_httpResponseMock.Object);
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequestMock.Object);
        _httpContextMock.Setup(x => x.Features).Returns(_featureCollectionMock.Object);
        _httpContextMock.Setup(x => x.User).Returns(UserMock.Object);
        _featureCollectionMock.Setup(x => x.Get<IEndpointFeature>()).Returns(_endpointFeatureMock.Object);

        _middleware = new JourneyAccessCheckerMiddleware(_ => Task.CompletedTask, options, NullLogger<JourneyAccessCheckerMiddleware>.Instance);
    }

    [TestMethod]
    [DataRow(PagePath.TeamMemberEmail, PagePath.ManageAccount)]
    [DataRow(PagePath.TeamMemberEmail, PagePath.ManageAccount, PagePath.ManageAccount)]
    public async Task GivenAccessRequiredPage_WhichIsNotPartOfTheVisitedURLs_WhenInvokeCalled_ThenRedirectedToExpectedPage(
        string pageUrl, string expectedPage, params string[] visitedUrls)
    {
        // Arrange
        var session = new JourneySession { AccountManagementSession = new AccountManagementSession { Journey = visitedUrls.ToList() } };
        var expectedURL = $"/manage-account/{expectedPage}";

        SetupEndpointMock(new JourneyAccessAttribute(pageUrl));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(expectedURL), Times.Once);
    }

    [TestMethod]
    public async Task GivenAccessRequiredPageToManagePermissions_WhenIdProvidedAndJourneyTypeIsManagePermissionStart_ThenDoNotRedirect()
    {
        // Arrange
        var id = Guid.NewGuid();
        var session = new JourneySession();

        SetupEndpointMock(new JourneyAccessAttribute(PagePath.ChangeAccountPermissions, JourneyName.ManagePermissionsStart));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var routingFeatureMock = new Mock<IRoutingFeature>();
        routingFeatureMock.Setup(x => x.RouteData).Returns(new RouteData(new RouteValueDictionary { { "id", id } }));

        _httpContextMock.Setup(x => x.Features.Get<IRoutingFeature>()).Returns(routingFeatureMock.Object);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task GivenAccessRequiredPageToManagePermissions_WhenIdNotProvided_ThenRedirect()
    {
        // Arrange
        var session = new JourneySession();
        var expectedURL = $"/manage-account/{PagePath.ManageAccount}";

        SetupEndpointMock(new JourneyAccessAttribute(PagePath.ChangeAccountPermissions, JourneyName.ManagePermissionsStart));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var routingFeatureMock = new Mock<IRoutingFeature>();
        routingFeatureMock.Setup(x => x.RouteData).Returns(new RouteData(new RouteValueDictionary { { "id", null } }));

        _httpContextMock.Setup(x => x.Features.Get<IRoutingFeature>()).Returns(routingFeatureMock.Object);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(expectedURL), Times.Once);
    }

    [TestMethod]
    public async Task GivenAccessRequiredPageToManagePermissions_WhenJourneyTypeIsManagePermissionAndPermissionManagementSessionIsEmpty_ThenRedirect()
    {
        // Arrange
        var id = Guid.NewGuid();
        var session = new JourneySession();
        var expectedURL = $"/manage-account/{PagePath.ManageAccount}";

        SetupEndpointMock(new JourneyAccessAttribute(PagePath.ChangeAccountPermissions, JourneyName.ManagePermissions));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var routingFeatureMock = new Mock<IRoutingFeature>();
        routingFeatureMock.Setup(x => x.RouteData).Returns(new RouteData(new RouteValueDictionary { { "id", id } }));

        _httpContextMock.Setup(x => x.Features.Get<IRoutingFeature>()).Returns(routingFeatureMock.Object);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(expectedURL), Times.Once);
    }


    [TestMethod]
    public async Task GivenAccessRequiredPageToManagePermissions_WhenJourneyTypeIsManagePermissionAndPermissionManagementSessionHasNoRequiredValue_ThenRedirect()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedURL = $"/manage-account/{PagePath.ChangeAccountPermissions}/{id}";
        var session = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id,
                        Journey = new List<string> { $"{PagePath.ChangeAccountPermissions}/{id}" }
                    }
                }
            }
        };
        
        SetupEndpointMock(new JourneyAccessAttribute(PagePath.RelationshipWithOrganisation, JourneyName.ManagePermissions));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var routingFeatureMock = new Mock<IRoutingFeature>();
        routingFeatureMock.Setup(x => x.RouteData).Returns(new RouteData(new RouteValueDictionary { { "id", id } }));

        _httpContextMock.Setup(x => x.Features.Get<IRoutingFeature>()).Returns(routingFeatureMock.Object);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(expectedURL), Times.Once);
    }

    [TestMethod]
    public async Task GivenAccessRequiredPageToManagePermissions_WhenJourneyTypeIsManagePermissionAndPermissionManagementSessionHasRequiredValue_ThenRedirect()
    {
        // Arrange
        var id = Guid.NewGuid();
        var session = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id,
                        Journey = new List<string>
                        {
                            $"{PagePath.ChangeAccountPermissions}/{id}",
                            $"{PagePath.RelationshipWithOrganisation}/{id}",
                        }
                    }
                }
            }
        };

        SetupEndpointMock(new JourneyAccessAttribute(PagePath.RelationshipWithOrganisation, JourneyName.ManagePermissions));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        var routingFeatureMock = new Mock<IRoutingFeature>();
        routingFeatureMock.Setup(x => x.RouteData).Returns(new RouteData(new RouteValueDictionary { { "id", id } }));

        _httpContextMock.Setup(x => x.Features.Get<IRoutingFeature>()).Returns(routingFeatureMock.Object);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task GivenAccessRequiredPageToManagePermissions_WhenJourneySessionIsNull_ThenRedirect()
    {
        // Arrange
        const string firstPageUrl = $"/manage-account/{PagePath.ManageAccount}";
        var id = Guid.NewGuid();

        SetupEndpointMock(new JourneyAccessAttribute(PagePath.RelationshipWithOrganisation, JourneyName.ManagePermissions));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync((JourneySession)null);

        var routingFeatureMock = new Mock<IRoutingFeature>();
        routingFeatureMock.Setup(x => x.RouteData).Returns(new RouteData(new RouteValueDictionary { { "id", id } }));

        _httpContextMock.Setup(x => x.Features.Get<IRoutingFeature>()).Returns(routingFeatureMock.Object);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(firstPageUrl), Times.Once);
    }

    [TestMethod]
    [DataRow(PagePath.TeamMemberEmail, PagePath.ManageAccount, PagePath.TeamMemberEmail)]
    public async Task GivenAccessRequiredPage_WhichIsPartOfTheVisitedURLs_WhenInvokeCalled_ThenNoRedirectionHappened(
        string pageUrl, params string[] visitedUrls)
    {
        // Arrange
        var session = new JourneySession { AccountManagementSession = new AccountManagementSession { Journey = visitedUrls.ToList() } };

        SetupEndpointMock(new JourneyAccessAttribute(pageUrl));

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    [DataRow(PagePath.TeamMemberEmail)]
    public async Task GivenAccessRequiredPage_WithoutStoredSession_WhenInvokeCalled_ThenRedirectedToFirstPage(string pageUrl)
    {
        // Arrange
        const string firstPageUrl = $"/manage-account/{PagePath.ManageAccount}";
        SetupEndpointMock(new JourneyAccessAttribute(pageUrl));

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(firstPageUrl), Times.Once);
    }

    [TestMethod]
    [DataRow(PagePath.ManageAccount)]
    public async Task GivenNoAccessRequiredPage_WhenInvokeCalled_ThenNoRedirectionHappened(string pageUrl)
    {
        // Arrange
        SetupEndpointMock();

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task GivenNoUserFound_WhenInvokeCalled_ThenRedirectsToCreateAccount()
    {
        // Arrange
        _claims.Add(new(ClaimTypes.UserData, Newtonsoft.Json.JsonConvert.SerializeObject(new UserData())));

        UserMock.Setup(x => x.Claims).Returns(_claims);
        _httpContextMock.Setup(x => x.User).Returns(UserMock.Object);

        // Act
        await _middleware.Invoke(_httpContextMock.Object, _sessionManagerMock.Object);

        // Assert
        _httpResponseMock.Verify(x => x.Redirect(It.IsAny<string>()), Times.Never);
    }

    private void SetupEndpointMock(params object[] attributes)
    {
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(attributes), null);

        _endpointFeatureMock.Setup(x => x.Endpoint).Returns(endpoint);
    }
}
