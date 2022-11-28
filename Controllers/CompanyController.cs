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
        private readonly IDataHelper _dataHelper;

        public CompanyController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IDataHelper dataHelper
            ) 
        {
            _context = context;
            _userManager = userManager;
            _dataHelper = dataHelper;
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

            if (companyClaimed == null) return Ok(new { message = "The partita iva you typed is available", DateTime.Now });

            if (companyClaimed.Users == null) return Ok(new { message = $"The P Iva is used by {_dataHelper.CriptName(companyClaimed.CompanyName)} company, but has no reference. The system has registered your request. You may receive an email from administrator", DateTime.Now });

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

        [Authorize(Policy = "AddCompanyData")]
        [HttpPost]
        [Route("AddCompanyData")]
        public async Task<IActionResult> AddCompanyData([FromBody] AddCompanyInputModel newCompany)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            if(CheckIvaAvailability(newCompany.CompanyIva)) return BadRequest( new { message = "The partita iva you inserted belongs to another company.", DateTime.Now });


        }

        #endregion

        #region PRIVATE ACTION

        #region CHECK P IVA AVAILABILITY

        private bool CheckIvaAvailability(string iva)
        {
            return _context.Companies.Any(c => string.Equals(c.CompanyIva, iva));
        }

        #endregion

        #endregion
    }
}
