using EmailService;
using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FGMEmailSenderApp.Services
{
    public class BackgroundTimedHostedServices : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<BackgroundTimedHostedServices> _logger;
        private Timer _timer;
        private readonly IUsersService _usersService;

        public BackgroundTimedHostedServices(
            ILogger<BackgroundTimedHostedServices> logger,
            IUsersService usersService)
        {
            _logger = logger;
            _usersService = usersService;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"FGM Services start now: {DateTime.Now}");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(23));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation($"Service remove permission to users have no claimed company data. {DateTime.Now}");

            await _usersService.RemoveRoleAddCompanyUser();
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
    }
}
