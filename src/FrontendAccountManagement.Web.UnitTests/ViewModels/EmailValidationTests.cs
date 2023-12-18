using FrontendAccountManagement.Web.ViewModels.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.UnitTests.ViewModels;

[TestClass]
public class EmailValidationTests 
{
    private EmailValidationAttribute _emailValidationAttribute;

    [TestInitialize]
    public void Setup()
    {
        _emailValidationAttribute = new EmailValidationAttribute("This is a test");        
    }

    [TestMethod]
    public void InvokeIsValid_ForNullEmailAddress_ReturnsFalse()
    {
        // Arrange

        // Act
        var result = _emailValidationAttribute.IsValid(null);

        // Assert
        Assert.IsTrue(result);        
    }

    [TestMethod]
    public void InvokeIsValid_ForInvalidEmailAddress_ReturnsFalse()
    {
        // Arrange
        var email = "an.otherabc.com";

        // Act
        var result = _emailValidationAttribute.IsValid(email);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void InvokeIsValid_ForValidEmailAddress_ReturnsFalse()
    {
        // Arrange
        var email = "an.other@abc.com";

        // Act
        var result = _emailValidationAttribute.IsValid(email);

        // Assert
        Assert.IsTrue(result);
    }
}