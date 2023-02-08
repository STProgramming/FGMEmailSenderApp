using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace FGMEmailSenderApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut]
        [Route("PromoteUserAtRoleFGM")]
        public async Task<IActionResult> PromoteUserAtRoleFGM(string email)
        {
            var emailController = new EmailAddressAttribute();

            if (!emailController.IsValid(email)) return StatusCode(406);

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var roleClaimUser = await _userManager.GetRolesAsync(user);

            if(roleClaimUser.Count > 0) await _userManager.RemoveFromRoleAsync(user, roleClaimUser.FirstOrDefault());

            if (await _roleManager.FindByNameAsync(RoleHelper.FGMEmployeeInternalRole) == null) await _roleManager.CreateAsync(new IdentityRole(RoleHelper.FGMEmployeeInternalRole));

            await _userManager.AddToRoleAsync(user, RoleHelper.FGMEmployeeInternalRole);

            await _userManager.UpdateAsync(user);

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [Route("AddNewRole")]
        public async Task<IActionResult> AddRole(string role)
        {
            if (await _roleManager.RoleExistsAsync(role)) return BadRequest();            

            await _roleManager.CreateAsync(new IdentityRole(role));

            await _context.SaveChangesAsync();

            return Ok( await _roleManager.FindByNameAsync(role));
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("GetAllUsers")]
        public List<ApplicationUser> GetAllUsers()
        {
            return _userManager.Users.ToList();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("GetAllCompanies")]
        public async Task<ActionResult<List<Company>>> GetAllCompanies()
        {
            return await _context.Companies.ToListAsync();
        }

        [Authorize(Roles = RoleHelper.UserRole)]
        [HttpGet]
        [Route("CreateNewAdmin")]

        public async Task<IActionResult> CreateNewAdmin()
        {
            var user = await _userManager.FindByEmailAsync("stcorp@outlook.it");

            await _userManager.RemoveFromRoleAsync(user, "User");

            string adminRole = "Administrator";

            await _roleManager.CreateAsync(new IdentityRole(adminRole));

            await _userManager.AddToRoleAsync(user, adminRole);

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete]
        [Route("RemoveFromRole")]
        public async Task<IActionResult> RemoveFromRole(string role, string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);

            var roleUserIsInCharge = await _roleManager.FindByNameAsync(role);

            if (roleUserIsInCharge == null) return NotFound();

            if(!await _userManager.IsInRoleAsync(user, role)) return BadRequest();

            await _userManager.RemoveFromRoleAsync(user, role);

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete]
        [Route("RemoveUser")]
        public async Task<IActionResult> RemoveUser(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);

            await _userManager.DeleteAsync(user);

            return Ok(user);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("PopulateTypesRequest")]
        public async Task<IActionResult> PopulateTypesRequest()
        {
            var TypeRequestOne = new TypeRequest { TypeNameRequest = ETypeRequest.RichiestaAggiungaReferenteAziendale.ToString() };
            var TypeRequestTwo = new TypeRequest { TypeNameRequest = ETypeRequest.ModificaDatiAziendali.ToString() };
            var TypeRequestThree = new TypeRequest { TypeNameRequest = ETypeRequest.PropostaCarico.ToString() };

            await _context.TypesRequest.AddAsync(TypeRequestOne);
            await _context.TypesRequest.AddAsync(TypeRequestTwo);
            await _context.TypesRequest.AddAsync(TypeRequestThree);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("PopulateStatusCargo")]
        public async Task<IActionResult> PopulateStatusCargo()
        {
            var StatusCargo1 = new StatusCargo { NameStatusCargo = EStatusCargo.Scaricato.ToString() };
            var StatusCargo2 = new StatusCargo { NameStatusCargo = EStatusCargo.InTransito.ToString() };
            var StatusCargo3 = new StatusCargo { NameStatusCargo = EStatusCargo.NonCaricato.ToString() };
            var StatusCargo4 = new StatusCargo { NameStatusCargo = EStatusCargo.Caricando.ToString() };
            var StatusCargo5 = new StatusCargo { NameStatusCargo = EStatusCargo.Scaricando.ToString() };
            var StatusCargo6 = new StatusCargo { NameStatusCargo = EStatusCargo.Problema.ToString() };
            var StatusCargo7 = new StatusCargo { NameStatusCargo = EStatusCargo.InPartenza.ToString() };
            var StatusCargo8 = new StatusCargo { NameStatusCargo = EStatusCargo.InArrivo.ToString() };
            var StatusCargo9 = new StatusCargo { NameStatusCargo = EStatusCargo.Consegnato.ToString() };
            var StatusCargo10 = new StatusCargo { NameStatusCargo = EStatusCargo.NonConsegnato.ToString() };

            await _context.StatusCargos.AddAsync(StatusCargo1);
            await _context.StatusCargos.AddAsync(StatusCargo2);
            await _context.StatusCargos.AddAsync(StatusCargo3);
            await _context.StatusCargos.AddAsync(StatusCargo4);
            await _context.StatusCargos.AddAsync(StatusCargo5);
            await _context.StatusCargos.AddAsync(StatusCargo6);
            await _context.StatusCargos.AddAsync(StatusCargo7);
            await _context.StatusCargos.AddAsync(StatusCargo8);
            await _context.StatusCargos.AddAsync(StatusCargo9);
            await _context.StatusCargos.AddAsync(StatusCargo10);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
