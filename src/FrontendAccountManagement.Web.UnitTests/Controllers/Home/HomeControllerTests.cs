using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Controllers.Home;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.Home;

[TestClass]
public class HomeControllerTests
{
    private Mock<HttpContext> _httpContextMock;
    private Mock<HttpResponse> _httpResponseMock;
    private Mock<IOptions<EprCookieOptions>> _cookieConfig;
    private Mock<IResponseCookies> _responseCookiesMock;
    private Mock<ISession> _sessionMock;
    private HomeController _systemUnderTest;
    private const string SessionCookieName = "SessionCookieName";

    private void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _httpResponseMock = new Mock<HttpResponse>();
        _responseCookiesMock = new Mock<IResponseCookies>();
        _cookieConfig = new Mock<IOptions<EprCookieOptions>>();
        _sessionMock = new Mock<ISession>();

        _cookieConfig.Setup(m => m.Value).Returns(new EprCookieOptions { SessionCookieName = "SessionCookieName"});
        _httpContextMock.Setup(m => m.Response).Returns(_httpResponseMock.Object);
        _httpContextMock.Setup(m => m.Session).Returns(_sessionMock.Object);
        _httpResponseMock.Setup(m => m.Cookies).Returns(_responseCookiesMock.Object);

        _systemUnderTest = new HomeController(_cookieConfig.Object);
        _systemUnderTest.ControllerContext.HttpContext = _httpContextMock.Object;
    }
    
    [TestMethod]
    public void OnSignOut_DeleteUserSessionCookie()
    {
        // Arrange
        Setup();

        // Act
        _systemUnderTest.SignedOut();

        // Assert
        _responseCookiesMock.Verify(x => x.Delete(SessionCookieName), Times.Once);
    }
}