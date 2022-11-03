using FGMEmailSenderApp.Models;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.InputModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.Net.Http.Headers.CookieHeaders
using static Duende.IdentityServer.Models.IdentityResources;
using Microsoft.AspNetCore.Authentication;

namespace FGMEmailSenderApp.Controllers
{
    [Route("Identity/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _signInManager = signInManager;
        }

        #region REGISTRAZIONE NUOVO UTENTE

        [HttpPost]
        [AllowAnonymous]
        [Route("SignUp")]
        public async Task<IActionResult> SignUp(RegistrationInputModel inputUserModel)
        {
            if (!ModelState.IsValid) return StatusCode(406, new { message = "The information you inserted are not aceptable for signing up an user.", DateTime.Now });

            var user = await _userManager.FindByEmailAsync(inputUserModel.EmailUser);

            if (user != null) return BadRequest( new { message = "The email you signed is already used by another user.", DateTime.Now});

            if (user != null && await _userManager.IsLockedOutAsync(user)) return BadRequest(new { message = "The email you signed is already used by another user."
                + " Your account is actually frozen, we suggest you to wait 60 minuts"
                + " or contacting the administrator.",
                DateTime.Now
            });

            if (user != null && !user.EmailConfirmed) return BadRequest(new { message = "The email you signed is already used by another user."
                + " It's look like you need to confirm the email verification by click to the link that we send to your email.",
                DateTime.Now
            });

            var newUser = new ApplicationUser
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                NameUser = inputUserModel.NameUser,
                LastNameUser = inputUserModel.LastNameUser,
                UserName = $"{inputUserModel.NameUser}.{inputUserModel.LastNameUser}",
                Email = inputUserModel.EmailUser,
                PhoneNumber = inputUserModel.PhoneUser
            };

            string role_User = "User";

            if (await _roleManager.FindByNameAsync(role_User) == null) await _roleManager.CreateAsync(new IdentityRole(role_User));

            var result = await _userManager.CreateAsync(newUser, inputUserModel.Password);

            if (!result.Succeeded) 
            {
                /// Eliminazione eventuale residuo dell' utente che non si e' potuto registrare,
                /// su database, altrimenti non potra' + registrarsi perche' il controllo lo
                /// impedirebbe si elimina ogni info su db.
                
                try
                {
                    await _userManager.DeleteAsync(newUser);
                }
                catch (Exception) { }

                return StatusCode(500, new { message = "Something went wrong while creating your new user, try it later.", DateTime.Now });
            } 

            await _userManager.AddToRoleAsync(newUser, role_User);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

            var confirmationLink = Url.Link("email-confirmation", new { token, inputUserModel.EmailUser });            

            await _context.SaveChangesAsync();

            ///TODO CREATE EMAIL SERVICE TO SEND THE TOKEN

            return Ok( new { message = "Your sign up was perfectly maded, you will receive e-mail with token for the confirmation.", DateTime.Now });
        }

        #endregion

        #region CONFERMA REGISTRAZIONE UTENTE

        [AllowAnonymous]
        [HttpPost]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var emailController = new EmailAddressAttribute();

            if (emailController.IsValid(email)) return StatusCode(406, new { message = "The information provided is not an email address.", DateTime.Now });

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return NotFound( new { message = "The email you provide is not assigned to any users.", DateTime.Now });

            var result = await _userManager.ConfirmEmailAsync(user, token);

            return (result.Succeeded ? Ok(new { message = $"the {email} is been confirmed.", DateTime.Now }) : NotFound(new { message = "The token you provide was not found.", DateTime.Now }));
        }

        #endregion

        #region RINVIA EMAIL DI CONFERMA

        [AllowAnonymous]
        [HttpPost]
        [Route("ResendEmailConf")]
        public async Task<IActionResult> ResendEmailConfirmation(string email)
        {
            var emailController = new EmailAddressAttribute();

            if (!emailController.IsValid(email)) return StatusCode(406, new { message = "The information you provide is not an email address.", DateTime.Now } );

            var user = await _userManager.FindByEmailAsync(email);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.Link("email-confirmation", new { token, email });

            ///TODO Creare un servizio che invii autonomamente l'email;
            
            return Ok(new { message = $"We send another confirmation link at {email}", DateTime.Now });
        }

        #endregion

        #region CONTROLLO DI SESSIONE

        [HttpGet]
        [Route("CheckSession")]
        public async Task<IActionResult> CheckSession()
        {
            var userEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

            if (userEmail == null) return Unauthorized(new { message = "You are not authenticated.", DateTime.Now });

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null) return Unauthorized( new { message = "You are not authenticated.", DateTime.Now });

            if (user != null && await _userManager.IsLockedOutAsync(user)) return Unauthorized(new { message = "It seems your account is locked out. Try later", DateTime.Now }); 

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new { message = "You are authenticated.", user.Email, user.NameUser, user.LastNameUser, roles, DateTime.Now });
        }

        #endregion

        #region LOGIN

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        
        public async Task<IActionResult> Login(LoginInputModel loginModel)
        {
            if (!ModelState.IsValid) return StatusCode(406, new { message = "Invalid email or password.", DateTime.Now});

            var user = await _userManager.FindByEmailAsync(loginModel.Email);

            if (user == null || await _userManager.CheckPasswordAsync(user, loginModel.Password) == false)
            {
                if (user != null) await _userManager.AccessFailedAsync(user);

                return StatusCode(401, new { message = "Wrong email or password.", DateTime.Now });
            }

            if (_signInManager.IsSignedIn(User)) return BadRequest(new { message = "You are already logged in.", DateTime.Now });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, loginModel.RememberMe);

            if (!result.Succeeded) return StatusCode(401, new { message = "Wrong email or password", DateTime.Now });

            if (result.IsLockedOut) return StatusCode(401, new { message = "Your account is been locked out, try to after 1 hour or contact the administrator", DateTime.Now });

            if (result.IsNotAllowed) return StatusCode(401, new { message = "Your account is not allowed to log in anymore. Try to contact the administrator.", DateTime.Now });

            if (result.RequiresTwoFactor)
            {
                CreatingTokenCookie2FA(loginModel.Email);

                return Ok(new { message = $"success = {result}, We send you the Two Factory Authentication token to your e-mail or your phone cell.", DateTime.Now });
            }
            else
            {
                var roles = CreatingAuthCookie(user, loginModel.RememberMe);

                return Ok(new { message = "success", user.Email, user.NameUser, user.LastNameUser, roles, DateTime.Now });
            }
        }

        #endregion

        #region INTERNAL - CHECK TOKEN COOKIE FOR 2 FACTORY AUTHENTICATION

        internal void CreatingTokenCookie2FA(string email)
        {
            string nameToken2FA = "FGM2FAStatus";

            var dataCookie = Request.Cookies[nameToken2FA];

            if (dataCookie != null) return;

            CookieOptions optionCookie = new CookieOptions();

            optionCookie.Expires = DateTime.Now.AddMinutes(30);

            optionCookie.HttpOnly = true;

            Response.Cookies.Append(nameToken2FA, $"{email}", optionCookie);
        }

        #endregion

        #region INTERNAL - CREATING AUTHENTICATION AND AUTHORIZATION COOKIE

        internal async Task<IList<string>> CreatingAuthCookie(ApplicationUser user, bool rememberMe)
        {
            var rolesUser = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.NameUser),
                    new Claim(ClaimTypes.Surname, user.LastNameUser),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

            foreach (var role in rolesUser)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "MyFGMTrasportiIdentity");

            var claimsPrincipal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyFGMTrasportiIdentity", claimsPrincipal, new AuthenticationProperties()
            {
                IsPersistent = rememberMe,
            });

            return rolesUser;
        }

        #endregion
    }
}
