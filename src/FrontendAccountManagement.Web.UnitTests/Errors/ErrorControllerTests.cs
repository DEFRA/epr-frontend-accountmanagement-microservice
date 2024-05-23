using FrontendAccountManagement.Web.Controllers.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Errors;

[TestClass]
public class ErrorControllerTests
{
    private Mock<HttpContext> _mockHttpContext;
    private Mock<HttpResponse> _mockHttpResponse;
    private ErrorController _errorController;

    [TestInitialize]
    public void Init()
    {
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpResponse = new Mock<HttpResponse>();
        _mockHttpContext.Setup(c => c.Response).Returns(_mockHttpResponse.Object);
        _errorController = new ErrorController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            }
        };
    }

    [TestMethod]
    public void InvokeError_For404_ReturnsPageNotFound()
    {
        // Arrange
        var statusCode = StatusCodes.Status404NotFound;
        _mockHttpResponse.SetupProperty(r => r.StatusCode);

        // Act
        var result = _errorController.Index(statusCode);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("PageNotFound", result.ViewName);
        Assert.AreEqual(statusCode, _mockHttpResponse.Object.StatusCode);
    }

    [TestMethod]
    public void InvokeError_For500_ReturnsError()
    {
        // Arrange
        var statusCode = StatusCodes.Status500InternalServerError;
        _mockHttpResponse.SetupProperty(r => r.StatusCode);

        // Act
        var result = _errorController.Index(statusCode);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Error", result.ViewName);
        Assert.AreEqual(statusCode, _mockHttpResponse.Object.StatusCode);
    }

    [TestMethod]
    public void Index_NoStatusCode_ReturnsErrorViewWithDefaultStatusCode()
    {
        // Arrange
        int? statusCode = null;
        var defaultStatusCode = StatusCodes.Status500InternalServerError;
        _mockHttpResponse.SetupProperty(r => r.StatusCode);

        // Act
        var result = _errorController.Index(statusCode);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Error", result.ViewName);
        Assert.AreEqual(defaultStatusCode, _mockHttpResponse.Object.StatusCode);
    }
}