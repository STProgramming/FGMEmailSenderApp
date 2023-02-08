using EmailService;
using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.InputModels;
using FGMEmailSenderApp.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;

namespace FGMEmailSenderApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CargoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ICompanyService _companyService;
        private readonly ILocationService _locationService;

        public CargoController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            ICompanyService companyService,
            ILocationService locationService
            )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _companyService = companyService;
            _locationService = locationService;
        }

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpGet]
        [Route("GetAllCargos")]
        public async Task<ActionResult<ICollection<Cargo>>> GetAllCargos()
        {
            return await _context.Cargos.ToListAsync();
        }

        [Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        [HttpPost]
        [Route("AddCargo")]
        public async Task<IActionResult> AddCargo(AddCargoInputModel newCargo)
        {
            if (!ModelState.IsValid) return StatusCode(406, $"The data you inserted is not valid. {DateTime.Now}");

            //Caricamento della compagnia Sender

            var EcompanySender = _companyService.GetCompanyFromEmail(newCargo.CompanySenderCargoEmail);

            var IcompanySender = _companyService.GetCompanyFromIva(newCargo.CompanySenderCargoIva);

            if (IcompanySender == null || EcompanySender == null) return NotFound(new { message = $"Email or Iva does not belong to any sender company.", DateTime.Now });

            if (IcompanySender != EcompanySender) return BadRequest(new { message = $"Email or Iva belongs to different company. These must belong to the same sender company", DateTime.Now });

            //Per nomenclatura preferisco allocare ad una nuova variabile i dati caricati dal service

            var companySender = IcompanySender;

            //Caricamento della compagnia Receiver

            var EcompanyReceiver = _companyService.GetCompanyFromEmail(newCargo.CompanyReceiverCargoEmail);

            var IcompanyReceiver = _companyService.GetCompanyFromIva(newCargo.CompanyReceiverCargoIva);

            if (EcompanyReceiver == null || IcompanyReceiver == null) return NotFound(new { message = $"Email or Iva does not belong to any receiver company.", DateTime.Now });

            if (EcompanyReceiver != IcompanyReceiver) return BadRequest(new { message = $"Email or Iva belongs to different company. These must belong to the same receiver company", DateTime.Now });

            var companyReceiver = IcompanyReceiver;

            if (!IsCompliantCargoDate(newCargo.LoadingDate, newCargo.DeliveryDate)) return BadRequest(new { message = $"The dates you inserted are not compliant.", DateTime.Now });

            if (IsUniqueTitleCargo(newCargo.TitleCargo)) return BadRequest(new { message = "The title cargo you inserted is already used.", DateTime.Now });

            Cargo cargoNew = new Cargo();

            cargoNew.TitleCargo = newCargo.TitleCargo;
            cargoNew.DetailCargo= newCargo.DetailCargo;
            cargoNew.DescriptionCargo = newCargo.DescriptionCargo;
            cargoNew.NoteCargo = newCargo.NoteCargo;
            cargoNew.HeightCargo = newCargo.HeightCargo;
            cargoNew.LenghtCargo = newCargo.LenghtCargo;
            cargoNew.WeightCargo = newCargo.WeigthCargo;
            cargoNew.DepthCargo = newCargo.DepthCargo;
            cargoNew.LoadingDate = (DateTime)newCargo.LoadingDate;
            cargoNew.LoadingAddress = newCargo.LoadingAddress;
            cargoNew.CapCityLoading = newCargo.CapCityLoading;
            cargoNew.CompanySenderCargoEmail = companySender.CompanyEmail;
            cargoNew.CompanySenderCargoIva = companySender.CompanyIva;
            cargoNew.DeliveryDate = (DateTime)newCargo.DeliveryDate;
            cargoNew.DeliveryAddress = newCargo.DeliveryAddress;
            cargoNew.CapCityDelivery = newCargo.CapCityDelivery;
            cargoNew.CompanyReceiverCargoEmail = companyReceiver.CompanyEmail;
            cargoNew.CompanySenderCargoIva = companyReceiver.CompanyIva;
            cargoNew.FK_IdCompanySender = companySender.IdCompany;
            cargoNew.FK_IdCompanyReceiver = companyReceiver.IdCompany;
            var newCargoEvent = new CargoEvent { FK_TitleCargo = newCargo.TitleCargo, DateEvent = DateTime.Now, FK_IdStatusCargo = newCargo.StatusNumber, NoteEvent = newCargo.NoteCargoEvent };
            cargoNew.CargoEvents.Add(newCargoEvent);

            await _context.Cargos.AddAsync(cargoNew);
            companySender.Cargos.Add(cargoNew);
            companyReceiver.Cargos.Add(cargoNew);
            await _context.CargoEvents.AddAsync(newCargoEvent);

            await _context.SaveChangesAsync();

            List<string> listEmailsNotification = new List<string>
            {
                companyReceiver.CompanyEmail,
                companyReceiver.User.Email,
                companySender.CompanyEmail,
                companySender.User.Email
            };

            _emailSender.CreateNewCargo(listEmailsNotification, newCargo.TitleCargo);

            return Ok(new { message = "The cargo was created successfully.", DateTime.Now });
        }

        //[Authorize(Roles = RoleHelper.FGMEmployeeInternalRole)]
        //[HttpPost]
        //[Route("AddCargos")]
        //public async Task<IActionResult> AddCargos()
        //{

        //}

        [Authorize]
        [HttpGet]
        [Route("GetCargoDetails")]
        public async Task<ActionResult<Cargo>> GetCargoDetails(string titleCargo)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            var roleUser = await _userManager.GetRolesAsync(user);

            if (roleUser.Contains(RoleHelper.UserRole) || roleUser.Contains(RoleHelper.ReferentRole))
            {
                var company = await _context.Companies.Where(c => c.IdCompany == user.Company.IdCompany).FirstOrDefaultAsync();

                var cargo = _context.Cargos.Where(c => c.TitleCargo == titleCargo && c.FK_IdCompanyReceiver == company.IdCompany || c.FK_IdCompanySender == company.IdCompany);

                return Ok(new { cargo, DateTime.Now });
            }
            else
            {
                var cargo = _context.Cargos.Where(c => c.TitleCargo == titleCargo);

                return Ok(new { cargo, DateTime.Now });
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetUserCargos")]
        public async Task<ActionResult<ICollection<Cargo>>> GetUserCargos()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            var company = _companyService.GetCompanyFromId((Int32)user.IdCompany);

            return Ok(new { company.Cargos, DateTime.Now });
        }

        private bool IsUniqueTitleCargo(string titleCargo)
        {
            return _context.Cargos.Any(c => c.TitleCargo == titleCargo);
        }

        private string GetNameStatusByNumber(int numberStatus)
        {
            switch (numberStatus)
            {
                case 1:
                    return EStatusCargo.Scaricato.ToString();

                case 2:
                    return EStatusCargo.InTransito.ToString();

                case 3:
                    return EStatusCargo.NonCaricato.ToString();

                case 4:
                    return EStatusCargo.Caricando.ToString();

                case 5:
                    return EStatusCargo.Scaricando.ToString();

                case 6:
                    return EStatusCargo.Problema.ToString();

                case 7:
                    return EStatusCargo.InPartenza.ToString();

                case 8:
                    return EStatusCargo.InArrivo.ToString();

                case 9:
                    return EStatusCargo.Consegnato.ToString();

                case 10:
                    return EStatusCargo.NonConsegnato.ToString();

                default:
                    throw new NotImplementedException();
            }
        }

        private bool IsCompliantCargoDate(DateTime? loadingDate, DateTime? deliveryDate)
        {
            bool isCompliant = false;
            isCompliant = loadingDate < deliveryDate && deliveryDate > loadingDate ? true : false;
            return isCompliant;
        }

    }
}
