using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyRestful.Core.DomainModels;
using MyRestful.Core.Interfaces;
using MyRestful.Infrastructure.Helpers;
using MyRestful.Infrastructure.Resources;
using MyRestful.Infrastructure.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyRestful.Api.Controllers
{
    [Route("api/countries")]
    public class CountryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingContainer _propertyMappingContainer;
        private readonly ITypeHelperService _typeHelperService;


        public CountryController(
            IUnitOfWork unitOfWork,
            ICountryRepository countryRepository,
            IMapper mapper,
            IUrlHelper urlHelper,
            IPropertyMappingContainer propertyMappingContainer,
            ITypeHelperService typeHelperService)
        {
            _unitOfWork = unitOfWork;
            _countryRepository = countryRepository;
            _mapper = mapper;
            _urlHelper = urlHelper;
            _propertyMappingContainer = propertyMappingContainer;
            _typeHelperService = typeHelperService;
        }

        [HttpGet(Name = "GetCountries")]
        public async Task<IActionResult> Get(CountryResourceParameters countryResourceParameters)
        {
            if (!_propertyMappingContainer.ValidMappingExistsFor<CountryResource, Country>(countryResourceParameters.OrderBy))
            {
                return BadRequest("Can't find the fields for sorting.");
            }

            if (!_typeHelperService.TypeHasProperties<CountryResource>(countryResourceParameters.Fields))
            {
                return BadRequest("Can't find the fields on Resource.");
            }

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

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            return Ok(countryResources.ToDynamicIEnumerable(countryResourceParameters.Fields));
        }

        [HttpGet("{id}", Name = "GetCountry")]
        public async Task<IActionResult> GetCountry(int id, string fields = null, bool includeCities = false)
        {
            if (!_typeHelperService.TypeHasProperties<CountryResource>(fields))
            {
                return BadRequest("Can't find the fields on Resource.");
            }
            var country = await _countryRepository.GetCountryByIdAsync(id, includeCities);
            if (country == null)
            {
                return NotFound();
            }
            var countryResource = _mapper.Map<CountryResource>(country);
            return Ok(countryResource.ToDynamic(fields));
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

        private string CreateCountryUri(CountryResourceParameters parameters, PaginationResourceUriType uriType)
        {
            switch (uriType)
            {
                case PaginationResourceUriType.PreviousPage:
                    var previousParameters = new
                    {
                        pageIndex = parameters.PageIndex - 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields,
                        chineseName = parameters.ChineseName,
                        englishName = parameters.EnglishName
                    };
                    return _urlHelper.Link("GetCountries", previousParameters);
                case PaginationResourceUriType.NextPage:
                    var nextParameters = new
                    {
                        pageIndex = parameters.PageIndex + 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields,
                        chineseName = parameters.ChineseName,
                        englishName = parameters.EnglishName
                    };
                    return _urlHelper.Link("GetCountries", nextParameters);
                case PaginationResourceUriType.CurrentPage:
                default:
                    return _urlHelper.Link("GetCountries", parameters);
            }
        }

    }
}
