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

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<NotificationType> NotificationTypes { get; set; }

        public DbSet<CampaignRequest> CampaignRequests { get; set; }

        public DbSet<Campaign> Campaigns { get; set; }

        public DbSet<CampaignRound> CampaignRounds { get; set; }

        public DbSet<Round> Rounds { get; set; }

        public DbSet<UserApplication> UserApplications { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<Test> Tests { get; set; }

        public DbSet<TestQuestion> TestQuesions { get; set; }

        public DbSet<TestOption> TestOptions { get; set; }

        public DbSet<TestSession> TestSessions { get; set; }

        public DbSet<TestAnswer> TestAnswers { get; set; }

        public DbSet<TestResult> TestResults { get; set; }

        public DbSet<EnquiryRequest> EnquiryRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CabinCrewCareerDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
