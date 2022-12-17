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

                _emailSender.SendNotificationUserChangeRole(user.NameUser, user.LastNameUser, user.Email, "createCompany", iva);

                return Ok(new { message = "The partita iva you typed is available", DateTime.Now }); 
            }

            if (companyClaimed.User == null) return Ok(new { message = $"The P Iva is used by {_dataHelper.CriptName(companyClaimed.CompanyName)} company, but has no reference with an user.", DateTime.Now });

            if (!string.Equals(companyClaimed.User.Id, userId)) return Ok(new { message = "This partita iva belongs to a company.", DateTime.Now });

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

            await _context.SaveChangesAsync();

            return Ok(new { company, DateTime.Now });
        }

        #endregion

        #region MODIFICA DATI COMPANY

        //TODO

        #endregion

        #region INVIA RICHIESTA REFERENTE AZIENDALE

        [Authorize]
        [HttpPost]
        [Route("CreateRequestForAddReferent")]
        public async Task<IActionResult> CreateRequestForAddReferent(string iva)
        {
            if (!CheckIvaCompliant(iva)) return StatusCode(406, $"The information you inserted is not a complant P IVA. {DateTime.Now}");

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            if (_companyService.CheckIvaAvailability(iva)) return NotFound(new { message = "The Iva you inserted doesn't belowed to any companies.", DateTime.Now });

            var companyClaimed = _context.Companies.Where(c => c.CompanyIva == iva && c.User == null);

            if (companyClaimed == null) return BadRequest(new { message = "The company has already a referent.", DateTime.Now });

            Request request = new Request();

            request.User = user;
            request.IdUser = user.Id;
            request.DescriptionRequest = string.Join(ETypeRequest.RichiestaAggiungaReferenteAziendale.ToString(), iva);
            request.TypesRequest = new TypeRequest { TypeNameRequest = ETypeRequest.RichiestaAggiungaReferenteAziendale.ToString(), Requests = new List<Request> { request } };

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Your request {ETypeRequest.RichiestaAggiungaReferenteAziendale.ToString()} was sent to our staff. You'll receive the answer to {_dataHelper.CriptEmail(user.Email)}", request ,DateTime.Now });
        }

        #endregion

        #region AGGIUNGI REFERENTE PER AZIENDA

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("AddReferentToCompany")]
        public async Task<IActionResult> AddReferentToCompany(string iva, string emailUser)
        {
            var emailController = new EmailAddressAttribute();

            if (CheckIvaCompliant(iva) || !emailController.IsValid(emailUser)) return StatusCode(406, $"The data you inserted are wrong.{DateTime.Now}");

            var user = await _userManager.FindByEmailAsync(emailUser);

            if (user == null) return NotFound(new { message = "No user was found", DateTime.Now });

            var company = _companyService.GetCompanyFromIva(iva);

            if (company == null) return NotFound(new { message = "No company was found", DateTime.Now });

            if (!_context.Requests.Any(r => r.IdUser == user.Id && r.DescriptionRequest == iva || company.User != null || user.Company != null)) return BadRequest(new { message = "The user doesn't require any action to be promoted as referent of company", DateTime.Now });

            var request = _context.Requests.Where(r => r.IdUser == user.Id && r.DescriptionRequest == iva);

            company.IdUser = user.Id;

            company.User = user;

            user.IdCompany = company.IdCompany;

            user.Company = company;

            if (!await _roleManager.RoleExistsAsync(RoleHelper.ReferentRole)) await _roleManager.CreateAsync(new IdentityRole { Name = RoleHelper.ReferentRole });

            await _userManager.AddToRoleAsync(user, RoleHelper.ReferentRole);

            await _context.SaveChangesAsync();

            request.FirstOrDefault().Response = request.FirstOrDefault().Response == null ? true : true;

            _emailSender.SendNotificationResponseRequest(request.FirstOrDefault().IdRequest, request.FirstOrDefault().TypesRequest.TypeNameRequest.ToString() ,request.FirstOrDefault().DescriptionRequest ,request.FirstOrDefault().Response, user.Email, user.NameUser, user.LastNameUser);

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
