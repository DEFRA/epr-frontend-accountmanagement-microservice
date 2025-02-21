using System;
using FrontendAccountManagement.Web.Constants;

namespace FrontendAccountManagement.Web.UnitTests.Constants;

[TestClass]
public class ConstantTests
{
    [TestMethod]
    public void ManageCompanyDetailChanges_FeatureName_ShouldBeCorrect()
    {
        // Arrange
        const string expectedFeatureName = "ManageCompanyDetailChanges";

        // Act
        var actualFeatureName = FeatureName.ManageCompanyDetailChanges;

        // Assert
        actualFeatureName.Should().Be(expectedFeatureName);
    }
}
