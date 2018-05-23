using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MyRestful.Core.DomainModels;

namespace MyRestful.Core.Interfaces
{
    public interface ICityRepository
    {
        Task<List<City>> GetCitiesForCountryAsync(int countryId);
        Task<City> GetCityForCountryAsync(int countryId, int cityId);
        void AddCity(int countryId, City city);
    }
}
