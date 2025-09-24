using FluentAssertions;
using FrontendAccountManagement.Web.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace FrontendAccountManagement.Web.UnitTests.Extensions;

[TestClass]
public class ConfigurationExtensionsTests
{
    [TestMethod]
    public void IsFeatureEnabled_ReturnsTrue_WhenFeatureIsConfiguredTrue()
    {
        // Arrange
        var inMemory = new Dictionary<string, string?>
        {
            ["FeatureManagement:MyTestFeature"] = "true"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory)
            .Build();

        // Act
        var result = configuration.IsFeatureEnabled("MyTestFeature");

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsFeatureEnabled_ReturnsFalse_WhenFeatureIsConfiguredFalse()
    {
        // Arrange
        var inMemory = new Dictionary<string, string?>
        {
            ["FeatureManagement:MyTestFeature"] = "false"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory)
            .Build();

        // Act
        var result = configuration.IsFeatureEnabled("MyTestFeature");

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsFeatureEnabled_ReturnsFalse_WhenFeatureNotConfigured()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = configuration.IsFeatureEnabled("NonExistentFeature");

        // Assert
        result.Should().BeFalse();
    }
}