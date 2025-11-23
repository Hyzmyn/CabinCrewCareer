namespace CabinCrewCareer.Infrastructure.UserApplications
{
    using CabinCrewCareer.Application.Common.Interfaces.Persistence;
    using CabinCrewCareer.Domain.Entities;
    using CabinCrewCareer.Domain.Common;
    using CabinCrewCareer.Infrastructure.Common.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class UserApplicationRepository : IUserApplicationRepository
    {
        private readonly CabinCrewCareerDbContext _dbContext;
        private readonly ILogger _logger;

        public UserApplicationRepository(CabinCrewCareerDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<UserApplication> CreateUserApplicationAsync(UserApplication application, List<Document> documents)
        {
            // Gắn documents vào application
            application.Documents = documents;
            _dbContext.UserApplications.Add(application);
            await _dbContext.SaveChangesAsync();
            return application;
        }

        public async Task<UserApplication?> GetUserApplicationByIdAsync(int id)
        {
            return await _dbContext.UserApplications
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.ApplicationId == id);
        }

        public async Task<UserApplication?> GetUserApplicationByUserIdAsync(int userId)
        {
            return await _dbContext.UserApplications
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<bool> UpdateApplicationStatusAsync(int id, ApplicationStatus status)
        {
            var application = await _dbContext.UserApplications.FirstOrDefaultAsync(x => x.ApplicationId == id);
            if (application == null)
                return false;
            application.ApplicationStatus = status;
            if (status == ApplicationStatus.Submitted)
                application.SubmissionDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
