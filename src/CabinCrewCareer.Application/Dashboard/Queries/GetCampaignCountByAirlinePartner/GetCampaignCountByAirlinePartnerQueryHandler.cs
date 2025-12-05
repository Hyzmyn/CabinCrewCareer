using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;
using System.Linq;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetCampaignCountByAirlinePartner
{
    public class GetCampaignCountByAirlinePartnerQueryHandler : IRequestHandler<GetCampaignCountByAirlinePartnerQuery, List<CountByTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetCampaignCountByAirlinePartnerQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<CountByTypeDto>> Handle(GetCampaignCountByAirlinePartnerQuery request, CancellationToken cancellationToken)
        {
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync();
            var result = System.Enum.GetValues(typeof(CabinCrewCareer.Domain.Common.AvailableAirlinePartner))
                .Cast<CabinCrewCareer.Domain.Common.AvailableAirlinePartner>()
                .Select(type => new CountByTypeDto
                {
                    Type = type.ToString(),
                    Count = campaigns.Count(x => x.CampaignRequest != null && x.CampaignRequest.AirlinePartner != null && x.CampaignRequest.AirlinePartner.PartnerId == (int)type)
                })
                .ToList();
            return result;
        }
    }
}
