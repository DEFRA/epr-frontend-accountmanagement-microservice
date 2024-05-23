using FrontendAccountManagement.Web.Controllers.Errors;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FrontendAccountManagement.Web.UnitTests.Errors;

[TestClass]
public class ErrorControllerTests
{
    private ErrorController _errorController;

    [TestInitialize]
    public void Setup()
    {
        _errorController = new ErrorController();
    }

    [TestMethod]
    public void InvokeError_For404_ReturnsPageNotFound()
    {
        // Arrange
        int statusCode = (int)HttpStatusCode.NotFound;
        string expected = "PageNotFound";
        // Act
        var result = _errorController.Error(statusCode);

        // Assert
        result.Should().BeOfType<ViewResult>();
        result?.ViewName.Should().Be(expected);
    }

    [TestMethod]
    public void InvokeError_For500_ReturnsError()
    {
        // Arrange
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string expected = "Error";
        // Act
        var result = _errorController.Error(statusCode);

        // Assert
        result.Should().BeOfType<ViewResult>();
        result?.ViewName.Should().Be(expected);
    }
}