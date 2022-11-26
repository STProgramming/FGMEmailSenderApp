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
        [Route("PromoteUserAtRole")]
        public async Task<IActionResult> PromoteUserAtRole(string email, string role)
        {
            var emailController = new EmailAddressAttribute();

            if (emailController.IsValid(email)) return StatusCode(406);

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return NotFound();

            var roleClaimUser = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRoleAsync(user, roleClaimUser.FirstOrDefault());

            await _userManager.AddToRoleAsync(user, role);

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
        public async Task<ActionResult<List<ApplicationUser>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        [Route("GetAllCompanies")]
        public async Task<ActionResult<List<Company>>> GetAllCompanies()
        {
            return await _context.Companies.ToListAsync();
        }

        [Authorize(Roles = "User")]
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
    }
}
