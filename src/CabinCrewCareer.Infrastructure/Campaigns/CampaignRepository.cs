using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CabinCrewCareer.Infrastructure.Campaigns
{
    public class CampaignRepository(CabinCrewCareerDbContext dbContext, ILogger logger) : Repository<Campaign>(dbContext, logger), ICampaignRepository
    {
        public async Task<PaginatedList<Campaign>> PaginatedListAsync(string? searchTerm, string? sortColumn, string? sortOrder, int? campaignType, int? campaignStatus, int? partnerId, int page, int pageSize)
        {
            IQueryable<Campaign> usersQuery = _dbContext.Campaigns
                .Include(c => c.CampaignRequest)
                .ThenInclude(cr => cr.AirlinePartner);

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
                    c.CampaignName.Contains(searchTerm));
            }

            if (campaignType != null)
            {
                usersQuery = usersQuery.Where(c => c.CampaignType == (CampaignType)campaignType);
            }

            if (campaignStatus != null)
            {
                usersQuery = usersQuery.Where(c => c.CampaignStatus == (CampaignStatus)campaignStatus);
            }

            if (partnerId != null)
            {
                usersQuery = usersQuery.Where(c => c.CampaignRequest.PartnerId == partnerId);
            }

            if (sortOrder?.ToLower() == "asc")
            {
                usersQuery = usersQuery.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                usersQuery = usersQuery.OrderByDescending(GetSortProperty(sortColumn));
            }

            var users = await PaginatedList<Campaign>.CreateAsync(usersQuery, page, pageSize);

            return users;
        }

        private static Expression<Func<Campaign, object>> GetSortProperty(string? sortColumn)
       => sortColumn?.ToLower() switch
       {
           "name" => user => user.CampaignName,
           //"dob" => user => user.DoB,
           _ => user => user.CampaignId
       };
    }
}
