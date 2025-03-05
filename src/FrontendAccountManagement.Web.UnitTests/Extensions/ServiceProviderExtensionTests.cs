namespace FrontendAccountManagement.Web.UnitTests.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;

using FrontendAccountManagement.Web.Extensions;
using Moq;
using FrontendAccountManagement.Web.Configs;
using Microsoft.AspNetCore.Http;

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

    [TestMethod]
    public void ConfigureSession_Should_Set_CookieSecurePolicy_To_Always()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "UseLocalSession", "true" }, 
            { "CookieOptions:SessionCookieName", "TestSession" },
            { "SessionIdleTimeOutMinutes", "30" }
        }).Build();

        services.Configure<EprCookieOptions>(configuration.GetSection("CookieOptions"));

        // Act
        services.RegisterWebComponents(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var sessionOptions = serviceProvider.GetRequiredService<IOptions<Microsoft.AspNetCore.Builder.SessionOptions>>().Value;

        // Assert
        Assert.AreEqual(CookieSecurePolicy.Always, sessionOptions.Cookie.SecurePolicy);
    }

}