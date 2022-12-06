using EmailService;
using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using Microsoft.AspNetCore.Identity;

namespace FGMEmailSenderApp.Services
{
    public class TimedHostedService
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _emailSender;
        private Timer _timer;
        private readonly string NameService = "ServiceRunCheckPermissionsUserAddCompany";

        public TimedHostedService(
            ILogger<TimedHostedService> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment env,
            IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _env = env;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"FGM Services start now: {DateTime.Now}");

            _timer = new Timer(RemovePermissionUser, null, TimeSpan.Zero, TimeSpan.FromHours(23));

            return Task.CompletedTask;
        }

        private async void RemovePermissionUser(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation($"Service remove permission to users have no claimed company data. {DateTime.Now}");

            try
            {
                var usersPermissionsAddDataCompany = await _userManager.GetUsersInRoleAsync(RoleHelper.AddCompanyPermissionRole);

                if(usersPermissionsAddDataCompany == null)
                {
                    CreateOrEditFile($"{NameService}: no user found. uservar:{usersPermissionsAddDataCompany}. date:{DateTime.Now}");
                }
                else
                {
                    List<ApplicationUser> usersPermissionRemove = new List<ApplicationUser>();
                    foreach(var user in usersPermissionsAddDataCompany)
                    {
                        if (user.Company == null) usersPermissionRemove.Add(user);
                    }

                    CreateOrEditFile($"{NameService}. users founded: {usersPermissionRemove.Count}. date {DateTime.Now}");

                    foreach(var user in usersPermissionRemove)
                    {
                        await _userManager.RemoveFromRoleAsync(user, RoleHelper.AddCompanyPermissionRole);
                        _emailSender.SendNotificationUserChangeRole(user.NameUser, user.LastNameUser, user.Email, null, null);
                    }

                    CreateOrEditFile($"{NameService}. batch ended: emails notification sended to all users. date{DateTime.Now}");
                }
            }
            catch(Exception ex)
            {
                CreateOrEditFile($"{NameService}. batch error! date{DateTime.Now}. Exception detail{ex.ToString()}");
            }
            finally
            {
                CreateOrEditFile($"{NameService}. batch ended date{DateTime.Now}");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FGM Services ends now.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
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
