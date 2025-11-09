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
                .Select(c => new GetCampaignListDto
                {
                    CampaignId = c.CampaignId,
                    CampaignName = c.CampaignName,
                    Description = c.Description,
                    TargetQuantity = c.TargetQuantity,
                    CampaignType = c.CampaignType.ToString(),
                    Status = c.CampaignStatus.ToString(),
                    StartDate = c.StartDate.HasValue ? c.StartDate.Value.GetFormatDateTime() : null,
                    EndDate = c.EndDate.HasValue ? c.EndDate.Value.GetFormatDateTime() : null
                })
                .ToListAsync(cancellationToken);
            return result;
        }
    }
}
