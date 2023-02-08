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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;

namespace FGMEmailSenderApp.Controllers
{
    [Route("api/Identity/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILightCriptoHelper _dataHelper;
        private readonly IEmailSender _emailSender;
        private readonly ICompanyService _companyService;

        public CompanyController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILightCriptoHelper dataHelper,
            IEmailSender emailSender,
            ICompanyService companyService
            ) 
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _dataHelper = dataHelper;
            _emailSender = emailSender;
            _companyService = companyService;
        }

        #region CHECK PARTITA IVA

        [Authorize]
        [HttpGet]
        [Route("CheckPIva")]
        public async Task<IActionResult> CheckPIva(string iva)
        {
            if (!CheckIvaCompliant(iva)) return StatusCode(406, $"The information you inserted is not a complant P IVA. {DateTime.Now}");

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            //TODO chiamata a api di invio e verifica partita iva

            Company companyClaimed = _context.Companies.Any(c => string.Equals(c.CompanyIva, iva)) ? _context.Companies.Where(c => string.Equals(c.CompanyIva, iva)).FirstOrDefault() : new Company();

            if (companyClaimed == null) 
            {
                if (await _roleManager.FindByNameAsync(RoleHelper.AddCompanyPermissionRole) == null) await _roleManager.CreateAsync(new IdentityRole(RoleHelper.AddCompanyPermissionRole));

                await _userManager.AddToRoleAsync(user, RoleHelper.AddCompanyPermissionRole);

                _emailSender.SendNotificationUserPermission(user.NameUser, user.LastNameUser, user.Email, "createCompany", iva);

                return Ok(new { message = "The partita iva you typed is available", DateTime.Now }); 
            }

            if (companyClaimed.User == null) return Ok(new { message = $"The P Iva is used by {_dataHelper.CriptName(companyClaimed.CompanyName)} company, but has no reference with an user.", DateTime.Now });

            if (!string.Equals(companyClaimed.User.Id, userId)) return Ok(new { message = "This partita iva belongs to a company.", DateTime.Now });

            return Ok(new { message = $"This {_dataHelper.CriptName(companyClaimed.CompanyName)} company is already got by you.", DateTime.Now });
        }

        #endregion

        #region GET ALL COMPANIES

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpGet]
        [Route("GetAllCompanies")]

        public async Task<ActionResult<ICollection<Company>>> GetAllCompanies()
        {
            return await _context.Companies.ToListAsync();
        }

        #endregion

        #region AGGIUNGI DATI COMPANY

        /// <summary>
        /// l'utente potrà aggiungere i dati della compangia solo se la partita iva non sarà inserita 
        /// ma l'agiunta di per se deve gestire un solo caso.
        /// Mentre l'update sarà congegnato in modo tale che l'utente manderà unarichiesta dimodifica informazioni ai nostri interni
        /// una volta autorizzato potrà modificare i dati
        /// </summary>
        /// <returns></returns>

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("AddCompanyData")]
        public async Task<IActionResult> AddCompanyData(AddCompanyInputModel newCompany)
        {
            if (!ModelState.IsValid) return StatusCode(406, $"The data you submitted is not correct. {DateTime.Now}");

            //TODO chiamata a api di invio e verifica partita iva

            if (_companyService.CheckIvaAvailability(newCompany.CompanyIva)) return BadRequest(new { message = "The partita iva you inserted belongs to another company.", DateTime.Now });

            if (_companyService.CheckUniqueEmail(newCompany.CompanyEmail) && _companyService.CheckUniqueTel(newCompany.CompanyTel)) return BadRequest(new { message = "Email or Phone belongs to anothers companies.", DateTime.Now });

            Company company = new Company
            {
                CompanyName = newCompany.CompanyName,
                CompanyEmail = newCompany.CompanyEmail,
                CompanyTel = newCompany.CompanyTel,
                CompanyIva = newCompany.CompanyIva,
                CompanyFax = newCompany.CompanyFax != null ? newCompany.CompanyFax : null
            };
            
            await _context.Companies.AddAsync(company);

            await _context.SaveChangesAsync();

            return Ok(new { company, DateTime.Now });
        }

        [Authorize(Policy = "AddDataCompanyPermission")]
        [HttpPost]
        [Route("AddCompanyDataByUser")]
        public async Task<IActionResult> AddCompanyDataByUser(AddCompanyInputModel newCompany)
        {
            if (!ModelState.IsValid) return StatusCode(406, $"The data you submitted is not correct. {DateTime.Now}");

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            if (user.Company != null || user.IdCompany != null) return BadRequest(new { message = "You can not add data from a different company", DateTime.Now });

            //TODO chiamata a api di invio e verifica partita iva

            if (_companyService.CheckIvaAvailability(newCompany.CompanyIva)) return BadRequest(new { message = "The partita iva you inserted belongs to another company.", DateTime.Now });

            if (_companyService.CheckUniqueEmail(newCompany.CompanyEmail) && _companyService.CheckUniqueTel(newCompany.CompanyTel)) return BadRequest(new { message = "Email or Phone belongs to anothers companies.", DateTime.Now });

            Company company = new Company
            {
                CompanyName = newCompany.CompanyName,
                CompanyEmail = newCompany.CompanyEmail,
                CompanyTel = newCompany.CompanyTel,
                CompanyIva = newCompany.CompanyIva,
                CompanyFax = newCompany.CompanyFax != null ? newCompany.CompanyFax : null
            };

            user.Company = company;

            await _userManager.RemoveFromRoleAsync(user, RoleHelper.AddCompanyPermissionRole);

            if (!await _roleManager.RoleExistsAsync(RoleHelper.ReferentRole)) await _roleManager.CreateAsync(new IdentityRole { Name = RoleHelper.ReferentRole });

            await _userManager.AddToRoleAsync(user, RoleHelper.ReferentRole);

            _emailSender.SendNotificationUserChangeRole(user.NameUser, user.LastNameUser, user.Email, RoleHelper.ReferentRole);

            await _context.Companies.AddAsync(company);

            user.Company = company;

            await _userManager.UpdateAsync(user);

            await _context.SaveChangesAsync();

            return Ok(new { company, DateTime.Now });
        }


        #endregion

        #region MODIFICA DATI COMPANY

        [Authorize(Policy = "EditDataCompanyPermission")]
        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPut]
        [Route("EditDataCompany")]
        public async Task<IActionResult> EditDataCompany(EditCompanyInputModel editCompany)
        {
            if (!ModelState.IsValid) return StatusCode(406, $"The data you inserted are wrong {DateTime.Now}");

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            var companyOldData = _companyService.GetCompanyFromId((Int32)user.IdCompany);

            var companyDataEdit = new Company();

            if (!String.IsNullOrEmpty(editCompany.CompanyEmail))
            {
                if (_companyService.CheckUniqueEmail(editCompany.CompanyEmail)) return BadRequest(new { message = $"The iva you inserted belongs to another company", DateTime.Now });
            }
            else
            {
                companyDataEdit.CompanyEmail = companyOldData.CompanyEmail;
            }

            if (!String.IsNullOrEmpty(editCompany.CompanyTel))
            {
                if (_companyService.CheckUniqueTel(editCompany.CompanyTel)) return BadRequest(new { message = $"The phone you inserted belongs to another company", DateTime.Now });
            }
            else
            {
                companyDataEdit.CompanyTel = companyOldData.CompanyTel;
            }

            if (!String.IsNullOrEmpty(editCompany.CompanyIva))
            {
                if (CheckIvaCompliant(editCompany.CompanyIva))
                {
                    if (_companyService.CheckIvaAvailability(editCompany.CompanyIva)) return BadRequest(new { message = $"The iva you inserted belongs to another company.", DateTime.Now });
                }
                else
                {
                    return BadRequest(new { message = $"The iva you inserted is not compliant.", DateTime.Now });
                }
            }
            else
            {
                companyDataEdit.CompanyIva = companyOldData.CompanyIva;
            }

            companyDataEdit.CompanyName = companyDataEdit.CompanyName != null ? companyDataEdit.CompanyName : companyOldData.CompanyName;

            companyDataEdit.CompanyFax = companyDataEdit.CompanyFax != null ? companyDataEdit.CompanyFax : companyOldData.CompanyFax;

            await _userManager.RemoveFromRoleAsync(user, RoleHelper.EditCompanyPermissionRole);

            _context.Companies.Update(companyDataEdit);

            return Ok(new { message = $"The update of company {_dataHelper.CriptName(companyDataEdit.CompanyName)}", companyDataEdit, DateTime.Now });
        }

        #endregion

        #region AGGIUNGI REFERENTE PER AZIENDA

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("AddReferentToCompany")]
        public async Task<IActionResult> AddReferentToCompany(string iva, string emailUser)
        {
            var emailController = new EmailAddressAttribute();

            if (!CheckIvaCompliant(iva) || !emailController.IsValid(emailUser)) return StatusCode(406, $"The data you inserted are wrong.{DateTime.Now}");

            var user = await _userManager.FindByEmailAsync(emailUser);

            if (user == null) return NotFound(new { message = "No user was found", DateTime.Now });

            var company = _companyService.GetCompanyFromIva(iva);

            if (company == null) return NotFound(new { message = "No company was found", DateTime.Now });

            var request = await _context.Requests.Where(r => r.IdUser == user.Id && r.IdTypesRequest == 1).FirstOrDefaultAsync();

            if (company.User != null || user.Company != null) return BadRequest(new { message = "The user doesn't require any action to be promoted as referent of company", DateTime.Now });

            company.IdUser = user.Id;

            company.User = user;

            user.IdCompany = company.IdCompany;

            user.Company = company;

            if (!await _roleManager.RoleExistsAsync(RoleHelper.ReferentRole)) await _roleManager.CreateAsync(new IdentityRole { Name = RoleHelper.ReferentRole });

            //await _userManager.AddToRoleAsync(user, RoleHelper.ReferentRole);

            await _userManager.RemoveFromRoleAsync(user, RoleHelper.UserRole);

            await _context.SaveChangesAsync();

            _emailSender.SendNotificationUserIsNowReferent(company.CompanyName, company.CompanyIva, company.CompanyEmail, user.NameUser, user.LastNameUser, user.Email);

            return Ok(new { message = "The company now has a new referent", DateTime.Now });
        }

        #endregion

        #region GET DETTAGLI COMPANY

        [Authorize(Roles = RoleHelper.ReferentRole)]
        [HttpGet]
        [Route("GetCompanyDetails")]

        public async Task<ActionResult<Company>> GetCompanyDetails()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            return Ok(new { user.Company, DateTime.Now });
        }

        #endregion

        #region INTERNAL CHECK IVA COMPLIANT

        internal bool CheckIvaCompliant(string iva)
        {
            bool flagCompliant = false;
            return flagCompliant = iva == null || !iva.Any(i => char.IsLetterOrDigit(i)) || iva.Length != 11 ? false : true;
        }

        #endregion
    }
}
