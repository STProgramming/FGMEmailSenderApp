namespace FGMEmailSenderApp.Models.Interfaces
{
    public interface ICompanyService
    {
        bool CheckIvaAvailability(string iva);

        bool CheckUniqueEmail(string emailCompany);

        bool CheckUniqueTel(string telCompany);
    }
}
