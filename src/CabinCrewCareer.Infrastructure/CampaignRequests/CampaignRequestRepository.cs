using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CabinCrewCareer.Infrastructure.CampaignRequests
{
    public class CampaignRequestRepository(
        CabinCrewCareerDbContext dbContext,
        ILogger logger) : Repository<CampaignRequest>(dbContext, logger), ICampaignRequestRepository
    {
        public async Task<PaginatedList<CampaignRequest>> PaginatedListAsync(
            string? searchTerm,
            string? sortColumn,
            string? sortOrder,
            int? status,
            int? requestType,
            int? partnerId,
            int page,
            int pageSize)
        {
            IQueryable<CampaignRequest> requestsQuery = _dbContext.CampaignRequests
                .Include(cr => cr.AirlinePartner)
                .Include(cr => cr.Director);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                requestsQuery = requestsQuery.Where(cr =>
                    cr.CampaignName.Contains(searchTerm));
            }

            if (status != null)
            {
                requestsQuery = requestsQuery.Where(cr => cr.Status == (RequestStatus)status);
            }

            if (requestType != null)
            {
                requestsQuery = requestsQuery.Where(cr => cr.RequestType == (CampaignType)requestType);
            }

            if (partnerId != null)
            {
                requestsQuery = requestsQuery.Where(cr => cr.PartnerId == partnerId);
            }

            if (sortOrder?.ToLower() == "asc")
            {
                requestsQuery = requestsQuery.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                requestsQuery = requestsQuery.OrderByDescending(GetSortProperty(sortColumn));
            }

            var requests = await PaginatedList<CampaignRequest>.CreateAsync(requestsQuery, page, pageSize);

            return requests;
        }

        private static Expression<Func<CampaignRequest, object>> GetSortProperty(string? sortColumn)
            => sortColumn?.ToLower() switch
            {
                "name" => request => request.CampaignName,
                "createdat" => request => request.CreatedAt ?? DateTime.MinValue,
                "status" => request => request.Status,
                _ => request => request.RequestId
            };
    }
}
