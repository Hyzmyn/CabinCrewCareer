using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList
{
    public class GetCampaignListQuery : IRequest<List<GetCampaignListDto>>
    {
    }
}
