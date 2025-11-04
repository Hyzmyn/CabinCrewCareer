using CabinCrewCareer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Infrastructure.Common.Persistence
{
    public class CabinCrewCareerDbContext(
        DbContextOptions<CabinCrewCareerDbContext> options,
        AuditableEntitiesInterceptor auditableEntitiesInterceptor) : DbContext(options)
    {
        private readonly AuditableEntitiesInterceptor _auditableEntitiesInterceptor = auditableEntitiesInterceptor;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_auditableEntitiesInterceptor);
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<AirlinePartner> AirlineParters { get; set; }

        public DbSet<Ward> Wards { get; set; }

        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CabinCrewCareerDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
