using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

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

        public async Task<PaginatedList<CampaignAssignment>> PaginatedListAsync(string? searchTerm, string? sortColumn, string? sortOrder, int? campaignType, int? campaignStatus, int? partnerId, int page, int pageSize, int userId)
        {
            IQueryable<CampaignAssignment> usersQuery = _dbContext.CampaignAssignments
                .Where(x => x.UserId == userId)
                .Select(x => new CampaignAssignment
                {
                    AssignmentId = x.AssignmentId,
                    Campaign = new Campaign
                    {
                        CampaignId = x.Campaign.CampaignId,
                        CampaignName = x.Campaign.CampaignName,
                        Description = x.Campaign.Description,
                        StartDate = x.Campaign.StartDate,
                        EndDate = x.Campaign.EndDate,
                        CampaignType = x.Campaign.CampaignType,
                        CampaignStatus = x.Campaign.CampaignStatus,
                        CampaignRequest = new CampaignRequest
                        {
                            AirlinePartner = new AirlinePartner
                            {
                                PartnerName = x.Campaign.CampaignRequest.AirlinePartner.PartnerName
                            }
                        }
                    }
                });

            //switch (isActive)
            //{
            //    case 1:
            //        usersQuery = usersQuery.Where(u => u.IsActive);
            //        break;
            //    case 0:
            //        usersQuery = usersQuery.Where(u => !u.IsActive);
            //        break;
            //    default: break;
            //}

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                usersQuery = usersQuery.Where(c =>
                    c.Campaign.CampaignName.Contains(searchTerm));
            }

            if (campaignType != null)
            {
                usersQuery = usersQuery.Where(c => c.Campaign.CampaignType == (CampaignType)campaignType);
            }

            if (campaignStatus != null)
            {
                usersQuery = usersQuery.Where(c => c.Campaign.CampaignStatus == (CampaignStatus)campaignStatus);
            }

            if (partnerId != null)
            {
                usersQuery = usersQuery.Where(c => c.Campaign.CampaignRequest.PartnerId == partnerId);
            }

            if (sortOrder?.ToLower() == "asc")
            {
                usersQuery = usersQuery.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                usersQuery = usersQuery.OrderByDescending(GetSortProperty(sortColumn));
            }

            var users = await PaginatedList<CampaignAssignment>.CreateAsync(usersQuery, page, pageSize);

            return users;
        }

        private static Expression<Func<CampaignAssignment, object>> GetSortProperty(string? sortColumn)
       => sortColumn?.ToLower() switch
       {
           "name" => user => user.Campaign.CampaignName,
           //"dob" => user => user.DoB,
           _ => user => user.Campaign.CampaignId
       };
    }
}