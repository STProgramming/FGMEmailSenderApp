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
        public DbSet<Request> Requests { get; set; }
        public DbSet<StatusCargo> StatusCargos { get; set; }
        public DbSet<TypeRequest> TypesRequest { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>()
            .HasOne(a => a.Company)
            .WithOne(a => a.User)
            .HasForeignKey<Company>(c => c.IdUser);
        }
    }
}
