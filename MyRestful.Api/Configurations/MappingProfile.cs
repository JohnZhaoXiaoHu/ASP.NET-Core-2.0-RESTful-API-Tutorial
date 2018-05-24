using AutoMapper;
using MyRestful.Api.Resources;
using MyRestful.Core.DomainModels;

namespace MyRestful.Api.Configurations
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Country, CountryResource>();
            CreateMap<CountryResource, Country>();
            CreateMap<CountryAddResource, Country>();

            CreateMap<City, CityResource>();
            CreateMap<CityResource, City>();
            CreateMap<CityAddResource, City>();
            CreateMap<CityUpdateResource, City>();
        }
    }
}
