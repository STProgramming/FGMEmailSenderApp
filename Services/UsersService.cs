using EmailService;
using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FGMEmailSenderApp.Services
{
    public class UsersService : IUsersService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _emailSender;
        private readonly string NameServiceUserCompany = "RemoveRoleAddCompanyUser";

        public UsersService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment env,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
            _emailSender = emailSender;
        }

        public async Task RemoveRoleAddCompanyUser()
        {
            try
            {
                var usersPermissionsAddDataCompany = await _userManager.GetUsersInRoleAsync(RoleHelper.AddCompanyPermissionRole);

                if (usersPermissionsAddDataCompany == null)
                {
                    CreateOrEditFile($"{NameServiceUserCompany}: no user found. uservar:{usersPermissionsAddDataCompany}. date:{DateTime.Now}");
                }
                else
                {
                    List<ApplicationUser> usersPermissionRemove = new List<ApplicationUser>();
                    foreach (var user in usersPermissionsAddDataCompany)
                    {
                        if (user.Company == null) usersPermissionRemove.Add(user);
                    }

                    CreateOrEditFile($"{NameServiceUserCompany}. users founded: {usersPermissionRemove.Count}. date {DateTime.Now}");

                    foreach (var user in usersPermissionRemove)
                    {
                        await _userManager.RemoveFromRoleAsync(user, RoleHelper.AddCompanyPermissionRole);
                        _emailSender.SendNotificationUserChangeRole(user.NameUser, user.LastNameUser, user.Email, null, null);
                    }

                    CreateOrEditFile($"{NameServiceUserCompany}. batch ended: emails notification sended to all users. date{DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                CreateOrEditFile($"{NameServiceUserCompany}. batch error! date{DateTime.Now}. Exception detail{ex.ToString()}");
            }
            finally
            {
                CreateOrEditFile($"{NameServiceUserCompany}. batch ended date{DateTime.Now}");
            }
        }

        private async void CreateOrEditFile(string statusContent)
        {
            string pathFile = "\\log\\logService.txt";

            string pathDirectory = _env.ContentRootPath;

            var pathComplete = pathDirectory + pathFile;

            using (var fileController = new StreamWriter(pathComplete, true))
            {
                await fileController.WriteLineAsync(statusContent);
            }
        }
    }
}
