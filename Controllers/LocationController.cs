using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMEmailSenderApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAllCountries")]
        public async Task<ActionResult<ICollection<Country>>> GetAllCountries()
        {
            return await _context.Countries.ToListAsync();
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllCities")]
        public async Task<ActionResult<ICollection<City>>> GetAllCities()
        {
            return await _context.Cities.ToListAsync();
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllDepartments")]
        public async Task<ActionResult<ICollection<Department>>> GetAllDepartments()
        {
            return await _context.Departments.ToListAsync();
        }

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("AddCountry")]
        public async Task<IActionResult> AddCountry(string nameCountry)
        {
            if (string.IsNullOrEmpty(nameCountry)) return StatusCode(406, $"The data you inserted is null. {DateTime.Now}");

            if (NameCountryExist(nameCountry)) return BadRequest( new { message = "This country is already signed in", DateTime.Now});

            var newCountry = new Country { CountryName = nameCountry };

            await _context.Countries.AddAsync(newCountry);

            await _context.SaveChangesAsync();

            return Ok( new { newCountry, DateTime.Now });
        }

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("AddDepartment")]
        public async Task<IActionResult> AddDepartment([FromBody]string nameDepartment, string codeDepartment, string nameCountry)
        {
            if(string.IsNullOrEmpty(nameDepartment) || string.IsNullOrEmpty(codeDepartment)) return StatusCode(406, $"The data you inserted is null. {DateTime.Now}");

            if (DepartmentCheck(nameDepartment, codeDepartment)) return BadRequest(new { message = "This department is already signed in", DateTime.Now });

            var country = GetIdCountryFromName(nameCountry);

            if (country == null) return NotFound( new { message = "This country name does not belong to any signed country. Try first to add the country", DateTime.Now});

            var newDepartment = new Department
            {
                NameDepartment = nameDepartment,
                CodeDepartment = codeDepartment,
                FK_IdCountry = country.IdCountry,
                Country = country
            };

            await _context.Departments.AddAsync(newDepartment);

            country.Departments.Add(newDepartment);

            _context.Countries.Update(country);

            await _context.SaveChangesAsync();

            return Ok( new { newDepartment, DateTime.Now });
        }

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("AddCity")]
        public async Task<IActionResult> AddCity([FromBody]string nameCity, string capCity, string nameCountry)
        {
            if (string.IsNullOrEmpty(nameCity) || string.IsNullOrEmpty(capCity) || string.IsNullOrEmpty(nameCountry)) return StatusCode(406, $"The data you inserted is null. {DateTime.Now}");

            if (CityCheck(nameCity, capCity)) return BadRequest(new { message = "This department is already signed in", DateTime.Now });

            var country = GetIdCountryFromName(nameCountry);

            if (country == null) return NotFound(new { message = "This country name does not belong to any signed country. Try first to add the country", DateTime.Now });

            var newCity = new City
            {
                NameCity = nameCity,
                CapCity = capCity,
                FK_IdCountry = country.IdCountry,
                Country = country
            };

            await _context.Cities.AddAsync(newCity);

            country.Cities.Add(newCity);

            _context.Countries.Update(country);

            await _context.SaveChangesAsync();

            return Ok(new { newCity, DateTime.Now });
        }

        private Country? GetIdCountryFromName(string nameCountry)
        {
            return _context.Countries.Where(c => c.CountryName.ToLower() == nameCountry.ToLower()).FirstOrDefault();
        }

        private bool NameCountryExist(string nameCountry)
        {
            return _context.Countries.Any(c => String.Equals(c.CountryName.ToLower(), nameCountry.ToLower()));
        }

        private bool DepartmentCheck(string? nameDepartment, string? codeDepartment)
        {
            return _context.Departments.Any(d => d.CodeDepartment == codeDepartment || d.NameDepartment == nameDepartment);
        }

        private bool CityCheck(string nameCity, string capCity)
        {
            return _context.Cities.Any(c => String.Equals(c.NameCity, nameCity) || String.Equals(c.CapCity, capCity));
        }
    }
}
