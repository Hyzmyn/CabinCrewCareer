using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList
{
    public class GetCampaignListQueryHandler : IRequestHandler<GetCampaignListQuery, List<GetCampaignListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetCampaignListQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<GetCampaignListDto>> Handle(GetCampaignListQuery request, CancellationToken cancellationToken)
        {
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync(disableTracking: true);
            var result = await campaigns
                .Include(c => c.CampaignRequest)
                .ThenInclude(cr => cr.AirlinePartner)
                .Select(c => new GetCampaignListDto
                {
                    CampaignId = c.CampaignId,
                    CampaignName = c.CampaignName,
                    Description = c.Description,
                    TargetQuantity = c.TargetQuantity,
                    CampaignType = c.CampaignType.ToString(),
                    Status = c.CampaignStatus.ToString(),
                    StartDate = c.StartDate.HasValue ? c.StartDate.Value.GetFormatDateTime() : null,
                    EndDate = c.EndDate.HasValue ? c.EndDate.Value.GetFormatDateTime() : null,
                    ApprovedAt = c.ApprovedAt.HasValue ? c.ApprovedAt.Value.GetFormatDateTime() : null,
                    RejectedAt = c.RejectedAt.HasValue ? c.RejectedAt.Value.GetFormatDateTime() : null,
                    ReviewedBy = c.ReviewedBy,
                    RejectedReason = c.RejectedReason,
                    RejectedCount = c.RejectedCount,
                    PartnerName = c.CampaignRequest != null && c.CampaignRequest.AirlinePartner != null ? c.CampaignRequest.AirlinePartner.PartnerName : null
                })
                .ToListAsync(cancellationToken);
            return result;
        }
    }
}
