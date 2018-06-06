using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyRestful.Core.DomainModels;
using MyRestful.Core.Interfaces;
using System.Linq.Dynamic.Core;

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
            var query = _myContext.Countries.AsQueryable();

            if (!string.IsNullOrEmpty(parameters.EnglishName))
            {
                var englishNameClause = parameters.EnglishName.Trim().ToLowerInvariant();
                query = query.Where(x => x.EnglishName.ToLowerInvariant() == englishNameClause);
            }
            if (!string.IsNullOrEmpty(parameters.ChineseName))
            {
                var chineseNameClause = parameters.ChineseName.Trim().ToLowerInvariant();
                query = query.Where(x => x.ChineseName.ToLowerInvariant() == chineseNameClause);
            }

            if (!string.IsNullOrEmpty(parameters.OrderBy))
            {
                var isDescending = parameters.OrderBy.EndsWith(" desc");
                var property = isDescending ? parameters.OrderBy.Split(" ")[0] : parameters.OrderBy;
                query = query.OrderBy(property + (isDescending ? " descending" : " ascending"));
            }

            var count = await query.CountAsync();
            var items = await query
                .Skip(parameters.PageSize * parameters.PageIndex)
                .Take(parameters.PageSize).ToListAsync();

            return new PaginatedList<Country>(parameters.PageIndex, parameters.PageSize, count, items);
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
