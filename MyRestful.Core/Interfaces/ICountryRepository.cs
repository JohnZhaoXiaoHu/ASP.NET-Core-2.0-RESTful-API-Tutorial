using System.Collections.Generic;
using System.Threading.Tasks;
using MyRestful.Core.DomainModels;

namespace MyRestful.Core.Interfaces
{
    public interface ICountryRepository
    {
        Task<IEnumerable<Country>> GetCountriesAsync();
        void AddCountry(Country country);
        Task<Country> GetCountryByIdAsync(int id);
        Task<bool> CountryExistAsync(int countryId);
        Task<IEnumerable<Country>> GetCountriesAsync(IEnumerable<int> ids);
        void DeleteCountry(Country country);
    }
}