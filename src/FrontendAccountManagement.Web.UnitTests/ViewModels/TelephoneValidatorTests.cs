using System.ComponentModel.DataAnnotations;
using FrontendAccountManagement.Web.ViewModels.Attributes;

namespace FrontendAccountManagement.Web.UnitTests.ViewModels;

/// <summary>
/// Tests the functionality of the <see cref="TelephoneNumberValidationAttribute"/> class.
/// </summary>
[TestClass]
public class TelephoneValidatorTests
{
    /// <summary>
    /// Check that the IsValid method doesn't return an error when valid phone numbers are validated.
    /// </summary>
    [TestMethod]
    [DataRow("07489621402")]
    [DataRow("020 1212 1212")]
    [DataRow("078 1212 1212")]
    [DataRow("78 1212 1212")]
    [DataRow("+44 078 1212 1212")]
    [DataRow("+44 78 1212 1212")]
    [DataRow("(+44) 78 1212 1212")]
    [DataRow("0044 078 1212 1212")]
    [DataRow("02012121212")]
    [DataRow("07812121212")]
    [DataRow("7812121212")]
    [DataRow("+4407812121212")]
    [DataRow("+447812121212")]
    [DataRow("004407812121212")]
    [DataRow("+49 30 901820")]
    [DataRow("+34919931307")]
    public void IsValid_Success(string phoneNumber)
    {
        // Arrange
        var toTest = new TelephoneNumberValidationAttribute();

        // Act
        var result = toTest.GetValidationResult(phoneNumber, new ValidationContext(phoneNumber));

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Checks that the IsValid method returns the expected error when an invalid phone number is validated.
    /// </summary>
    /// <param name="phoneNumber">The phone number to test.</param>
    /// <param name="expectedIsValid">Whether the phone number being tested is expected to be valid or not.</param>
    [TestMethod]
    [DataRow("05815985")]
    [DataRow("020 1212 121")]
    [DataRow("020 1212 121")]
    [DataRow("078 1212 121A")]
    [DataRow("a")]
    [DataRow("800 890 567sad")]
    [DataRow("800 890 567123")]
    [DataRow("asd800 890 567")]
    [DataRow("123800 890 567")]
    [DataRow("07812121212!!")]
    [DataRow("..07812121212")]
    [DataRow("!@£$%800 890 567")]
    [DataRow("072121212^&*()_+")]
    [DataRow("0721^&*()_+2121")]
    [DataRow("078 1(212 12)1A")]
    [DataRow("0800 not gud")]
    [DataRow("01234567890123456789012345678901234567890123456789")]
    public void IsValid_FailOnInvalidNumber(string phoneNumber)
    {
        // Arrange
        var toTest = new TelephoneNumberValidationAttribute();

        // Act
        var result = toTest.GetValidationResult(phoneNumber, new ValidationContext(phoneNumber));

        // Assert
        Assert.AreEqual("Enter a valid telephone number", result.ErrorMessage);
    }

    /// <summary>
    /// Check that the IsValid method returns the correct error message when an empty phone number is validated.
    /// </summary>
    [TestMethod]
    public void IsValid_FailOnEmptyNumber()
    {
        // Arrange
        var toTest = new TelephoneNumberValidationAttribute();
        string phoneNumber = string.Empty;

        // Act
        var result = toTest.GetValidationResult(phoneNumber, new ValidationContext(phoneNumber));

        // Assert
        Assert.AreEqual("Enter your telephone number", result.ErrorMessage);
    }

    /// <summary>
    /// Check that the IsValid method returns the correct error message when a phone number longer than 50 characters is validated.
    /// </summary>
    [TestMethod]
    public void IsValid_FailOnNumberTooLong()
    {
        // Arrange
        var toTest = new TelephoneNumberValidationAttribute();
        string phoneNumber = "012345678901234567890123456789012345678901234567890";

        // Act
        var result = toTest.GetValidationResult(phoneNumber, new ValidationContext(phoneNumber));

        // Assert
        Assert.AreEqual("Telephone number must be 50 characters or less", result.ErrorMessage);
    }
}