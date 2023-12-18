namespace FrontendAccountManagement.Web.UnitTests.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;

using FrontendAccountManagement.Web.Extensions;
using Moq;

[TestClass]
public class ServiceProviderExtensionTests
{
    [TestMethod]
    public void ConfigureMsalDistributedTokenOptions_RegistersMsalDistributedTokenCacheAdapterOptions()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceCollection>();
        var mockConfig = new Mock<IConfiguration>();
        var descriptors = new List<ServiceDescriptor>();
        mockServiceProvider.Setup(m => m.Add(It.IsAny<ServiceDescriptor>()))
            .Callback((ServiceDescriptor a) =>
            {
                descriptors.Add(a);
            });

        // Act
        mockServiceProvider.Object.ConfigureMsalDistributedTokenOptions(mockConfig.Object);

        // Assert
        mockServiceProvider.Verify(m => m.Add(It.IsAny<ServiceDescriptor>()), Times.AtLeastOnce);
        Assert.IsTrue(descriptors.Any(d => d.ServiceType == typeof(IConfigureOptions<MsalDistributedTokenCacheAdapterOptions>)));
    }
}