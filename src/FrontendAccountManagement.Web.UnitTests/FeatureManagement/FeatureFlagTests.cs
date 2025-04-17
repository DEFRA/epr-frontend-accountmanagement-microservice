using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

namespace FrontendAccountManagement.Web.UnitTests.FeatureManagement;

[TestClass]
public class FeatureFlagTests
{
    private IConfiguration configuration;

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ShowYourFeedbackFooter_Should_Be_True(bool value)
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            {FrontendAccountManagement.Web.Configs.FeatureFlags.ShowYourFeedbackFooter, value.ToString()}
        };

        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        bool showYourFeedbackFooter = bool.Parse(configuration[FrontendAccountManagement.Web.Configs.FeatureFlags.ShowYourFeedbackFooter]);

        // Assert
        showYourFeedbackFooter.Should().Be(value);
    }
}
