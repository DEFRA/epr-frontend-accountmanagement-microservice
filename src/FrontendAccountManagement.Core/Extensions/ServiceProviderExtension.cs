using FrontendAccountManagement.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceProviderExtension
    {
        public static IServiceCollection RegisterCoreComponents(this IServiceCollection services, IConfiguration configuration)
        {
            var useMockData = configuration.GetValue<bool>("FacadeAPI:UseMockData");
            if (useMockData)
            {
                services.AddSingleton<IFacadeService, MockedFacadeService>();
            }
            else
            {
                services.AddHttpClient<IFacadeService, FacadeService>(c => c.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("FacadeAPI:TimeoutSeconds")));
            }

            return services;
        }
    }
}