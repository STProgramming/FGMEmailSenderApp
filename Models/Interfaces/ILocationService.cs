using FGMEmailSenderApp.Models.EntityFrameworkModels;

namespace FGMEmailSenderApp.Models.Interfaces
{
    public interface ILocationService
    {
        Department GetDepartmentFromCode(string codeDepartment);

        City GetCityFromCap(string capCity);

        Department GetDepartmentFromName(string department);
    }
}
