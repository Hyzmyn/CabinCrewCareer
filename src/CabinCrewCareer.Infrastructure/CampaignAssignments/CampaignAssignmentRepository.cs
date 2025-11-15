using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Infrastructure.CampaignAssignments
{
    public class CampaignAssignmentRepository : Repository<CampaignAssignment>, ICampaignAssignmentRepository
    {
        public CampaignAssignmentRepository(CabinCrewCareerDbContext dbContext, ILogger logger) 
            : base(dbContext, logger)
        {
        }

        public async Task<List<CampaignAssignment>> GetAssignmentsByUserIdAsync(int userId)
        {
            return await _dbContext.CampaignAssignments
                .Where(a => a.UserId == userId)
                .Include(a => a.Campaign)
                .Include(a => a.User)
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();
        }

        public async Task<List<CampaignAssignment>> GetAssignmentsByCampaignIdAsync(int campaignId)
        {
            return await _dbContext.CampaignAssignments
                .Where(a => a.CampaignId == campaignId)
                .Include(a => a.Campaign)
                .Include(a => a.User)
                .OrderBy(a => a.Task)
                .ToListAsync();
        }

        public async Task<bool> HasAssignmentsForCampaignAsync(int campaignId)
        {
            return await _dbContext.CampaignAssignments
                .AnyAsync(a => a.CampaignId == campaignId);
        }

        public async Task<int> GetOngoingCampaignsCountByUserIdAsync(int userId)
        {
            // Đếm số campaigns có status "Ongoing" mà user này đang được assign
            return await _dbContext.CampaignAssignments
                .Where(a => a.UserId == userId)
                .Include(a => a.Campaign)
                .Where(a => a.Campaign != null && a.Campaign.CampaignStatus == CampaignStatus.Ongoing)
                .Select(a => a.CampaignId)
                .Distinct()
                .CountAsync();
        }
    }
}