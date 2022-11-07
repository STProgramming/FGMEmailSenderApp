using FGMEmailSenderApp.Models.EntityFrameworkModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace FGMEmailSenderApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Route("Seed/api/[controller]")]
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

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        [Route("AddNewRole")]
        public async Task<IActionResult> AddRole(string role)
        {
            if (await _roleManager.RoleExistsAsync(role)) return BadRequest();

            IdentityRole newRole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = role,
                NormalizedName = role.ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            await _roleManager.CreateAsync(newRole);

            await _context.SaveChangesAsync();

            return Ok( await _roleManager.FindByNameAsync(role));
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        [Route("GetAllUsers")]
        public async Task<ActionResult<List<ApplicationUser>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        [Route("GetAllCompanies")]
        public async Task<ActionResult<List<Company>>> GetAllCompanies()
        {
            return await _context.Companies.ToListAsync();
        }
    }
}
