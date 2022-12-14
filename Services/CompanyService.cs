using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.Interfaces;

namespace FGMEmailSenderApp.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;

        public CompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region CHECK P IVA AVAILABILITY

        public bool CheckIvaAvailability(string iva)
        {
            return _context.Companies.Any(c => string.Equals(c.CompanyIva, iva));
        }

        #endregion

        #region CHECK UNIQUE EMAIL COMPANY

        public bool CheckUniqueEmail(string emailCompany)
        {
            return _context.Companies.Any(c => string.Equals(c.CompanyEmail, emailCompany));
        }

        #endregion

        #region CHECK UNIQUE TEL COMPANY

        public bool CheckUniqueTel(string telCompany)
        {
            return _context.Companies.Any(c => string.Equals(c.CompanyTel, telCompany));
        }

        #endregion

        #region

        //TODO CHIAMATA AD API ESTERNE PER VERIFICARE LA PARTITA IVA

        #endregion
    }
}
