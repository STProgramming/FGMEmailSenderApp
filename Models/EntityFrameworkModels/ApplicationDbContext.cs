using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FGMEmailSenderApp.Models.EntityFrameworkModels
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Cargo> Cargos { get; set; }
        public DbSet<CargoEvent> CargoEvents { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<StatusCargo> statusCargos { get; set; }
    }
}
