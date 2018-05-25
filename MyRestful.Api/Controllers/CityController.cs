using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
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
            _cityRepository.AddCityForCountry(countryId, cityModel);
            if (!await _unitOfWork.SaveAsync())
            {
                return StatusCode(500, "Error occurred when adding");
            }

            var cityResource = Mapper.Map<CityResource>(cityModel);

            return CreatedAtRoute("GetCity", new { countryId, cityId = cityModel.Id }, cityResource);
        }

        [HttpDelete("{cityId}")]
        public async Task<IActionResult> DeleteCityForCountry(int countryId, int cityId)
        {
            if (!await _countryRepository.CountryExistAsync(countryId))
            {
                return NotFound();
            }

            var city = await _cityRepository.GetCityForCountryAsync(countryId, cityId);
            if (city == null)
            {
                return NotFound();
            }

            _cityRepository.DeleteCity(city);

            if (!await _unitOfWork.SaveAsync())
            {
                return StatusCode(500, $"Deleting city {cityId} for country {countryId} failed when saving.");
            }

            return NoContent();
        }

        [HttpPut("{cityId}")]
        public async Task<IActionResult> UpdateCityForCountry(int countryId, int cityId, 
            [FromBody] CityUpdateResource cityUpdate)
        {
            if (cityUpdate == null)
            {
                return BadRequest();
            }

            if (!await _countryRepository.CountryExistAsync(countryId))
            {
                return NotFound();
            }

            var city = await _cityRepository.GetCityForCountryAsync(countryId, cityId);
            if (city == null)
            {
                //var cityToAdd = _mapper.Map<City>(cityUpdate);
                //cityToAdd.Id = cityId; // 如果Id不是自增的话
                //_cityRepository.AddCityForCountry(countryId, cityToAdd);

                //if (!await _unitOfWork.SaveAsync())
                //{
                //    return StatusCode(500, $"Upserting city {cityId} for country {countryId} failed when inserting");
                //}

                //var cityResource = Mapper.Map<CityResource>(cityToAdd);
                //return CreatedAtRoute("GetCity", new { countryId, cityId }, cityResource);
                return NotFound();
            }

            // 把cityUpdate的属性值都映射给city
            _mapper.Map(cityUpdate, city);

            _cityRepository.UpdateCityForCountry(city);

            if (!await _unitOfWork.SaveAsync())
            {
                return StatusCode(500, $"Updating city {cityId} for country {countryId} failed when saving.");
            }

            return NoContent(); 
        }

        [HttpPatch("{cityId}")]
        public async Task<IActionResult> PartiallyUpdateCityForCountry(int countryId, int cityId, 
            [FromBody] JsonPatchDocument<CityUpdateResource> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!await _countryRepository.CountryExistAsync(countryId))
            {
                return NotFound();
            }

            var city = await _cityRepository.GetCityForCountryAsync(countryId, cityId);
            if (city == null)
            {
                //var cityUpdate = new CityUpdateResource();
                //patchDoc.ApplyTo(cityUpdate);
                //var cityToAdd = _mapper.Map<City>(cityUpdate);
                //cityToAdd.Id = cityId; // 只适用于Id不是自增的情况

                //_cityRepository.AddCityForCountry(countryId, cityToAdd);

                //if (!await _unitOfWork.SaveAsync())
                //{
                //    return StatusCode(500, $"P city {cityId} for country {countryId} failed when inserting");
                //}
                //var cityResource = Mapper.Map<CityResource>(cityToAdd);
                //return CreatedAtRoute("GetCity", new { countryId, cityId }, cityResource);

                return NotFound();
            }

            var cityToPatch = _mapper.Map<CityUpdateResource>(city);

            patchDoc.ApplyTo(cityToPatch);

            // 需要验证

            _mapper.Map(cityToPatch, city);
            _cityRepository.UpdateCityForCountry(city);

            if (!await _unitOfWork.SaveAsync())
            {
                return StatusCode(500, $"Patching city {cityId} for country {countryId} failed when saving.");
            }

            return NoContent();
        }
    }
}
