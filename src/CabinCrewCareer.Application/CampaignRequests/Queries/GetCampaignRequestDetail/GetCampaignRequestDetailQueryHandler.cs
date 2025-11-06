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

            return new Response<GetCampaignRequestResponse>(mappedRequest, "Get campaign request detail successfully");
        }
    }
}
