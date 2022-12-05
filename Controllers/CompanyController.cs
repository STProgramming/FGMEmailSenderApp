using EmailService;
using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.InputModels;
using FGMEmailSenderApp.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FGMEmailSenderApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILightCriptoHelper _dataHelper;
        private readonly IEmailSender _emailSender;

        public CompanyController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILightCriptoHelper dataHelper,
            IEmailSender emailSender
            ) 
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _dataHelper = dataHelper;
            _emailSender = emailSender;
        }

        #region CHECK PARTITA IVA

        [Authorize]
        [HttpGet]
        [Route("CheckPIva")]
        public async Task<IActionResult> CheckPIva(string iva)
        {
            if (iva == null || !iva.Any(i => char.IsLetterOrDigit(i)) || iva.Length != 11 ) return StatusCode(406, $"The information you inserted is not a complant P IVA. {DateTime.Now}");

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            //TODO chiamata a api di invio e verifica partita iva

            Company companyClaimed = _context.Companies.Any(c => string.Equals(c.CompanyIva, iva)) ? _context.Companies.Where(c => string.Equals(c.CompanyIva, iva)).FirstOrDefault() : new Company();

            if (companyClaimed == null) 
            {
                if (await _roleManager.FindByNameAsync(RoleHelper.AddCompanyPermissionRole) == null) await _roleManager.CreateAsync(new IdentityRole(RoleHelper.AddCompanyPermissionRole));

                await _userManager.AddToRoleAsync(user, RoleHelper.AddCompanyPermissionRole);

                _emailSender.SendNotificationUserChangeRole(user.NameUser, user.LastNameUser, user.Email, "createCompany", iva);

                return Ok(new { message = "The partita iva you typed is available", DateTime.Now }); 
            }

            if (companyClaimed.Users == null) return Ok(new { message = $"The P Iva is used by {_dataHelper.CriptName(companyClaimed.CompanyName)} company, but has no reference with an user.", DateTime.Now });

            if (!string.Equals(companyClaimed.Users.FirstOrDefault().Id, userId)) return Ok(new { message = "This partita iva belongs to a company.", DateTime.Now });

            return Ok(new { message = $"This {_dataHelper.CriptName(companyClaimed.CompanyName)} company is already got by you.", DateTime.Now });
        }

        #endregion

        #region INSERISCI DATI DENTRO LA COMPANY

        /// <summary>
        /// l'utente potrà aggiungere i dati della compangia solo se la partita iva non sarà inserita 
        /// ma l'agiunta di per se deve gestire un solo caso.
        /// Mentre l'update sarà congegnato in modo tale che l'utente manderà unarichiesta dimodifica informazioni ai nostri interni
        /// una volta autorizzato potrà modificare i dati
        /// </summary>
        /// <returns></returns>

        [Authorize(Policy = "AddCompanyReferentPermission")]
        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("AddCompanyData")]
        public async Task<IActionResult> AddCompanyData([FromBody] AddCompanyInputModel newCompany)
        {
            if (!ModelState.IsValid) return StatusCode(406, $"The data you submitted is not correct. {DateTime.Now}");

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            if (CheckIvaAvailability(newCompany.CompanyIva)) return BadRequest(new { message = "The partita iva you inserted belongs to another company.", DateTime.Now });

            if (CheckUniqueEmail(newCompany.CompanyEmail) && CheckUniqueTel(newCompany.CompanyTel)) return BadRequest(new { message = "Email or Phone belongs to anothers companies.", DateTime.Now });

            Company company = new Company
            {
                CompanyName = newCompany.CompanyName,
                CompanyEmail = newCompany.CompanyEmail,
                CompanyTel = newCompany.CompanyTel,
                CompanyIva = newCompany.CompanyIva,
                CompanyFax = newCompany.CompanyFax != null ? newCompany.CompanyFax : null
            };
            
            user.Company = company;

            await _context.SaveChangesAsync();

            return Ok(new { company, DateTime.Now });
        }

        #endregion

        #region CHECK P IVA AVAILABILITY

        private bool CheckIvaAvailability(string iva)
        {
            return _context.Companies.Any(c => string.Equals(c.CompanyIva, iva));
        }

        #endregion

        #region CHECK UNIQUE EMAIL COMPANY

        private bool CheckUniqueEmail(string emailCompany)
        {
            return _context.Companies.Any(c => string.Equals(c.CompanyEmail, emailCompany));
        }

        #endregion

        #region CHECK UNIQUE TEL COMPANY

        private bool CheckUniqueTel(string telCompany)
        {
            return _context.Companies.Any(c => string.Equals(c.CompanyTel, telCompany));
        }

        #endregion
    }
}
