using AutoMapper;
using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.CampaignRequests.Queries.GetCampaignRequests
{
    public class GetCampaignRequestsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetCampaignRequestsQuery, Response<PaginatedList<GetCampaignRequestResponse>>>
    {
        public async Task<Response<PaginatedList<GetCampaignRequestResponse>>> Handle(
            GetCampaignRequestsQuery request,
            CancellationToken cancellationToken)
        {
            // Get both role names from database in a single query
            var rolesQuery = await unitOfWork.Roles.GetAsync(r => r.RoleId == 6 || r.RoleId == 9);
            var rolesList = await rolesQuery.ToListAsync(cancellationToken);
            var roles = rolesList.ToDictionary(r => r.RoleId, r => r.RoleName);
            
            var cabinCrewRoleName = roles.GetValueOrDefault(6, string.Empty);
            var purserRoleName = roles.GetValueOrDefault(9, string.Empty);

            var campaignRequests = await unitOfWork.CampaignRequests.PaginatedListAsync(
                request.SearchTerm,
                request.SortColumn,
                request.SortOrder,
                request.Status,
                request.RequestType,
                request.PartnerId,
                request.Page,
                request.PageSize);

            var mappedRequests = mapper.Map<PaginatedList<GetCampaignRequestResponse>>(campaignRequests);

            // Set Position based on RequestType and actual Role names
            foreach (var campaignRequest in mappedRequests.Items)
            {
                campaignRequest.Position = campaignRequest.RequestType == "Recruitment" ? cabinCrewRoleName : purserRoleName;
            }

            return new Response<PaginatedList<GetCampaignRequestResponse>>(code: (int)ResponseCode.SUCCESS, data: mappedRequests, message: ResponseCode.SUCCESS.ToString());
        }
    }
}
