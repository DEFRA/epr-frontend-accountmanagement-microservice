using FrontendAccountManagement.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Middleware;

[TestClass]
public class SecurityHeaderMiddlewareTests
{
    private Mock<RequestDelegate> _mockRequestDelegate = null!;
    private Mock<IConfiguration> _mockConfiguration = null!;
    private SecurityHeaderMiddleware _middleware = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockRequestDelegate = new Mock<RequestDelegate>();
        _mockConfiguration = new Mock<IConfiguration>();
        _middleware = new SecurityHeaderMiddleware(_mockRequestDelegate.Object);
    }

    [DataTestMethod]
    [DataRow("base-uri 'none'")]
    [DataRow("require-trusted-types-for 'script'")]
    public async Task Invoke_ShouldContainContentSecurityPolicyDirectives(string expectedDirective)
    {
        // Arrange
        var context = new DefaultHttpContext();
        _mockConfiguration.Setup(config => config["AzureAdB2C:Instance"])
            .Returns("https://mocked-instance.b2clogin.com/");

        // Act
        await _middleware.Invoke(context, _mockConfiguration.Object);

        // Assert
        var contentSecurityPolicy = context.Response.Headers.ContentSecurityPolicy.ToString();
        var directives = contentSecurityPolicy.Split(';');

        Assert.IsTrue(directives.Contains(expectedDirective));
    }
}
