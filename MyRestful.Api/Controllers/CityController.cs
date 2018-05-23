using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyRestful.Api.Resources;
using MyRestful.Core.DomainModels;
using MyRestful.Core.Interfaces;

namespace MyRestful.Api.Controllers
{
    [Route("api/countries/{countryId}/cities")]
    public class CityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;

        public CityController(IUnitOfWork unitOfWork, 
            ICountryRepository countryRepository, 
            ICityRepository cityRepository, 
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCitiesForCountry(int countryId)
        {
            if (!await _countryRepository.CountryExistAsync(countryId))
            {
                return NotFound();
            }

            var citiesForCountry = await _cityRepository.GetCitiesForCountryAsync(countryId);
            var citiesResources = _mapper.Map<IEnumerable<CityResource>>(citiesForCountry);
            return Ok(citiesResources);
        }

        [HttpGet("{cityId}", Name = "GetCity")]
        public async Task<IActionResult> GetCityForCountry(int countryId, int cityId)
        {
            if (!await _countryRepository.CountryExistAsync(countryId))
            {
                return NotFound();
            }

            var cityForCountry = await _cityRepository.GetCityForCountryAsync(countryId, cityId);
            if (cityForCountry == null)
            {
                return NotFound();
            }
            var cityResource = _mapper.Map<CityResource>(cityForCountry);
            return Ok(cityResource);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCityForCountry(int countryId, [FromBody] CityAddResource city)
        {
            if (city == null)
            {
                return BadRequest();
            }

            if (!await _countryRepository.CountryExistAsync(countryId))
            {
                return NotFound();
            }

            var cityModel = _mapper.Map<City>(city);
            _cityRepository.AddCity(countryId, cityModel);
            if (!await _unitOfWork.SaveAsync())
            {
                return StatusCode(500, "Error occurred when adding");
            }

            var cityResource = Mapper.Map<CityResource>(cityModel);

            return CreatedAtRoute("GetCity", new { countryId, cityId = cityModel.Id }, cityResource);
        }
    }
}
