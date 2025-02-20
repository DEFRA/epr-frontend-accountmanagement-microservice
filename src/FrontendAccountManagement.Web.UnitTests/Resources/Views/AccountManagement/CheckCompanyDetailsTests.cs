using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.UnitTests.Resources.Views.AccountManagement;

[TestClass]
public class CheckCompanyDetailsTests
{
    private const string ResourceLocation = "FrontendAccountManagement.Web.Resources.Views.AccountManagement.CheckCompanyDetails";
    private ResourceManager _resourceManager;

    [TestInitialize]
    public void Setup()
    {
        _resourceManager = new ResourceManager(ResourceLocation, typeof(CheckCompanyDetailsViewModel).Assembly);
    }

    /// <summary>
    /// Test if a resource key exists in the default resource file.
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    [DataTestMethod]
    [DataRow("CheckYourDetails")]
    [DataRow("Name")]
    [DataRow("Address")]
    [DataRow("UKNation")]
    public void GivenResourceKeyExists_ResourceShouldReturnValue(string resourceKey)
    {
        // Arrange

        // Act
        var value = _resourceManager.GetString(resourceKey, CultureInfo.InvariantCulture);

        // Assert
        value.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Test non existing key should return null
    /// </summary>
    [TestMethod]
    public void GivenResourceKeyIsMissing_ResourceShouldReturnNull()
    {
        // Arrange
        var nonExistentKey = "NonExistingKey";

        // Act
        var value = _resourceManager.GetString(nonExistentKey, CultureInfo.CurrentUICulture);

        // Assert
        value.Should().BeNull();
    }

    /// <summary>
    /// Test translations for multiple cultures
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    /// <param name="culture">The culture</param>
    /// <param name="expectedValue">Expected text</param>
    [TestMethod]
    [DataRow("CheckYourDetails", "en", "Check your details")]
    [DataRow("CheckYourDetails", "cy", "Gwiriwch eich manylion")]
    [DataRow("Name", "en", "Name")]
    [DataRow("Name", "cy", "Enw")]
    [DataRow("Address", "en", "Address")]
    [DataRow("Address", "cy", "Cyfeiriad")]
    [DataRow("UKNation", "en", "UK Nation")]
    [DataRow("UKNation", "cy", "Gwlad yn y Deyrnas Unedig")]
    public void GivenTranslationExists_ResourceShouldTranslateCorrectly(string resourceKey, string culture, string expectedValue)
    {
        // Arrange
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

        // Act
        var value = _resourceManager.GetString(resourceKey, CultureInfo.CurrentUICulture);

        // Assert
        value.Should().Be(expectedValue);
    }

    /// <summary>
    /// Test that default culture is used when translation is missing
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    /// <param name="cultureNotAvailable">The culture which is not available</param>
    /// <param name="expectedValue">Default value if specified culture not available</param>
    [TestMethod]
    [DataRow("CheckYourDetails", "es", "Check your details")]
    [DataRow("Name", "es", "Name")]
    [DataRow("Address", "es", "Address")]
    [DataRow("UKNation", "es", "UK Nation")]
    public void GivenTranslationDoesNotExist_ResourceShouldFallbackToDefaultLanguage(string resourceKey, string cultureNotAvailable, string expectedValue)
    {
        // Arrange
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureNotAvailable);

        // Act
        var value = _resourceManager.GetString(resourceKey, CultureInfo.CurrentUICulture);

        // Assert
        value.Should().Be(expectedValue);
    }
}