using MediatR;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetCampaignCountByAirlinePartner
{
    public class GetCampaignCountByAirlinePartnerQuery : IRequest<List<CountByTypeDto>>
    {
    }
}
