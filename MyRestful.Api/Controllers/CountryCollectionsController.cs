using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyRestful.Api.Resources;
using MyRestful.Core.DomainModels;
using MyRestful.Core.Interfaces;

namespace MyRestful.Api.Controllers
{
    [Route("api/countrycollections")]
    public class CountryCollectionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;

        public CountryCollectionsController(
            IUnitOfWork unitOfWork, 
            ICountryRepository countryRepository, 
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _countryRepository = countryRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCountryCollection(
            [FromBody] IEnumerable<CountryAddResource> countries)
        {
            if (countries == null)
            {
                return BadRequest();
            }

            var countriesModel = _mapper.Map<IEnumerable<Country>>(countries);
            foreach (var country in countriesModel)
            {
                _countryRepository.AddCountry(country);
            }

            if (!await _unitOfWork.SaveAsync())
            {
                return StatusCode(500, "Error occurred when adding");
            }

            return Ok();
        }
    }
}
