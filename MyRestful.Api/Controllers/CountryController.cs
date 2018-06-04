using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyRestful.Api.Resources;
using MyRestful.Core.DomainModels;
using MyRestful.Core.Interfaces;

namespace MyRestful.Api.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/countries")]
    public class CountryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;

        public CountryController(
            IUnitOfWork unitOfWork,
            ICountryRepository countryRepository, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _countryRepository = countryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var countries = await _countryRepository.GetCountriesAsync();
            var countryResources = _mapper.Map<List<CountryResource>>(countries);
            return Ok(countryResources);
        }

        [HttpGet("{id}", Name = "GetCountry")]
        public async Task<IActionResult> GetCountry(int id, bool includeCities = false)
        {
            var country = await _countryRepository.GetCountryByIdAsync(id, includeCities);
            if (country == null)
            {
                return NotFound();
            }
            var countryResource = _mapper.Map<CountryResource>(country);
            return Ok(countryResource);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCountry([FromBody] CountryAddResource country)
        {
            if (country == null)
            {
                return BadRequest();
            }

            var countryModel = _mapper.Map<Country>(country);
            _countryRepository.AddCountry(countryModel);
            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception("Error occurred when adding");
            }

            var countryResource = Mapper.Map<CountryResource>(countryModel);

            return CreatedAtRoute("GetCountry", new { id = countryModel.Id }, countryResource);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> BlockCreatingCountry(int id)
        {
            var country = await _countryRepository.GetCountryByIdAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            return StatusCode(StatusCodes.Status409Conflict);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countryRepository.GetCountryByIdAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            _countryRepository.DeleteCountry(country);

            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception($"Deleting country {id} failed when saving.");
            }

            return NoContent();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] CountryUpdateResource countryUpdate)
        {
            if (countryUpdate == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            var country = await _countryRepository.GetCountryByIdAsync(id, includeCities: true);
            if (country == null)
            {
                return NotFound();
            }
            _mapper.Map(countryUpdate, country);
            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception($"Updating country {id} failed when saving.");
            }
            return NoContent();
        }

    }
}
