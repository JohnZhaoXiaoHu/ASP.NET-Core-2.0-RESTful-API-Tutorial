using System.Collections.Generic;
using System.Linq;
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

        public async Task<PaginatedList<Country>> GetCountriesAsync(CountryResourceParameters parameters)
        {
            var query = _myContext.Countries
                .OrderBy(x => x.Id)
                .Skip(parameters.PageSize * parameters.PageIndex)
                .Take(parameters.PageSize);
            var count = await query.CountAsync();
            var items = await query.ToListAsync();

            return new PaginatedList<Country>(new PaginationBase
            {
                Count = count,
                PageIndex = parameters.PageIndex,
                PageSize = parameters.PageSize
            }, items);
        }

        public async Task<IEnumerable<Country>> GetCountriesAsync(IEnumerable<int> ids)
        {
            return await _myContext.Countries.Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<Country> GetCountryByIdAsync(int id, bool includeCities = false)
        {
            if (!includeCities)
            {
                return await _myContext.Countries.FindAsync(id);
            }
            return await _myContext.Countries.Include(x => x.Cities).SingleOrDefaultAsync(x => x.Id == id);
        }

        public void AddCountry(Country country)
        {
            _myContext.Countries.Add(country);
        }

        public void UpdateCountry(Country country)
        {
            _myContext.Countries.Update(country);
        }

        public async Task<bool> CountryExistAsync(int countryId)
        {
            return await _myContext.Countries.AnyAsync(x => x.Id == countryId);
        }

        public void DeleteCountry(Country country)
        {
            _myContext.Countries.Remove(country);
        }

    }
}
