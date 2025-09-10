using System.Threading.Tasks;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Cookies;
using FrontendAccountManagement.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;

namespace FrontendAccountManagement.Web.UnitTests.Middleware
{
    [TestClass]
    public class AnalyticsCookieMiddlewareTests
    {
        private Mock<ICookieService> _cookieServiceMock;
        private Mock<IOptions<AnalyticsOptions>> _analyticsOptionsMock;
        private Mock<RequestDelegate> _nextMock;
        private DefaultHttpContext _httpContext;
        private AnalyticsCookieMiddleware _middleware;

        [TestInitialize]
        public void Setup()
        {
            _cookieServiceMock = new Mock<ICookieService>();
            _analyticsOptionsMock = new Mock<IOptions<AnalyticsOptions>>();
            _nextMock = new Mock<RequestDelegate>();
            _httpContext = new DefaultHttpContext();
            _middleware = new AnalyticsCookieMiddleware(_nextMock.Object);
        }

        [TestMethod]
        public async Task InvokeAsync_SetsItems_WhenUserAcceptedCookies()
        {
            // Arrange
            _cookieServiceMock.Setup(x => x.HasUserAcceptedCookies(_httpContext.Request.Cookies)).Returns(true);
            _analyticsOptionsMock.Setup(x => x.Value).Returns(new AnalyticsOptions { TagManagerContainerId = "GTM-1234" });
            _nextMock.Setup(x => x(_httpContext)).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_httpContext, _cookieServiceMock.Object, _analyticsOptionsMock.Object);

            // Assert
            _httpContext.Items[ContextKeys.UseGoogleAnalyticsCookieKey].Should().Be(true);
            _httpContext.Items[ContextKeys.TagManagerContainerIdKey].Should().Be("GTM-1234");
            _nextMock.Verify(x => x(_httpContext), Times.Once);
        }

        [TestMethod]
        public async Task InvokeAsync_SetsItems_WhenUserDidNotAcceptCookies()
        {
            // Arrange
            _cookieServiceMock.Setup(x => x.HasUserAcceptedCookies(_httpContext.Request.Cookies)).Returns(false);
            _analyticsOptionsMock.Setup(x => x.Value).Returns(new AnalyticsOptions { TagManagerContainerId = "GTM-5678" });
            _nextMock.Setup(x => x(_httpContext)).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_httpContext, _cookieServiceMock.Object, _analyticsOptionsMock.Object);

            // Assert
            _httpContext.Items[ContextKeys.UseGoogleAnalyticsCookieKey].Should().Be(false);
            _httpContext.Items[ContextKeys.TagManagerContainerIdKey].Should().Be("GTM-5678");
            _nextMock.Verify(x => x(_httpContext), Times.Once);
        }
    }
}
