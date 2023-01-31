using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.Interfaces;

namespace FGMEmailSenderApp.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;
        public LocationService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public Department GetDepartmentFromCode(string codeDepartment)
        {
            return _context.Departments.Where(d => d.CodeDepartment == codeDepartment).FirstOrDefault();
        }

        public City GetCityFromCap(string capCity)
        {
            return _context.Cities.Where(c => c.CapCity == capCity).FirstOrDefault();
        }

        public Department GetDepartmentFromName(string department)
        {
            return _context.Departments.Where(d => d.NameDepartment.ToLower().Contains(department.ToLower()) || d.NameDepartment.ToLower() == department.ToLower()).FirstOrDefault();
        }
    }
}
