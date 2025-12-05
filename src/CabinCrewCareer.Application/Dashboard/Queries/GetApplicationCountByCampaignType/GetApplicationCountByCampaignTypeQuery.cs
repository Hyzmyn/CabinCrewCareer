using MediatR;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetApplicationCountByCampaignType
{
    public class GetApplicationCountByCampaignTypeQuery : IRequest<List<CountByTypeDto>>
    {
    }
}
