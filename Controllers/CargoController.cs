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

            var statusCargoSelected = await _context.StatusCargos.FindAsync(newCargo.StatusNumber);

            if (statusCargoSelected == null) return NotFound(new { message = $"No status cargo with {newCargo.StatusNumber} id was found.", DateTime.Now });

            var referentUserSenderCompany = await _userManager.FindByIdAsync(companyReceiver.IdUser);

            var referentUserReceiverCompany = await _userManager.FindByIdAsync(companyReceiver.IdUser);

            //if(IcompanySender == IcompanyReceiver) return BadRequest( new { message = ""})
            //TODO francamente non saprei e` possibile ci siano piu` sedi della stessa societa` che possa inviarsi carichi

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
            cargoNew.CapCityLoading = Int32.Parse(newCargo.CapCityLoading);
            cargoNew.CompanySenderCargoEmail = companySender.CompanyEmail;
            cargoNew.CompanySenderCargoIva = companySender.CompanyIva;
            cargoNew.DeliveryDate = (DateTime)newCargo.DeliveryDate;
            cargoNew.DeliveryAddress = newCargo.DeliveryAddress;
            cargoNew.CapCityDelivery = Int32.Parse(newCargo.CapCityDelivery);
            cargoNew.CompanyReceiverCargoEmail = companyReceiver.CompanyEmail;
            cargoNew.CompanyReceiverCargoIva = companyReceiver.CompanyIva;
            cargoNew.FK_IdCompanySender = companySender.IdCompany;
            cargoNew.CompanySender = companySender;
            cargoNew.FK_IdCompanyReceiver = companyReceiver.IdCompany;

            //creating new object and save it for retriving the id
            using (var transaction = _context.Database.BeginTransaction())
            {
                await transaction.CreateSavepointAsync("before commit new cargo");

                try
                {
                    await _context.Cargos.AddAsync(cargoNew);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackToSavepointAsync("before commit new cargo");
                    return StatusCode(500, $"Ops, something went wrong CODE: ERRTRDBNC1 {DateTime.Now}");
                }

                //retring new id added from efcore

                //getting new id created before
                var newCargoCreated = await _context.Cargos.Where(c => c.TitleCargo == cargoNew.TitleCargo).FirstOrDefaultAsync();

                //creating the new object cargo event and bind it with cargo obj
                var newCargoEvent = new CargoEvent 
                { 
                    FK_TitleCargo = newCargo.TitleCargo, 
                    DateEvent = DateTime.Now, 
                    FK_IdStatusCargo = newCargo.StatusNumber,
                    StatusCargoes = statusCargoSelected,
                    NoteEvent = newCargo.NoteCargoEvent = newCargo.NoteCargoEvent == null ? "nessuna nota":newCargo.NoteCargoEvent, 
                    FK_IdCargo = newCargoCreated.IdCargo,
                    Cargoes = newCargoCreated
                };

                try
                {
                    await _context.CargoEvents.AddAsync(newCargoEvent);
                    companySender.Cargos.Add(cargoNew);
                    companyReceiver.Cargos.Add(cargoNew);
                    newCargoCreated.CargoEvents.Add(newCargoEvent);

                    _context.Companies.Update(companySender);
                    _context.Companies.Update(companyReceiver);
                    _context.Cargos.Update(newCargoCreated);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackToSavepointAsync("before commit new cargo");
                    return StatusCode(500, $"Ops, something went wrong CODE: ERRTRDBNC2 {DateTime.Now}");
                }
            }

            List<string> listEmailsNotification = new List<string>
            {
                companyReceiver.CompanyEmail,
                referentUserReceiverCompany.Email,
                companySender.CompanyEmail,
                referentUserReceiverCompany.Email
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

        private bool IsCompliantCargoDate(DateTime? loadingDate, DateTime? deliveryDate)
        {
            bool isCompliant = false;
            isCompliant = loadingDate < deliveryDate && deliveryDate > loadingDate ? true : false;
            return isCompliant;
        }

    }
}
