namespace Restful.Core.DomainModels
{
    public class CountryResourceParameters: PaginationBase
    {
        public string EnglishName { get; set; }
        public string ChineseName { get; set; }
    }
}
