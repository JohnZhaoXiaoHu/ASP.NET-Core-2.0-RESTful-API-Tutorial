using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyRestful.Api.Resources;
using MyRestful.Core.DomainModels;
using MyRestful.Core.Interfaces;
using MyRestful.Infrastructure.Repositories;

namespace MyRestful.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/Values")]
    public class ValuesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICountryRepository _repository;
        private readonly IMapper _mapper;

        public ValuesController(IUnitOfWork unitOfWork, ICountryRepository repository, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var newCountry = new Country
            {
                ChineseName = "俄罗斯",
                EnglishName = "Russia",
                Abbreviation = "Russia"
            };
            _repository.AddCountry(newCountry);
            await _unitOfWork.SaveAsync();

            var countries = await _repository.GetCountriesAsync();
            var countryResources = _mapper.Map<List<CountryResource>>(countries);
            return Ok(countryResources);
        }

    }
}
