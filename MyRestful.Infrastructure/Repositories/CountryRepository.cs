using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyRestful.Core.DomainModels;
using MyRestful.Core.Interfaces;

namespace MyRestful.Infrastructure.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly MyContext _myContext;

        public CountryRepository(MyContext myContext)
        {
            _myContext = myContext;
        }

        public async Task<IEnumerable<Country>> GetCountriesAsync()
        {
            return await _myContext.Countries.ToListAsync();
        }

        public async Task<Country> GetCountryByIdAsync(int id)
        {
            return await _myContext.Countries.FindAsync(id);
        }

        public void AddCountry(Country country)
        {
            _myContext.Countries.Add(country);
        }

        public async Task<bool> CountryExistAsync(int countryId)
        {
            return await _myContext.Countries.AnyAsync(x => x.Id == countryId);
        }
    }
}
