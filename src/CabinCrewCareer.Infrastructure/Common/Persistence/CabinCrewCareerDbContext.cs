using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Infrastructure.Common.Persistence
{
    public class CabinCrewCareerDbContext : DbContext
    {
        private readonly AuditableEntitiesInterceptor _auditableEntitiesInterceptor;

        //public TeacherAIToolsDbContext()
        //{
        //}

        public CabinCrewCareerDbContext(
            DbContextOptions<CabinCrewCareerDbContext> options,
            AuditableEntitiesInterceptor auditableEntitiesInterceptor)
        : base(options)
        {
            _auditableEntitiesInterceptor = auditableEntitiesInterceptor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_auditableEntitiesInterceptor);
        }
    }
}
