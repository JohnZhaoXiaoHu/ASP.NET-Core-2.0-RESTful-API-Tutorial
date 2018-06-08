using Microsoft.Extensions.DependencyInjection;
using MyRestful.Infrastructure.Services;

namespace MyRestful.Infrastructure.Extensions
{
    public static class PropertyMappingExtensions
    {
        public static void AddPropertyMappings(this IServiceCollection services)
        {
            var propertyMappingContainer = new PropertyMappingContainer();
            propertyMappingContainer.Register<CountryPropertyMapping>();

            services.AddSingleton<IPropertyMappingContainer>(propertyMappingContainer);
        }
    }
}
