using MediatR;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Dashboard.Common;
using System.Collections.Generic;
using System.Linq;

namespace CabinCrewCareer.Application.Dashboard.Queries.GetApplicationCountByCampaignType
{
    public class GetApplicationCountByCampaignTypeQueryHandler : IRequestHandler<GetApplicationCountByCampaignTypeQuery, List<CountByTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetApplicationCountByCampaignTypeQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<CountByTypeDto>> Handle(GetApplicationCountByCampaignTypeQuery request, CancellationToken cancellationToken)
        {
            var snapshots = await _unitOfWork.UserApplicationSnapshots.GetAllAsync();
            var roundActivities = await _unitOfWork.RoundActivites.GetAllAsync();
            var rounds = await _unitOfWork.Rounds.GetAllAsync();
            var campaignRounds = await _unitOfWork.CampaignRounds.GetAllAsync();
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync();

            var joined = from ra in roundActivities
                         join snap in snapshots on ra.ApplicationSnapshotId equals snap.ApplicationSnapshotId
                         join r in rounds on ra.RoundId equals r.RoundId
                         join cr in campaignRounds on r.CampaignRoundId equals cr.CampaignRoundId
                         join c in campaigns on cr.CampaignId equals c.CampaignId
                         select new { snap, c };

            var result = System.Enum.GetValues(typeof(CabinCrewCareer.Domain.Common.CampaignType))
                .Cast<CabinCrewCareer.Domain.Common.CampaignType>()
                .Select(type => new CountByTypeDto
                {
                    Type = type.ToString(),
                    Count = joined.Count(x => x.c.CampaignType == type)
                })
                .ToList();
            return result;
        }
    }
}
