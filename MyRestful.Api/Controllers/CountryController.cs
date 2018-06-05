using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyRestful.Api.Resources;
using MyRestful.Core.DomainModels;
using MyRestful.Core.Interfaces;
using Newtonsoft.Json;

namespace MyRestful.Api.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/countries")]
    public class CountryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;

        public CountryController(
            IUnitOfWork unitOfWork,
            ICountryRepository countryRepository, IMapper mapper, IUrlHelper urlHelper)
        {
            _unitOfWork = unitOfWork;
            _countryRepository = countryRepository;
            _mapper = mapper;
            _urlHelper = urlHelper;
        }
        
        [HttpGet(Name = "GetCountries")]
        public async Task<IActionResult> Get(CountryResourceParameters countryResourceParameters)
        {
            var pagedList = await _countryRepository.GetCountriesAsync(countryResourceParameters);
            var countryResources = _mapper.Map<List<CountryResource>>(pagedList);

            var previousLink = pagedList.HasPrevious
                ? CreateCountryUri(countryResourceParameters, PaginationResourceUriType.PreviousPage) : null;
            var nextLink = pagedList.HasNext
                ? CreateCountryUri(countryResourceParameters, PaginationResourceUriType.NextPage) : null;

            var meta = new
            {
                pagedList.TotalItemsCount,
                pagedList.PaginationBase.PageSize,
                pagedList.PaginationBase.PageIndex,
                pagedList.PageCount,
                PreviousPageLink = previousLink,
                NextPageLink = nextLink
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta));
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

        private string CreateCountryUri(PaginationBase parameters, PaginationResourceUriType uriType)
        {
            switch (uriType)
            {
                case PaginationResourceUriType.PreviousPage:
                    var previousParameters = parameters.Clone();
                    previousParameters.PageIndex--;
                    return _urlHelper.Link("GetCountries", previousParameters);
                case PaginationResourceUriType.NextPage:
                    var nextParameters = parameters.Clone();
                    nextParameters.PageIndex++;
                    return _urlHelper.Link("GetCountries", nextParameters);
                case PaginationResourceUriType.CurrentPage:
                default:
                    return _urlHelper.Link("GetCountries", parameters);
            }
        }

    }
}
