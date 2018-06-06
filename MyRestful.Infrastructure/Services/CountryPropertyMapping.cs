using System.Collections.Generic;
using MyRestful.Core.DomainModels;
using MyRestful.Infrastructure.Resources;

namespace MyRestful.Infrastructure.Services
{
    public class CountryPropertyMapping : PropertyMapping<CountryResource, Country>
    {
        public CountryPropertyMapping() : base(new Dictionary<string, List<MappedProperty>>
        {
            [nameof(CountryResource.EnglishName)] = new List<MappedProperty>
            {
                new MappedProperty{ Name = nameof(Country.EnglishName), Revert = false}
            },
            [nameof(CountryResource.ChineseName)] = new List<MappedProperty>
            {
                new MappedProperty{ Name = nameof(Country.ChineseName), Revert = false}
            },
            [nameof(CountryResource.Abbreviation)] = new List<MappedProperty>
            {
                new MappedProperty{ Name = nameof(Country.Abbreviation), Revert = false}
            }
        })
        {
        }
    }
}
