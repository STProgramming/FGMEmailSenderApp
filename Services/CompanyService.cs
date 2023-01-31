using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        #region GET COMPANY FROM IVA

        public Company GetCompanyFromIva(string iva) 
        {
            return _context.Companies.Where(c => c.CompanyIva == iva).FirstOrDefault();
        }

        #endregion

        #region GET ID COMPANY FROM IVA

        public int GetIdCompanyFromIva(string iva)
        {
            return _context.Companies.Where(c => c.CompanyIva == iva).FirstOrDefault().IdCompany;
        }

        #endregion

        #region GET EMAIL COMPANY FROM IVA

        public string GetEmailCompanyFromIva(string iva)
        {
            return _context.Companies.Where(c => c.CompanyIva == iva).FirstOrDefault().CompanyEmail;
        }

        #endregion

        #region GET COMPANY FROM ID

        public Company GetCompanyFromId(int IdCompany)
        {
            return _context.Companies.Where(c => c.IdCompany == IdCompany).FirstOrDefault();
        }

        #endregion

        #region GET COMPANY FROM EMAIL

        public Company GetCompanyFromEmail(string emailCompany)
        {
            return _context.Companies.Where(c => c.CompanyEmail == emailCompany).FirstOrDefault();
        }

        #endregion
    }
}
