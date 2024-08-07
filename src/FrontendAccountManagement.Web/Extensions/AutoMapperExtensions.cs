using AutoMapper;
using FrontendAccountManagement.Web.Profiles;

namespace FrontendAccountManagement.Web.Extensions
{
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
                mc.AllowNullCollections = true;
            });

            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}