using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FrontendAccountManagement.IntegrationTests;

[TestClass]
public class HealthCheckTests : TestBase
{
    [TestMethod]
    public async Task HealthCheck_ReturnsOk()
    {
        // Arrange
        const string url = "/admin/health";

        // Act
        var response = await this._httpClient.GetAsync(url, CancellationToken.None);
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None);

        // Assert
        response.Should().BeSuccessful();
        content.Should().Be(HealthStatus.Healthy.ToString());
    }
}