using AutoMapper;
using FrontendAccountManagement.Web.Profiles;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Extension class for any further AutoMapper functionality
    /// </summary>
    public static class AutoMapperExtensions
    {
        /// <summary>
        /// Configures the AutoMapper with the profiles that exist for mapping purposes
        /// </summary>
        /// <param name="services">The IServiceCollection</param>
        public static void ConfigureAutoMapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AccountManagementProfile());
                mc.AddProfile(new CompaniesHouseResponseProfile());
                mc.AllowNullCollections = true;
            });

            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}