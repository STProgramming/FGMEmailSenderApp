using FGMEmailSenderApp.Models;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.InputModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static Duende.IdentityServer.Models.IdentityResources;
using Microsoft.AspNetCore.Authentication;
using System.Security;
using FGMEmailSenderApp.Helpers;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Manage.Internal;
using System.Data;

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
        private readonly DataHelper _dataHelper;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager,
            DataHelper dataHelper
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _signInManager = signInManager;
            _dataHelper = dataHelper;
        }

        #region REGISTRAZIONE NUOVO UTENTE

        [HttpPost]
        [AllowAnonymous]
        [Route("SignUp")]
        public async Task<IActionResult> SignUp(RegistrationInputModel inputUserModel)
        {
            if (!ModelState.IsValid) return StatusCode(406, new { message = "The information you inserted are not aceptable for signing up an user.", DateTime.Now });

            var user = await _userManager.FindByEmailAsync(inputUserModel.EmailUser);

            if (_context.Users.Any(x => x.PhoneNumber.Equals(inputUserModel.PhoneUser))) return BadRequest(new { message = "Your phone is already used by another user", DateTime.Now });

            if (user != null) return BadRequest( new { message = "The email you signed is already used by another user.", DateTime.Now });

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

            return Ok( new { message = "Your sign up was perfectly maded, you will receive e-mail with token for the confirmation.", user.Email ,DateTime.Now });
        }

        #endregion

        #region CONFERMA EMAIL

        [HttpPost]
        [AllowAnonymous]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            if (token == null || email == null) return StatusCode(406, new { message = "You need to provide the informations", DateTime.Now });

            var emailController = new EmailAddressAttribute();

            if (emailController.IsValid(email)) return StatusCode(406, new { message = "The information provided is not an email address.", DateTime.Now });

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return NotFound( new { message = "The email you provide is not assigned to any users.", DateTime.Now });

            var result = await _userManager.ConfirmEmailAsync(user, token);

            return (result.Succeeded ? Ok(new { message = $"the {email} is been confirmed.", DateTime.Now }) : NotFound(new { message = "The token you provide was not found.", DateTime.Now }));
        }

        #endregion

        #region RINVIA EMAIL DI CONFERMA

        [HttpPost]
        [AllowAnonymous]
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

        #region LOGIN

        [HttpPost]
        [AllowAnonymous]
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

            if (!result.Succeeded)
            {
                await _userManager.AccessFailedAsync(user);

                return StatusCode(401, new { message = "Wrong email or password", DateTime.Now });
            }

            if (result.IsLockedOut) return StatusCode(401, new { message = "Your account is been locked out, try to after 1 hour or contact the administrator", DateTime.Now });

            if (result.IsNotAllowed) return StatusCode(401, new { message = "Your account is not allowed to log in anymore. Try to contact the administrator.", DateTime.Now });

            if (result.RequiresTwoFactor)
            {
                await GenerateToken2FA();

                return Ok(new { message = $"success = {result}, We send you the Two Factory Authentication token to your e-mail or your phone cell.", DateTime.Now });
            }
            else
            {
                var roles = await CreatingAuthCookie(user, loginModel.RememberMe);

                return Ok(new { message = "success", user.Email, user.NameUser, user.LastNameUser, roles, DateTime.Now });
            }
        }

        #endregion

        #region VERIFICA 2FA

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [Route("Verify2FA")]
        public async Task<IActionResult> Verify2FA(string codeToken, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if(user == null) throw new SecurityException($"Access denied, you have to provide you are enabled to two factory authentication {DateTime.Now}");

            string tokenProvider = user.PhoneNumberConfirmed ? "SMS" : "E-mail";

            if (codeToken == null) return StatusCode(403, new { message = "You need to provide the token of two factory authentication", DateTime.Now });

            if (codeToken.All(c => Char.IsLetterOrDigit(c))) throw new SecurityException($"Your token doesn't respect the security role {DateTime.Now}");

            //TODO ATTUALMENTE HARD CODATO IN ATTESA DI UNA DELUCIDAZIONE SU COME PRENDERE QUESTO BOOL "REMEMBER CLIENT"

            var result = await _signInManager.TwoFactorSignInAsync(tokenProvider, codeToken, rememberMe, true);

            if (!result.Succeeded)
            {
                await _userManager.AccessFailedAsync(user);

                return StatusCode(401, new { message = "Wrong email or password", DateTime.Now });
            }

            if (result.IsLockedOut) return StatusCode(401, new { message = "Your account is been locked out, try to after 1 hour or contact the administrator", DateTime.Now });

            if (result.IsNotAllowed) return StatusCode(401, new { message = "Your account is not allowed to log in anymore. Try to contact the administrator.", DateTime.Now });

            var roles = await CreatingAuthCookie(user, rememberMe);

            return Ok(new { message = "success", user.Email, user.NameUser, user.LastNameUser, roles, DateTime.Now });
        }

        #endregion

        #region LOGOUT

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpGet]
        [Route("Logout")]
        public async Task<ActionResult> LogOut()
        {
            var userEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null) return NotFound();

            await _signInManager.SignOutAsync();

            await HttpContext.SignOutAsync("MyFGMTrasportiIdentity");

            return Ok(new { message = "You successfully logged out", DateTime.Now });
        }

        #endregion

        #region CONTROLLO DI SESSIONE

        [HttpGet]
        [AllowAnonymous]
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

        #region CAMBIA PASSWORD

        [HttpPost]
        [Authorize]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            return Ok( new { message = "You have changed successfully the password", DateTime.Now });
        }

        #endregion

        #region CAMBIA EMAIL

        [HttpPost]
        [Authorize]
        [Route("ChangeEmail")]
        public async Task<IActionResult> ChangeEmail(string oldEmail, string newEmail)
        {
            var emailController = new EmailAddressAttribute();

            if (oldEmail == null || newEmail == null) return BadRequest(new { message = "The fields are required", DateTime.Now });

            if (!emailController.IsValid(oldEmail) || !emailController.IsValid(newEmail)) return StatusCode(406, new { message = "The information you provide is not an email address.", DateTime.Now });

            var user = await _userManager.FindByEmailAsync(oldEmail);

            if (user == null) return NotFound(new { message = "the e-mail you inserted is not one of our users", DateTime.Now });

            var userEmailClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (!string.Equals(oldEmail, userEmailClaim)) throw new SecurityException($"The email you are trying to change is not your {DateTime.Now}");

            await _userManager.ChangeEmailAsync(user, oldEmail, newEmail);

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

            var confirmationLink = Url.Link("email-confirmation", new { token, newEmail });

            await _context.SaveChangesAsync();

            //TODO INVIARE TOKEN CON UN EMAIL SENDER

            RedirectToAction("Logout");

            return Ok(new { message = $"You will receive to your {_dataHelper.CriptEmail(newEmail)} a link to conferme the new e-mail provided. Now You automatically being redirect to logout", DateTime.Now });
        }

        #endregion

        #region CAMBIA NUMERO DI TELEFONO

        [HttpPost]
        [Authorize]
        [Route("ChangePhoneNumber")]
        public async Task<IActionResult> ChangePhoneNumber(string oldPhone, string newPhone)
        {
            if (oldPhone == null || newPhone == null) return StatusCode(406, new { message = "The values inserted are null", DateTime.Now });

            if (!oldPhone.Any(x => char.IsDigit(x)) || !newPhone.Any(c => char.IsDigit(c)) || oldPhone.Length != 10 || newPhone.Length != 10) return StatusCode(406, new { message = "The datas you insered are not phone number", DateTime.Now });

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            if (_context.Users.Any(u => u.PhoneNumber.Equals(oldPhone))) return NotFound(new { message = "Your phone is not found", DateTime.Now });

            var userClaimPhone = _context.Users.Where(u => u.PhoneNumber.Equals(oldPhone));

            if (user != userClaimPhone) return BadRequest(new { message = "The phone you inserted is not your", DateTime.Now });

            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, newPhone);

            var confirmationLink = Url.Link("phone-confirmation", new { token, newPhone });

            //TODO SMS HELPER

            return Ok(new { message = $"You will receive the OTP to your new {_dataHelper.CriptPhone(newPhone)}", DateTime.Now });
        }

        #endregion

        #region CONFERMA NUMERO DI TELEFONO

        [HttpPost]
        [Authorize]
        [Route("ConfirmPhone")]
        public async Task<IActionResult> ConfirmPhone(string token, string phone)
        {
            if (token == null || phone == null) return StatusCode(406, new { message = "You need to provide the informations", DateTime.Now });

            if (phone.Any(p => char.IsDigit(p))) return StatusCode(406, new { message = "The information provided is not an phone number.", DateTime.Now });

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            if (_context.Users.Any(u => u.PhoneNumber.Equals(phone))) return NotFound(new { message = "Your phone is not found", DateTime.Now });

            var userClaimPhone = _context.Users.Where(u => u.PhoneNumber.Equals(phone));

            if (user != userClaimPhone) return BadRequest(new { message = "The phone you inserted is not your", DateTime.Now });

            var result = await _userManager.VerifyChangePhoneNumberTokenAsync(user, token, phone);

            return (result ? Ok(new { message = $"The {_dataHelper.CriptPhone(phone)} is been confirmed.", DateTime.Now }) : NotFound(new { message = "The token you provide was not found.", DateTime.Now }));
        }


        #endregion

        #region MODIFICA UTENTE

        //TODO

        //attualmente scartato dato che quello che posso modificare sono solo nome e cognome il resto è stato tutto gestito

        #endregion

        //TODO DA FARE PASSWORD DIMENTICATA

        #region PRIVATE ACTION - GENERATE 2 FACTORY AUTHENTICATION TOKEN AND SEND IT

        private async Task<IActionResult> GenerateToken2FA()
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null) throw new SecurityException($"Access denied, you have to provide you are enabled to two factory authentication {DateTime.Now}");

            string tokenProvider = String.Empty;

            if (await _userManager.IsPhoneNumberConfirmedAsync(user)) tokenProvider = "SMS";

            else tokenProvider = "Email";
            
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, tokenProvider);

            string sourceCredential = string.Empty;

            string criptedCredential = string.Empty;

            switch (tokenProvider)
            {
                case "SMS":
                    sourceCredential = user.PhoneNumber;
                    criptedCredential = _dataHelper.CriptPhone(sourceCredential);
                    //TODO smshelper
                    break;

                default:
                    sourceCredential = user.Email;
                    criptedCredential = _dataHelper.CriptEmail(sourceCredential);
                    //TODO emailhelper
                    break;
            }

            return Ok( new { message = $"You will receive via {tokenProvider} the OTP on your" + (tokenProvider.Contains("SMS") ? " phone number " : " e-mail address") + $" {criptedCredential} ", DateTime.Now });
        }

        #endregion

        #region PRIVATE - CREATING AUTHENTICATION AND AUTHORIZATION COOKIE

        private async Task<IList<string>> CreatingAuthCookie(ApplicationUser user, bool rememberMe)
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
