using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.UserApplications
{
    public class UserApplicationSnapshotRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<UserApplicationSnapshot>(dbContext, logger), IUserApplicationSnapshotRepository
    {
        public async Task<UserApplicationSnapshot?> GetUserApplicationSnapshotByIdAsync(int id)
        {
            return await _dbContext.UserApplicationSnapshots
                .Include(x => x.DocumentSnapshots)
                .FirstOrDefaultAsync(x => x.ApplicationSnapshotId == id);
        }

        public async Task<bool> UpdateApplicationStatusAsync(int id, ApplicationStatus status)
        {
            var application = await _dbContext.UserApplicationSnapshots.FirstOrDefaultAsync(x => x.ApplicationSnapshotId == id);
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
