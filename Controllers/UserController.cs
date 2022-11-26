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
using FGMEmailSenderApp.Models.Interfaces;
using System.Web;
using FGMEmailSenderApp.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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
        private readonly IDataHelper _dataHelper;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager,
            IDataHelper dataHelper
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _signInManager = signInManager;
            _dataHelper = dataHelper;
        }

        //TODO CONTROLLARE PERCHE' IL TOKEN DI CONFERMA EMAIL VIENE SEMPRE INVALIDATO
        //TODO CREARE L'AZIONE CHE TI PERMETTE DI GENERARE ED INVIARE IL TOKEN DI CONFERMA DEL NUMERO DI TELEFONO
        //TODO CREARE L'AZIONE DI RINVIO TOKEN DI CONFERMA DEL CELLULARE
        //TODO CREARE L'AZIONE CHE TI PERMETTE DI GENERARE IL TOKEN 2FA CON LA SCELTA DEL CANALE DI COMUNICAZIONE IN CUI SI VUOLE L'INVIO
        //TODO DA FARE PASSWORD DIMENTICATA

        #region REGISTRAZIONE NUOVO UTENTE

        [AllowAnonymous]
        [HttpPost]
        [Route("SignUp")]
        public async Task<IActionResult> SignUp([FromBody]RegistrationInputModel inputUserModel)
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

            //EmailConfirmed = true, hardcodata per evitare problemi in fase di test

            var newUser = new ApplicationUser
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                NameUser = inputUserModel.NameUser,
                LastNameUser = inputUserModel.LastNameUser,
                UserName = $"{inputUserModel.NameUser}.{inputUserModel.LastNameUser}",
                Email = inputUserModel.EmailUser,
                PhoneNumber = inputUserModel.PhoneUser,
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

            return Ok( new { message = "Your sign up was perfectly maded, you will receive e-mail with token for the confirmation.", inputUserModel.EmailUser, DateTime.Now });
        }

        #endregion

        #region LOGIN

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]   
        public async Task<IActionResult> Login([FromBody]LoginInputModel loginModel)
        {
            if (!ModelState.IsValid) return StatusCode(406, new { message = "Invalid email or password.", DateTime.Now});

            var user = new ApplicationUser();

            switch (CheckEmailOrPhone(loginModel.Email))
            {
                case 1:
                    user = await _userManager.FindByEmailAsync(loginModel.Email);
                    break;

                case 2:
                    user = _context.Users.Where(u => u.PhoneNumber.Equals(loginModel.Email)).FirstOrDefault();
                    if (user != null)
                    {
                        if (!user.PhoneNumberConfirmed) return BadRequest(new { message = $"Dear {_dataHelper.CriptName(user.NameUser)}, for login with phone number is necessary to confirm that. Until your phone is not confirmed, you can't procede by login using it.", DateTime.Now });
                    }
                    break;
                default:
                    return NotFound(new { message = "Wrong email or password.", DateTime.Now });
            }

            if (user == null || await _userManager.CheckPasswordAsync(user, loginModel.Password) == false)
            {
                if (user != null) await _userManager.AccessFailedAsync(user);

                return StatusCode(401, new { message = "Wrong email or password.", DateTime.Now });
            }

            if (_signInManager.IsSignedIn(User)) 
            {
                var emailUserLoggedIn = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                if (String.Equals(emailUserLoggedIn, loginModel.Email)) return BadRequest(new { message = "You are already logged in.", DateTime.Now });

                else
                {
                    await LogOut();

                    return Ok( new { message = "Before your login, the app found a session open with another account. Right now we have disconnected the previous user. Please try to log in now", DateTime.Now });
                }
            };

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, loginModel.RememberMe);

            if (!result.Succeeded)
            {
                await _userManager.AccessFailedAsync(user);

                return StatusCode(401, new { message = "Wrong email or password", DateTime.Now });
            }

            if (result.IsLockedOut) return StatusCode(401, new { message = "Your account is been locked out, try to after 1 hour or contact the administrator", DateTime.Now });

            if (result.IsNotAllowed) return StatusCode(401, new { message = "Your account is not allowed to log in anymore. Try to contact the administrator.", DateTime.Now });

            if (user.TwoFactorEnabled && user.PhoneNumberConfirmed)
            {
                await GenerateToken2FA(user);

                return Ok(new { message = $"success = {result}, We send you the Two Factory Authentication token to your e-mail or your phone cell.", DateTime.Now });
            }
            else
            {
                var roles = await UserSignIn(user, loginModel.RememberMe);

                return Ok(new { message = "success", user.Email, user.NameUser, user.LastNameUser, roles, DateTime.Now });
            }
        }

        #endregion

        #region VERIFICA 2FA

        [AllowAnonymous]
        [HttpPost]
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

            var roles = await UserSignIn(user, rememberMe);

            return Ok(new { message = "success", user.Email, user.NameUser, user.LastNameUser, roles, DateTime.Now });
        }

        #endregion

        #region LOGOUT

        [Authorize]
        [HttpGet]
        [Route("Logout")]
        public async Task<ActionResult> LogOut()
        {
            var userEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null) return NotFound();

            await _signInManager.SignOutAsync();

            return Ok(new { message = "You successfully logged out", DateTime.Now });
        }

        #endregion

        #region CONTROLLO DI SESSIONE

        [AllowAnonymous]
        [HttpGet]
        [Route("CheckAuth")]
        public async Task<IActionResult> CheckAuthentication()
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

        [Authorize]
        [HttpPost]
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

        [Authorize]
        [HttpPost]
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

        [Authorize]
        [HttpPost]
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

        #region CONFERMA EMAIL

        [AllowAnonymous]
        [HttpPost]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailInputModel confirmation)
        {
            if (!ModelState.IsValid) return StatusCode(406, new { message = "You need to provide the informations", DateTime.Now });

            var user = await _userManager.FindByEmailAsync(confirmation.Email);

            if (user == null) return NotFound( new { message = "The email you provide is not assigned to any users.", DateTime.Now });

            if (await _userManager.IsEmailConfirmedAsync(user)) return StatusCode(401, new { message = "Your email is already confirmed", DateTime.Now });

            var result = await _userManager.ConfirmEmailAsync(user, confirmation.Token);

            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            return (result.Succeeded ? Ok(new { message = $"the {_dataHelper.CriptEmail(user.Email)} is been confirmed.", DateTime.Now }) : NotFound(new { message = "The token you provide was not found.", DateTime.Now }));
        }

        #endregion

        #region CONFERMA NUMERO DI TELEFONO

        [Authorize]
        [HttpPost]
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

        [Authorize]
        [HttpPut]
        [Route("EditUser")]
        public async Task<IActionResult> EditUser([FromBody]EditUserInputModel updateUser)
        {
            if (!ModelState.IsValid) return StatusCode(406, $"The informations you inserted are not valid, {DateTime.Now}");

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) throw new SecurityException($"You are not allowed, {DateTime.Now}");

            user.NameUser = updateUser.NameUser = string.IsNullOrEmpty(updateUser.NameUser) ? user.NameUser : updateUser.NameUser;
            user.LastNameUser = updateUser.LastNameUser = string.IsNullOrEmpty(updateUser.LastNameUser) ? user.LastNameUser : updateUser.LastNameUser;
            user.UserName = updateUser.UserName = string.IsNullOrEmpty(updateUser.UserName) ? user.UserName : updateUser.UserName;
            user.TwoFactorEnabled = updateUser.TwoFactAuth;
            user.NewsSenderAggrement = updateUser.NewsSenderAgreement;

            await _userManager.SetTwoFactorEnabledAsync(user, user.TwoFactorEnabled);

            await _userManager.UpdateAsync(user);

            await _context.SaveChangesAsync();

            return Ok( new { message = "Your user has been update", DateTime.Now });
        }

        #endregion

        #region RINVIA EMAIL DI CONFERMA

        [AllowAnonymous]
        [HttpGet]
        [Route("ResendEmailConf")]
        public async Task<IActionResult> ResendEmailConfirmation(string email)
        {
            var emailController = new EmailAddressAttribute();

            if (!emailController.IsValid(email)) return StatusCode(406, new { message = "The information you provide is not an email address.", DateTime.Now } );

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return NotFound(new { message = "You need to signup firts.", DateTime.Now});

            if (user.EmailConfirmed) return StatusCode(401, new { message = "Your email is already confirmed", DateTime.Now });

            var token = HttpUtility.UrlEncode(await _userManager.GenerateEmailConfirmationTokenAsync(user));

            var confirmationLink = Url.Link("email-confirmation", new { token, email });

            ///TODO Creare un servizio che invii autonomamente l'email;
            
            return Ok(new { message = $"We send another confirmation link at {_dataHelper.CriptEmail(email)}", DateTime.Now });
        }

        #endregion

        #region MOSTRA INFO UTENTE

        [Authorize]
        [HttpGet]
        [Route("GetInfoUser")]
        public async Task<IActionResult> GetInfoUser()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) throw new SecurityException("You are not allowed " + DateTime.Now);

            var company = user.Company != null ? _context.Companies.Where(c => String.Equals(c.Users, user.Id)).FirstOrDefault() : null;

            UserViewModel userView = new UserViewModel
            {
                Email = user.Email,
                Name = user.NameUser,
                LastName = user.LastNameUser,
                NewsSenderAggrement = user.NewsSenderAggrement,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                PhoneConfirmed = user.PhoneNumberConfirmed,
                TwoFactoryEnabled = user.TwoFactorEnabled,
                NameCompany = company == null ? String.Empty : company.CompanyName,
                IvaCompany = company == null ? String.Empty : company.CompanyIva
            };

            return Ok(new { message = "success", userView, DateTime.Now });
        }

        #endregion

        #region PRIVATE ACTION - GENERATE 2 FACTORY AUTHENTICATION TOKEN AND SEND IT

        private async Task<IActionResult> GenerateToken2FA(ApplicationUser user)
        {
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

            //TODO https://stackoverflow.com/questions/43317473/how-to-implement-two-factor-auth-in-web-api-2-using-asp-net-identity

            return Ok( new { message = $"You will receive via {tokenProvider} the OTP on your" + (tokenProvider.Contains("SMS") ? " phone number " : " e-mail address") + $" {criptedCredential} ", DateTime.Now });
        }

        #endregion

        #region PRIVATE - SIGN IN USER COOKIE AUTHENTICATION

        private async Task<IList<string>> UserSignIn(ApplicationUser user, bool rememberMe)
        {
            var rolesUser = await _userManager.GetRolesAsync(user);

            await _signInManager.SignInAsync(user, rememberMe);

            return rolesUser;
        }

        #endregion

        #region INTERNAL - CHECK IF IS AN EMAIL OR A PHONE

        internal short CheckEmailOrPhone(string data)
        {
            var emailController = new EmailAddressAttribute();

            if (emailController.IsValid(data)) return 1;

            if (data.Any(x => char.IsDigit(x)) || data.Length == 10) return 2;

            return 0;
        }

        #endregion
    }
}
