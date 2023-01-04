using FGMEmailSenderApp.Models.EntityFrameworkModels;

namespace FGMEmailSenderApp.Models.Interfaces
{
    public interface ICompanyService
    {
        bool CheckIvaAvailability(string iva);

        bool CheckUniqueEmail(string emailCompany);

        bool CheckUniqueTel(string telCompany);

        Company GetCompanyFromIva(string iva);

        int GetIdCompanyFromIva(string iva);

        string GetEmailCompanyFromIva(string iva);

        Company GetCompanyFromId(int IdCompany);
    }
}
