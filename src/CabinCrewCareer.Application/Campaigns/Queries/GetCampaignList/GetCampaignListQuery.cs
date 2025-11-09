using MediatR;
using System.Collections.Generic;
using CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList;

namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList
{
    public class GetCampaignListQuery : IRequest<List<GetCampaignListDto>>
    {
    }
}
