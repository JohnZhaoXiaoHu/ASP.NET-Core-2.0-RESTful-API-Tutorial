using Microsoft.Extensions.DependencyInjection;
using MyRestful.Infrastructure.Services;

namespace MyRestful.Api.Configurations
{
    public static class PropertyMappingExtension
    {
        public static void AddPropertyMappings(this IServiceCollection services)
        {
            var propertyMappingContainer = new PropertyMappingContainer();
            propertyMappingContainer.Register<CountryPropertyMapping>();

            services.AddSingleton<IPropertyMappingContainer>(propertyMappingContainer);
        }
    }
}
