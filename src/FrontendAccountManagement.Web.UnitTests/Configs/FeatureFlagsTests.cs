using FrontendAccountManagement.Web.Configs;

namespace FrontendAccountManagement.Web.UnitTests.Configs;

[TestClass]
public class FeatureFlagsTests
{
    [TestMethod]
    public void ManageCompanyDetailChanges_FeatureName_ShouldBeCorrect()
    {
        // Arrange
        const string expectedFeatureName = "ManageCompanyDetailChanges";

        // Act
        var actualFeatureName = FeatureFlags.ManageCompanyDetailChanges;

        // Assert
        actualFeatureName.Should().Be(expectedFeatureName);
    }
}