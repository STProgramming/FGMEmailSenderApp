using FGMEmailSenderApp.Models.EntityFrameworkModels;
using Microsoft.AspNetCore.Identity;

namespace FGMEmailSenderApp.Helpers
{
    public static class RoleHelper
    {
        public const string UserRole = "UserRole";

        public const string AddCompanyPermissionRole = "AddCompany";

        public const string FGMEmployeeInternalRole = "FGMEmployee";
    }
}
