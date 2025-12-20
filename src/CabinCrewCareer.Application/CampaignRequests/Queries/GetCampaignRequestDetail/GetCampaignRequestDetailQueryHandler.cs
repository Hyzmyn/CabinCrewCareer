using AutoMapper;
using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.CampaignRequests.Queries.GetCampaignRequestDetail
{
    public class GetCampaignRequestDetailQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetCampaignRequestDetailQuery, Response<GetCampaignRequestResponse>>
    {
        public async Task<Response<GetCampaignRequestResponse>> Handle(
            GetCampaignRequestDetailQuery request,
            CancellationToken cancellationToken)
        {
            // Get both role names from database in a single query
            var rolesQuery = await unitOfWork.Roles.GetAsync(r => r.RoleId == 6 || r.RoleId == 9);
            var rolesList = await rolesQuery.ToListAsync(cancellationToken);
            var roles = rolesList.ToDictionary(r => r.RoleId, r => r.RoleName);
            
            var cabinCrewRoleName = roles.GetValueOrDefault(6, string.Empty);
            var purserRoleName = roles.GetValueOrDefault(9, string.Empty);

            var campaignRequestQuery = await unitOfWork.CampaignRequests
                .GetAsync(cr => cr.RequestId == request.RequestId);

            var campaignRequest = await campaignRequestQuery
                .Include(cr => cr.AirlinePartner)
                .Include(cr => cr.Director)
                .FirstOrDefaultAsync(cancellationToken);

            if (campaignRequest == null)
            {
                throw new ApiException(ResponseCode.CAMPAIGN_REQUEST_NOT_FOUND);
            }

            var mappedRequest = mapper.Map<GetCampaignRequestResponse>(campaignRequest);
            
            // Set Position based on RequestType and actual Role names
            mappedRequest.Position = mappedRequest.RequestType == "Recruitment" ? cabinCrewRoleName : purserRoleName;

            return new Response<GetCampaignRequestResponse>(code: (int)ResponseCode.SUCCESS, data: mappedRequest, message: ResponseCode.SUCCESS.ToString());
        }
    }
}
