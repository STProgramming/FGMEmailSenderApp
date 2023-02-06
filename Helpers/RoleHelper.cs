using FGMEmailSenderApp.Models.EntityFrameworkModels;
using Microsoft.AspNetCore.Identity;

namespace FGMEmailSenderApp.Helpers
{
    public static class RoleHelper
    {
        public const string UserRole = "UserRole";

        public const string AddCompanyPermissionRole = "AddCompany";

        public const string EditCompanyPermissionRole = "EditCompany";

        public const string FGMEmployeeInternalRole = "FGMEmployee";

        public const string ReferentRole = "Referent";

        public const string UserOrReferentRole = "UserRole, Referent";
    }
}
