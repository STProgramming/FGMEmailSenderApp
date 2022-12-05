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
        private Timer _timer;

        public TimedHostedService(
            ILogger<TimedHostedService> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }
    }
}
