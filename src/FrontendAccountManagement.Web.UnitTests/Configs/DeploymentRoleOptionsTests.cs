using FrontendAccountManagement.Web.Configs;

namespace FrontendAccountManagement.Web.UnitTests.Configs;

[TestClass]
public class DeploymentRoleOptionsTests
{
    [TestMethod]
    [DataRow(DeploymentRoleOptions.RegulatorRoleValue, true)]
    [DataRow("Producer", false)]
    public async Task IsRegulator_returns_correct_value(string deploymentRole, bool expectedResult)
    {
        // Arrange
        var option = new DeploymentRoleOptions { DeploymentRole = deploymentRole };

        // Act
        var result = option.IsRegulator();
        
        // Assert
        result.Should().Be(expectedResult);
    }

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