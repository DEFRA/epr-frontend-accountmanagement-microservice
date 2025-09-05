using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Security.Claims;
using System.Text.Json;

namespace FrontendAccountManagement.Web.UnitTests.Utilities;

[TestClass]
public class ClaimsExtensionsWrapperTests
{
    private Mock<IHttpContextAccessor> _mockContextAccessor;
    private Mock<HttpContext> _httpContextMock;
    private ClaimsExtensionsWrapper _claimsExtensionWrapper;

    [TestInitialize]
    public void Init()
    {
        _httpContextMock = new Mock<HttpContext>();

        _mockContextAccessor = new Mock<IHttpContextAccessor>();
        _mockContextAccessor.Setup(c => c.HttpContext).Returns(_httpContextMock.Object);

        _claimsExtensionWrapper = new ClaimsExtensionsWrapper(
            _mockContextAccessor.Object);
    }

    [TestMethod]
    public async Task UpdateUserDataClaimsAndSignInAsync_SignsInSuccessfully()
    {
        // Arrange
        var newFirstName = "New";
        var userData = new UserData
        {
            FirstName = "Old"
        };

        var userDataNew = new UserData
        {
            FirstName = newFirstName
        };

        // Create a mock identity
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)),
        };

        var identity = new ClaimsIdentity(claims);

        // Create a mock ClaimsPrincipal
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _httpContextMock.Setup(c => c.User).Returns(claimsPrincipal);

        var mockFeatureCollection = new Mock<IFeatureCollection>();
        var mockAuthenticateResultFeature = new Mock<IAuthenticateResultFeature>();
        var mockAuthenticateResult = new Mock<AuthenticateResult>();

        // Setup the AuthenticateResultFeature to return the AuthenticateResult
        mockAuthenticateResultFeature.Setup(arf => arf.AuthenticateResult).Returns(mockAuthenticateResult.Object);

        // Setup the Features collection to return the AuthenticateResultFeature
        mockFeatureCollection.Setup(fc => fc.Get<IAuthenticateResultFeature>()).Returns(mockAuthenticateResultFeature.Object);

        // Setup the HttpContext to return the mocked Features collection
        _httpContextMock.Setup(hc => hc.Features).Returns(mockFeatureCollection.Object);

        var mockAuthenticationService = new Mock<IAuthenticationService>();

        // Setup IAuthenticationService to do nothing when SignInAsync is called
        mockAuthenticationService
            .Setup(s => s.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var mockRequestServices = new Mock<IServiceProvider>();

        // Setup IServiceProvider to return the mock IAuthenticationService
        mockRequestServices
            .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
            .Returns(mockAuthenticationService.Object);

        _httpContextMock
            .Setup(hc => hc.RequestServices)
            .Returns(mockRequestServices.Object);

        // Act
        await _claimsExtensionWrapper.UpdateUserDataClaimsAndSignInAsync(
            userDataNew);

        // Assert
        mockAuthenticationService.Verify(s => s.SignInAsync(
            It.IsAny<HttpContext>(),
            CookieAuthenticationDefaults.AuthenticationScheme,
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<AuthenticationProperties>()),
            Times.Once);
        claimsPrincipal.Claims.Count().Should().Be(1);
        claimsPrincipal.HasClaim(c => c.Type == ClaimTypes.UserData).Should().BeTrue();
        JsonSerializer.Deserialize<UserData>(claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.UserData).Value).FirstName.Should().Be(newFirstName);
    }

    [TestMethod]
    public async Task TryGetOrganisatonIds_ReturnExpectedOrganisationIds_WhenClaimPresent()
    {
        // Arrange
        const string organisationIds = "012345,67890";

        var claims = new List<Claim>
        {
            new Claim(ExtensionClaims.OrganisationIdsClaim, organisationIds)
        };

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _httpContextMock.Setup(c => c.User).Returns(claimsPrincipal);

        // Act
        var result = await _claimsExtensionWrapper.TryGetOrganisatonIds();

        // Assert
        result.Should().Be(organisationIds);
    }

    [TestMethod]
    public async Task TryGetOrganisatonIds_ReturnsNull_WhenNoOrganisationIdsClaimPresent()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // no claims
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _httpContextMock.Setup(c => c.User).Returns(claimsPrincipal);

        // Act
        var result = await _claimsExtensionWrapper.TryGetOrganisatonIds();

        // Assert
        result.Should().BeNull();
    }


    [TestMethod]
    public async Task TryGetOrganisatonIds_ShouldReturnNull_WhenClaimsIsNull()
    {
        // Arrange
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(x => x.Claims).Returns((IEnumerable<Claim>)null);

        _httpContextMock.Setup(c => c.User).Returns(claimsPrincipalMock.Object);

        // Act
        var result = await _claimsExtensionWrapper.TryGetOrganisatonIds();

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task TryGetOrganisatonIds_ShouldReturnNull_WhenClaimsPrincipalIsNull()
    {
        // Arrange
        _httpContextMock.Setup(c => c.User).Returns(default(ClaimsPrincipal));

        // Act
        var result = await _claimsExtensionWrapper.TryGetOrganisatonIds();

        // Assert
        result.Should().BeNull();
    }
}