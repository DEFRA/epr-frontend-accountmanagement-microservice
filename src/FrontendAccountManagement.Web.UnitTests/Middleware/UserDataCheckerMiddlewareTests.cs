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
using Organisation = EPR.Common.Authorization.Models.Organisation;
using FrontendAccountManagement.Web.Utilities.Interfaces;
using Microsoft.AspNetCore.Routing;

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

        _claimsIdentityMock = new Mock<ClaimsIdentity>();
        _claimsPrincipalMock.Setup(cp => cp.Identity).Returns(_claimsIdentityMock.Object);

        SetupControllerName("SomeControllerName");
        
        _systemUnderTest = new UserDataCheckerMiddleware(
            _facadeServiceMock.Object,
            _claimsExtensionsWrapperMock.Object,
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
	public async Task Middleware_DoesNotCallGetUserAccountWithEnrolments_WhenAnonController()
	{
		// Arrange
		SetupControllerName("Privacy");

		// Act
		await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

		// Assert
		_facadeServiceMock.Verify(x => x.GetUserAccountWithEnrolments(It.IsAny<string>()), Times.Never);
		_requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
	}

	[TestMethod]
	public async Task Middleware_DoesNotCallGetUserAccountWithEnrolments_WhenUserIsNotAuthenticated()
	{
		// Arrange
		_claimsPrincipalMock.Setup(x => x.Identity.IsAuthenticated).Returns(false);
		_httpContextMock.Setup(x => x.User).Returns(_claimsPrincipalMock.Object);

		// Act
		await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

		// Assert
		_facadeServiceMock.Verify(x => x.GetUserAccountWithEnrolments(It.IsAny<string>()), Times.Never);
		_requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
	}

	[TestMethod]
	public async Task Middleware__DoesNotCallGetUserAccountWithEnrolments_WhenUserDataAlreadyExistsInUserClaims()
	{
		// Arrange
		_claimsIdentityMock.Setup(x => x.IsAuthenticated).Returns(true);
		_claimsIdentityMock.Setup(x => x.Claims).Returns(new List<Claim> { new(ClaimTypes.UserData, "{}") });

		// Act
		await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

		// Assert
		_facadeServiceMock.Verify(x => x.GetUserAccountWithEnrolments(It.IsAny<string>()), Times.Never);
		_requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
	}

	[TestMethod]
	public async Task Middleware_CallsGetUserAccountWitnEnrolmentsAndSignsIn_WhenUserDataDoesNotExistInUserClaims_ForReEx()
	{
		// Arrange
		SetupControllerName("ReExAccountManagement");

		var claims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() { }, "authenticationType"));
		var serviceProviderMock = new Mock<IServiceProvider>();
		serviceProviderMock.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(Mock.Of<IAuthenticationService>());
		_httpContextMock.Setup(x => x.User).Returns(claims);
		_httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);

		var organisationId = Guid.NewGuid().ToString();
		var routeValues = new RouteValueDictionary { { "organisationId", organisationId } };
		var requestMock = new Mock<HttpRequest>();
		requestMock.Setup(x => x.RouteValues).Returns(routeValues);
		_httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

		_facadeServiceMock.Setup(x => x.GetUserAccountWithEnrolments(It.IsAny<string>())).ReturnsAsync(GetUserAccount());

		// Act
		await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

		// Assert
		_facadeServiceMock.Verify(x => x.GetUserAccountWithEnrolments(It.IsAny<string>()), Times.Once);
		_requestDelegateMock.Verify(x => x(_httpContextMock.Object), Times.Once);
	}

	[TestMethod]
	public async Task Middleware_CallsGetUserAccountWithEnrolmentsAndSignsIn_WhenUserDataDoesNotExistInTheDB_ForReEx()
	{
		// Arrange
		SetupControllerName("ReExAccountManagement");

		var httpResponse = (UserAccountDto)null;
		var claims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "authenticationType"));
		var serviceProviderMock = new Mock<IServiceProvider>();
		serviceProviderMock.Setup(x => x.GetService(typeof(IAuthenticationService))).Returns(Mock.Of<IAuthenticationService>());
		_httpContextMock.Setup(x => x.User).Returns(claims);
		_httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);

		var organisationId = Guid.NewGuid().ToString();
		var routeValues = new RouteValueDictionary { { "organisationId", organisationId } };
		var requestMock = new Mock<HttpRequest>();
		requestMock.Setup(x => x.RouteValues).Returns(routeValues);
		_httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);

		_facadeServiceMock.Setup(x => x.GetUserAccountWithEnrolments(It.IsAny<string>())).ReturnsAsync(httpResponse);

		// Act
		await _systemUnderTest.InvokeAsync(_httpContextMock.Object, _requestDelegateMock.Object);

		// Assert
		_facadeServiceMock.Verify(x => x.GetUserAccountWithEnrolments(It.IsAny<string>()), Times.Once);
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