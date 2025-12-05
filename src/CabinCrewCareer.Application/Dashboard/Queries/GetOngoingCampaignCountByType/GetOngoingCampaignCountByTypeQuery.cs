using MediatR;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetOngoingCampaignCountByType
{
    public class GetOngoingCampaignCountByTypeQuery : IRequest<List<CountByTypeDto>>
    {
    }
}
