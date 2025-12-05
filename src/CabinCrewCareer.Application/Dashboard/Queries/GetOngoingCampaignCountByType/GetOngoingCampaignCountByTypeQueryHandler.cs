using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;
using System.Linq;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetOngoingCampaignCountByType
{
    public class GetOngoingCampaignCountByTypeQueryHandler : IRequestHandler<GetOngoingCampaignCountByTypeQuery, List<CountByTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetOngoingCampaignCountByTypeQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<CountByTypeDto>> Handle(GetOngoingCampaignCountByTypeQuery request, CancellationToken cancellationToken)
        {
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync();
            var result = System.Enum.GetValues(typeof(CabinCrewCareer.Domain.Common.CampaignType))
                .Cast<CabinCrewCareer.Domain.Common.CampaignType>()
                .Select(type => new CountByTypeDto
                {
                    Type = type.ToString(),
                    Count = campaigns.Count(x => x.CampaignStatus == CabinCrewCareer.Domain.Common.CampaignStatus.Ongoing && x.CampaignType == type)
                })
                .ToList();
            return result;
        }
    }
}
