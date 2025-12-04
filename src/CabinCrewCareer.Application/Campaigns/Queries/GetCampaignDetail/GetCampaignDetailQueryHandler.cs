using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CabinCrewCareer.Domain.Common;
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
                includeFunc: q => q.Include(c => c.CampaignRounds)
                    .ThenInclude(cr => cr.Rounds)
                    .Include(c => c.CampaignRequest)
                    .ThenInclude(cr => cr.AirlinePartner),
                disableTracking: true);
            var campaign = await campaignQuery.FirstOrDefaultAsync(cancellationToken);
            if (campaign == null) return null;

            var result = new GetCampaignDetailDto
            {
                CampaignId = campaign.CampaignId,
                CampaignName = campaign.CampaignName,
                Description = campaign.Description,
                JobDescription = campaign.JobDescription,
                JobRequirement = campaign.JobRequirement,
                TargetQuantity = campaign.TargetQuantity,
                CampaignType = campaign.CampaignType.ToString(),
                Status = campaign.CampaignStatus.ToString(),
                StartDate = campaign.StartDate.HasValue ? campaign.StartDate.Value.GetFormatDateTime() : null,
                EndDate = campaign.EndDate.HasValue ? campaign.EndDate.Value.GetFormatDateTime() : null,
                ApprovedAt = campaign.ApprovedAt.HasValue ? campaign.ApprovedAt.Value.GetFormatDateTime() : null,
                RejectedAt = campaign.RejectedAt.HasValue ? campaign.RejectedAt.Value.GetFormatDateTime() : null,
                ReviewedBy = campaign.ReviewedBy,
                RejectedReason = campaign.RejectedReason,
                RejectedCount = campaign.RejectedCount,
                Rounds = campaign.CampaignRounds.Select(r => {
                    var screeningRound = r.Rounds.FirstOrDefault(rd => rd.RoundType == RoundType.Screening); // Assuming 1 represents Screening Round");
                    return new RoundDto
                {
                    CampaignRoundId = r.CampaignRoundId,
                    RoundName = r.RoundName,
                    Description = r.Description,
                    TargetQuantity = r.TargetQuantity,
                    ActualQuantiy = r.ActualQuantiy,
                    Status = r.RoundStatus.ToString(),
                    StartDate = r.StartDate.HasValue ? r.StartDate.Value.GetFormatDateTime() : null,
                    EndDate = r.EndDate.HasValue ? r.EndDate.Value.GetFormatDateTime() : null,
                    ScreeningStartDate = screeningRound?.StartDate.HasValue == true ? screeningRound.StartDate.Value.GetFormatDateTime() : null,
                    ScreeningEndDate = screeningRound?.EndDate.HasValue == true ? screeningRound.EndDate.Value.GetFormatDateTime() : null
                };
                }).ToList(),
                PartnerName = campaign.CampaignRequest != null && campaign.CampaignRequest.AirlinePartner != null ? campaign.CampaignRequest.AirlinePartner.PartnerName : null,
                CreatedAt = campaign.CreatedAt.HasValue ? campaign.CreatedAt.Value.GetFormatDateTime() : null
            };
            return result;
        }
    }
}
