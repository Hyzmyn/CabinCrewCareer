using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRequests.Queries.GetCampaignRequestDetail
{
    public class GetCampaignRequestDetailQuery : IRequest<Response<GetCampaignRequestResponse>>
    {
        public int RequestId { get; set; }

        public GetCampaignRequestDetailQuery(int requestId)
        {
            RequestId = requestId;
        }
    }
}
