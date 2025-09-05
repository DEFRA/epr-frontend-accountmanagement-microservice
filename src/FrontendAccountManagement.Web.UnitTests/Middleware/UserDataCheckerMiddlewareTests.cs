using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Services.Interfaces;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Middleware;
using FrontendAccountManagement.Web.Utilities.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using System.Security.Claims;
using Organisation = EPR.Common.Authorization.Models.Organisation;

namespace FrontendAccountManagement.Web.UnitTests.Middleware;

[TestClass]
public class UserDataCheckerMiddlewareTests
{
    private Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private Mock<ClaimsIdentity> _claimsIdentityMock;
    private Mock<HttpContext> _httpContextMock;
    private Mock<RequestDelegate> _requestDelegateMock;
    private Mock<IFacadeService> _facadeServiceMock;
    private Mock<IClaimsExtensionsWrapper> _claimsExtensionsWrapperMock;
    private Mock<IFeatureManager> _featureManagerMock;
    private Mock<IGraphService> _graphServiceMock;
    private Mock<ILogger<UserDataCheckerMiddleware>> _loggerMock;
    private UserDataCheckerMiddleware _systemUnderTest;

    [TestInitialize]
    public void SetUp()
    {
        _claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        _requestDelegateMock = new Mock<RequestDelegate>();
        _httpContextMock = new Mock<HttpContext>();
        _claimsExtensionsWrapperMock = new Mock<IClaimsExtensionsWrapper>();
        _loggerMock = new Mock<ILogger<UserDataCheckerMiddleware>>();
        _facadeServiceMock = new Mock<IFacadeService>();
        _claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        _httpContextMock.Setup(C => C.User).Returns(_claimsPrincipalMock.Object);
        _featureManagerMock = new Mock<IFeatureManager>();
        _graphServiceMock = new Mock<IGraphService>();

        _claimsIdentityMock = new Mock<ClaimsIdentity>();
        _claimsPrincipalMock.Setup(cp => cp.Identity).Returns(_claimsIdentityMock.Object);

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(nameof(FeatureFlags.UseGraphApiForExtendedUserClaims)))
            .ReturnsAsync(false);

        SetupControllerName("SomeControllerName");

        _systemUnderTest = new UserDataCheckerMiddleware(
            _facadeServiceMock.Object,
            _claimsExtensionsWrapperMock.Object,
            _featureManagerMock.Object,
            _graphServiceMock.Object,
            _loggerMock.Object);
    }

    private void SetupControllerName(string controllerName)
    {
        var controllerActionDescriptor = new ControllerActionDescriptor();
        controllerActionDescriptor.ControllerName = controllerName;

        var metadata = new List<object> { controllerActionDescriptor };

        _httpContextMock.Setup(x => x.Features.Get<IEndpointFeature>().Endpoint).Returns(new Endpoint(c => Task.CompletedTask, new EndpointMetadataCollection(metadata), "EndpointName"));
    }

    [TestMethod]
    public async Task Middleware_DoesNotCallGetUserAccount_WhenAnonController()
    {
        // Arrange
        SetupControllerName("Privacy");

        // Act
        await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        _facadeServiceMock.Verify(x => x.GetUserAccount(), Times.Never);
        _requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
    }

    [TestMethod]
    public async Task Middleware_DoesNotCallGetUserAccount_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _claimsPrincipalMock.Setup(x => x.Identity.IsAuthenticated).Returns(false);
        _httpContextMock.Setup(x => x.User).Returns(_claimsPrincipalMock.Object);

        // Act
        await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        _facadeServiceMock.Verify(x => x.GetUserAccount(), Times.Never);
        _requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
    }

    [TestMethod]
    public async Task Middleware_DoesNotCallGetUserAccount_WhenUserDataAlreadyExistsInUserClaims()
    {
        // Arrange
        _claimsIdentityMock.Setup(x => x.IsAuthenticated).Returns(true);
        _claimsIdentityMock.Setup(x => x.Claims).Returns(new List<Claim> { new(ClaimTypes.UserData, "{}") });

        // Act
        await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        _facadeServiceMock.Verify(x => x.GetUserAccount(), Times.Never);
        _requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
    }

    [TestMethod]
    public async Task Middleware_CallsGetUserAccountAndSignsIn_WhenUserDataDoesNotExistInUserClaims()
    {
        // Arrange
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() { }, "authenticationType"));
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(Mock.Of<IAuthenticationService>());
        _httpContextMock.Setup(x => x.User).Returns(claims);
        _httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);
        _facadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(GetUserAccount());

        // Act
        await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        _facadeServiceMock.Verify(x => x.GetUserAccount(), Times.Once);
        _requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
    }

    [TestMethod]
    public async Task Middleware_CallsGetUserAccountAndSignsIn_WhenUserDataDoesNotExistInTheDB()
    {
        // Arrange
        var httpResponse = (UserAccountDto)null;
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "authenticationType"));
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(Mock.Of<IAuthenticationService>());
        _httpContextMock.Setup(x => x.User).Returns(claims);
        _httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);
        _facadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(httpResponse);

        // Act
        await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        _facadeServiceMock.Verify(x => x.GetUserAccount(), Times.Once);
        _requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
    }

    [TestMethod]
    public async Task Middleware_LogsOrgIdsClaim_WhenClaimIsPresentAtSignIn()
    {
        // Arrange
        const string orgIds = "12345";
        const string expectedLog = $"Found claim {ExtensionClaims.OrganisationIdsClaim} with value {orgIds}";

        _claimsIdentityMock.Setup(x => x.IsAuthenticated).Returns(true);
        _claimsIdentityMock.Setup(x => x.Claims).Returns(new List<Claim> { new(ExtensionClaims.OrganisationIdsClaim, orgIds) });
        _claimsExtensionsWrapperMock.Setup(x => x.TryGetOrganisatonIds()).ReturnsAsync(orgIds);

        _facadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(GetUserAccount());

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(nameof(FeatureFlags.UseGraphApiForExtendedUserClaims)))
            .ReturnsAsync(true);

        // Act
        await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        _facadeServiceMock.Verify(x => x.GetUserAccount(), Times.Once);
        _requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);

        _loggerMock.VerifyLog(logger => logger.LogInformation(expectedLog), Times.Once);
    }

    [TestMethod]
    public async Task Middleware_CallsGraphService_WhenOrgIdsClaimIsEmpty()
    {
        // Arrange
        const string orgIds = "123456";

        _claimsIdentityMock.Setup(x => x.IsAuthenticated).Returns(true);
        _claimsIdentityMock.Setup(x => x.Claims).Returns(new List<Claim>());

        _facadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(GetUserAccount());

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(nameof(FeatureFlags.UseGraphApiForExtendedUserClaims)))
            .ReturnsAsync(true);

        // Act
        await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        _graphServiceMock.Verify(x => x.PatchUserProperty(It.IsAny<Guid>(), ExtensionClaims.OrganisationIdsExtensionClaimName, orgIds, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Middleware_DoesNotCallGraphService_WhenOrgIdsClaimIsEmpty_And_GraphApiFeature_IsNotEnabled()
    {
        // Arrange
        const string orgIds = "123456";

        _claimsIdentityMock.Setup(x => x.IsAuthenticated).Returns(true);
        _claimsIdentityMock.Setup(x => x.Claims).Returns(new List<Claim>());

        _facadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(GetUserAccount());

        // Act
        await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        _graphServiceMock.Verify(x => x.PatchUserProperty(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Middleware_DoesNotCallGraphService_WhenOrgIdsClaimMatches()
    {
        // Arrange
        var orgIds = "123456";

        //var claims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() { new(CustomClaimTypes.OrganisationIds, orgIds) }, "authenticationType"));
        //var serviceProviderMock = new Mock<IServiceProvider>();
        //serviceProviderMock.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(Mock.Of<IAuthenticationService>());
        //_httpContextMock.Setup(x => x.User).Returns(claims);
        //_httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);
        //_userAccountServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(GetUserAccount());

        _claimsIdentityMock.Setup(x => x.IsAuthenticated).Returns(true);
        _claimsIdentityMock.Setup(x => x.Claims).Returns(new List<Claim> { new(ExtensionClaims.OrganisationIdsClaim, orgIds) });
        _claimsExtensionsWrapperMock.Setup(x => x.TryGetOrganisatonIds()).ReturnsAsync(orgIds);

        _facadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(GetUserAccount());

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(nameof(FeatureFlags.UseGraphApiForExtendedUserClaims)))
            .ReturnsAsync(true);

        // Act
        await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        _graphServiceMock.Verify(x => x.PatchUserProperty(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Middleware_DoesNotThrowException_WhenGraphServiceIsNull()
    {
        // Arrange
        const string orgIds = "123456";

        _claimsIdentityMock.Setup(x => x.IsAuthenticated).Returns(true);
        _claimsIdentityMock.Setup(x => x.Claims).Returns(new List<Claim> { new(ExtensionClaims.OrganisationIdsClaim, orgIds) });
        _claimsExtensionsWrapperMock.Setup(x => x.TryGetOrganisatonIds()).ReturnsAsync(orgIds);

        _facadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(GetUserAccount());

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(nameof(FeatureFlags.UseGraphApiForExtendedUserClaims)))
            .ReturnsAsync(true);

        _systemUnderTest = new UserDataCheckerMiddleware(
            _facadeServiceMock.Object,
            _claimsExtensionsWrapperMock.Object,
            _featureManagerMock.Object,
            (IGraphService)null,
            _loggerMock.Object);

        // Act
        var act = async () => await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

        // Assert
        await act.Should().NotThrowAsync<Exception>();
    }

    private static UserAccountDto GetUserAccount()
    {
        return new UserAccountDto
        {
            User = new UserData
            {
                Id = Guid.NewGuid(),
                FirstName = "Joe",
                LastName = "Test",
                Email = "JoeTest@something.com",
                RoleInOrganisation = "Test Role",
                EnrolmentStatus = "Enrolled",
                ServiceRole = "Test service role",
                Service = "Test service",
                Organisations = new List<Organisation>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "TestCo",
                        OrganisationNumber = "123456",
                        OrganisationRole = "Producer",
                        OrganisationType = "test type",
                    },
                },
            },
        };
    }
}