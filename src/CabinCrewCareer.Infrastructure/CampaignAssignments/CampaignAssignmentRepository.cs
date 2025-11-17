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

        /// <summary>
        /// Đếm số campaigns đang ở trạng thái "Ongoing" mà user đang được assign
        /// </summary>
        /// <param name="userId">ID của user cần kiểm tra</param>
        /// <returns>Số lượng campaigns ongoing</returns>
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

        /// <summary>
        /// Kiểm tra xem user có campaign nào overlap về thời gian với campaign mới không
        /// Logic: Chỉ cho phép 1 campaign tại một thời điểm, không được overlap thời gian
        /// </summary>
        /// <param name="userId">ID của user cần kiểm tra</param>
        /// <param name="campaignId">ID của campaign mới muốn assign</param>
        /// <returns>True nếu có overlap, False nếu không overlap</returns>
        public async Task<bool> HasOverlappingCampaignAsync(int userId, int campaignId)
        {
            // Lấy thông tin campaign mới
            var newCampaign = await _dbContext.Campaigns
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId);
                
            if (newCampaign == null) return false;

            // Kiểm tra xem user có campaign nào overlap về thời gian không
            var overlappingAssignments = await _dbContext.CampaignAssignments
                .Where(a => a.UserId == userId && a.CampaignId != campaignId)
                .Include(a => a.Campaign)
                .Where(a => a.Campaign != null && a.Campaign.IsActive &&
                           // Chỉ kiểm tra overlap thời gian
                           (newCampaign.StartDate < a.Campaign.EndDate && newCampaign.EndDate > a.Campaign.StartDate))
                .AnyAsync();

            return overlappingAssignments;
        }
    }
}