namespace MyRestful.Core.DomainModels
{
    public class City
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Country Country { get; set; }
    }
}
