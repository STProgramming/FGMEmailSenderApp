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

        #region GET ALL COUNTRIES

        [HttpGet]
        [Route("GetAllCountries")]
        public async Task<ActionResult<ICollection<Country>>> GetAllCountries()
        {
            return await _context.Countries.ToListAsync();
        }

        #endregion

        #region GET ALL CITIES

        [Authorize]
        [HttpGet]
        [Route("GetAllCities")]
        public async Task<ActionResult<ICollection<City>>> GetAllCities()
        {
            return await _context.Cities.ToListAsync();
        }

        #endregion

        #region GET ALL DEPARTMENTS

        [Authorize]
        [HttpGet]
        [Route("GetAllDepartments")]
        public async Task<ActionResult<ICollection<Department>>> GetAllDepartments()
        {
            return await _context.Departments.ToListAsync();
        }

        #endregion

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
        public async Task<IActionResult> AddDepartment(string nameDepartment, string codeDepartment, string nameCountry)
        {
            if(string.IsNullOrEmpty(nameDepartment) || string.IsNullOrEmpty(codeDepartment) || string.IsNullOrEmpty(nameCountry)) return StatusCode(406, $"The data you inserted is null. {DateTime.Now}");

            if (DepartmentCheck(nameDepartment, codeDepartment)) return BadRequest(new { message = "This department is already signed in", DateTime.Now });

            var country = GetCountryFromName(nameCountry);

            if (country == null) return NotFound( new { message = "This country name does not belong to any signed country. Try first to add the country", DateTime.Now});

            var newDepartment = new Department
            {
                NameDepartment = nameDepartment,
                CodeDepartment = codeDepartment,
                FK_IdCountry = country.IdCountry,
                Country = country
            };

            await _context.Departments.AddAsync(newDepartment);

            await _context.SaveChangesAsync();

            return Ok( new { newDepartment, DateTime.Now });
        }

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("AddCity")]
        public async Task<IActionResult> AddCity(string nameCity, string capCity, string nameCountry, string nameDepartment)
        {
            if (string.IsNullOrEmpty(nameCity) || string.IsNullOrEmpty(capCity) || string.IsNullOrEmpty(nameCountry) || string.IsNullOrEmpty(nameDepartment)) return StatusCode(406, $"The data you inserted is null. {DateTime.Now}");

            if (CityCheck(nameCity, capCity)) return BadRequest(new { message = "This department is already signed in", DateTime.Now });

            var country = GetCountryFromName(nameCountry);

            var department = GetDepartmentFromName(nameDepartment);

            if (country == null) return NotFound(new { message = "This country name does not belong to any signed country. Try first to add the country", DateTime.Now });

            if (department == null) return NotFound(new { message = "This department name does not belong to any signed department. Try first to add the department", DateTime.Now });

            var newCity = new City
            {
                NameCity = nameCity,
                CapCity = capCity,
                FK_IdCountry = country.IdCountry,
                Country = country,
                FK_IdDepartment = department.IdDepartment,                
            };

            await _context.Cities.AddAsync(newCity);

            await _context.SaveChangesAsync();

            return Ok(new { newCity, DateTime.Now });
        }

        private Country? GetCountryFromName(string nameCountry)
        {
            return _context.Countries.Where(c => c.CountryName.ToLower() == nameCountry.ToLower()).FirstOrDefault();
        }

        private Department? GetDepartmentFromName(string nameDepartment)
        {
            return _context.Departments.Where(d => d.NameDepartment.ToLower() == nameDepartment.ToLower()).FirstOrDefault();
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
