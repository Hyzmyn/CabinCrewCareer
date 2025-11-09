using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignDetail
{
    public class GetCampaignDetailQueryHandler : IRequestHandler<GetCampaignDetailQuery, GetCampaignDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetCampaignDetailQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetCampaignDetailDto> Handle(GetCampaignDetailQuery request, CancellationToken cancellationToken)
        {
            var campaignQuery = await _unitOfWork.Campaigns.GetAsync(
                c => c.CampaignId == request.CampaignId,
                includeFunc: q => q.Include(c => c.CampaignRounds),
                disableTracking: true);
            var campaign = await campaignQuery.FirstOrDefaultAsync(cancellationToken);
            if (campaign == null) return null;

            var result = new GetCampaignDetailDto
            {
                CampaignId = campaign.CampaignId,
                CampaignName = campaign.CampaignName,
                Description = campaign.Description,
                TargetQuantity = campaign.TargetQuantity,
                CampaignType = campaign.CampaignType.ToString(),
                Status = campaign.CampaignStatus.ToString(),
                StartDate = campaign.StartDate.HasValue ? campaign.StartDate.Value.GetFormatDateTime() : null,
                EndDate = campaign.EndDate.HasValue ? campaign.EndDate.Value.GetFormatDateTime() : null,
                Rounds = campaign.CampaignRounds.Select(r => new RoundDto
                {
                    CampaignRoundId = r.CampaignRoundId,
                    RoundName = r.RoundName,
                    Description = r.Description,
                    TargetQuantity = r.TargetQuantity,
                    ActualQuantiy = r.ActualQuantiy,
                    Status = r.RoundStatus.ToString(),
                    StartDate = r.StartDate.HasValue ? r.StartDate.Value.GetFormatDateTime() : null,
                    EndDate = r.EndDate.HasValue ? r.EndDate.Value.GetFormatDateTime() : null
                }).ToList()
            };
            return result;
        }
    }
}
