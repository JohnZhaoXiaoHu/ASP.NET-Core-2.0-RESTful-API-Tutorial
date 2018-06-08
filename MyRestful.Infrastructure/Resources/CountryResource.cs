using System.Collections.Generic;
using MyRestful.Infrastructure.Resources.Hateoas;

namespace MyRestful.Infrastructure.Resources
{
    public class CountryResource: LinkResourceBase
    {
        public CountryResource()
        {
            Cities = new List<CityResource>();
        }

        public int Id { get; set; }
        public string EnglishName { get; set; }
        public string ChineseName { get; set; }
        public string Abbreviation { get; set; }

        public List<CityResource> Cities { get; set; }
    }
}
