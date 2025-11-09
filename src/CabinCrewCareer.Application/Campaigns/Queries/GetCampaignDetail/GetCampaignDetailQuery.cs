using MediatR;
using CabinCrewCareer.Application.Campaigns.Queries.GetCampaignDetail;

namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignDetail
{
    public class GetCampaignDetailQuery : IRequest<GetCampaignDetailDto>
    {
        public int CampaignId { get; set; }
        public GetCampaignDetailQuery(int campaignId)
        {
            CampaignId = campaignId;
        }
    }
}
