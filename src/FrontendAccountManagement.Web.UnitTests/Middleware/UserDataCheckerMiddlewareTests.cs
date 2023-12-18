using System.Security.Claims;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models;
using Microsoft.Extensions.Logging;
using FrontendAccountManagement.Core.Services;
using FrontendAccountManagement.Web.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Middleware;

[TestClass]
public class UserDataCheckerMiddlewareTests
{
    private Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private Mock<HttpContext> _httpContextMock;
    private Mock<RequestDelegate> _requestDelegateMock;
    private Mock<IFacadeService> _facadeServiceMock;
    private Mock<ILogger<UserDataCheckerMiddleware>> _loggerMock;
    private UserDataCheckerMiddleware _systemUnderTest;

    [TestInitialize]
    public void SetUp()
    {
        _claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        _requestDelegateMock = new Mock<RequestDelegate>();
        _httpContextMock = new Mock<HttpContext>();
        _loggerMock = new Mock<ILogger<UserDataCheckerMiddleware>>();
        _facadeServiceMock = new Mock<IFacadeService>();

        SetupControllerName("SomeControllerName");
        
        _systemUnderTest = new UserDataCheckerMiddleware(_facadeServiceMock.Object, _loggerMock.Object);
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
        _claimsPrincipalMock.Setup(x => x.Identity.IsAuthenticated).Returns(true);
        _claimsPrincipalMock.Setup(x => x.Claims).Returns(new List<Claim> { new(ClaimTypes.UserData, "{}") });
        _httpContextMock.Setup(x => x.User).Returns(_claimsPrincipalMock.Object);

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
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "authenticationType"));
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
                        OrganisationRole = "Producer",
                        OrganisationType = "test type",
                    },
                },
            },
        };
    }
}