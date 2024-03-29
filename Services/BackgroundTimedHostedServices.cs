﻿using EmailService;
using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using IEmailSender = EmailService.IEmailSender;

namespace FGMEmailSenderApp.Services
{
    public class BackgroundTimedHostedServices : BackgroundService
    {
        private readonly ILogger<BackgroundTimedHostedServices> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider;
        private string nameServiceUserPermission = "RemoveRoleAddCompanyUser";

        public BackgroundTimedHostedServices(
            ILogger<BackgroundTimedHostedServices> logger,
            IWebHostEnvironment env,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _env = env;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Starting service in backgroung. Remove Role Add Company. {DateTime.Now}");

                await RemovePermissionAddCompanyUser();

                await Task.Delay(new TimeSpan(23, 0, 0));
            }
        }

        private async Task RemovePermissionAddCompanyUser()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>> ();
                    var usersPermissionsAddDataCompany = await scopedUserManager.GetUsersInRoleAsync(RoleHelper.AddCompanyPermissionRole);

                    if (usersPermissionsAddDataCompany.Count == 0)
                    {
                        CreateOrEditFile($"{nameServiceUserPermission}: no user found. uservar:{usersPermissionsAddDataCompany}. date:{DateTime.Now}");
                    }
                    else
                    {
                        List<ApplicationUser> usersPermissionRemove = new List<ApplicationUser>();
                        foreach (var user in usersPermissionsAddDataCompany)
                        {
                            if (user.Company == null) usersPermissionRemove.Add(user);
                        }

                        CreateOrEditFile($"{nameServiceUserPermission}. users founded: {usersPermissionRemove.Count}. date {DateTime.Now}");

                        foreach (var user in usersPermissionRemove)
                        {
                            await scopedUserManager.RemoveFromRoleAsync(user, RoleHelper.AddCompanyPermissionRole);
                        }

                        CreateOrEditFile($"{nameServiceUserPermission}. batch ended: emails notification sended to all users. date{DateTime.Now}");
                    }
                }
            }
            catch (Exception ex)
            {
                CreateOrEditFile($"{nameServiceUserPermission}. batch error! date{DateTime.Now}. Exception detail{ex.ToString()}");
            }
            finally
            {
                CreateOrEditFile($"{nameServiceUserPermission}. batch ended date{DateTime.Now}");
            }
        }

        private async Task RemovePremissionEditCompanyUser()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var usersPermissionsEditDataCompany = await scopedUserManager.GetUsersInRoleAsync(RoleHelper.EditCompanyPermissionRole);

                    if (usersPermissionsEditDataCompany.Count == 0)
                    {
                        CreateOrEditFile($"{nameServiceUserPermission}: no user found. uservar:{usersPermissionsEditDataCompany}. date:{DateTime.Now}");
                    }
                    else
                    {
                        List<ApplicationUser> usersPermissionRemove = new List<ApplicationUser>();
                        foreach (var user in usersPermissionsEditDataCompany)
                        {
                            if (user.Company == null) usersPermissionRemove.Add(user);
                        }

                        CreateOrEditFile($"{nameServiceUserPermission}. users founded: {usersPermissionRemove.Count}. date {DateTime.Now}");

                        foreach (var user in usersPermissionRemove)
                        {
                            await scopedUserManager.RemoveFromRoleAsync(user, RoleHelper.EditCompanyPermissionRole);
                        }

                        CreateOrEditFile($"{nameServiceUserPermission}. batch ended: emails notification sended to all users. date{DateTime.Now}");
                    }
                }
            }
            catch (Exception ex)
            {
                CreateOrEditFile($"{nameServiceUserPermission}. batch error! date{DateTime.Now}. Exception detail{ex.ToString()}");
            }
            finally
            {
                CreateOrEditFile($"{nameServiceUserPermission}. batch ended date{DateTime.Now}");
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
