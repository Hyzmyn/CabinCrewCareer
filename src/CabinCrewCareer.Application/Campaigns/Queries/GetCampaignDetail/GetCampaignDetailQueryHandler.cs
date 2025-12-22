using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace CabinCrewCareer.Application.Campaigns.Queries.GetCampaignDetail
{
    public class GetCampaignDetailQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<GetCampaignDetailQuery, GetCampaignDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<GetCampaignDetailDto> Handle(GetCampaignDetailQuery request, CancellationToken cancellationToken)
        {
            int userId = 0;

            var user = _currentUserService.CurrentPrincipal;

            if (user is not null) userId = Int32.Parse(user);

            // Get both role names from database in a single query
            var rolesQuery = await _unitOfWork.Roles.GetAsync(r => r.RoleId == 6 || r.RoleId == 9);
            var rolesList = await rolesQuery.ToListAsync(cancellationToken);
            var roles = rolesList.ToDictionary(r => r.RoleId, r => r.RoleName);

            var cabinCrewRoleName = roles.GetValueOrDefault(6, string.Empty);
            var purserRoleName = roles.GetValueOrDefault(9, string.Empty);

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
                Position = campaign.CampaignType == Domain.Common.CampaignType.Recruitment ? cabinCrewRoleName : purserRoleName,
                StartDate = campaign.StartDate.HasValue ? campaign.StartDate.Value.GetFormatDateTime() : null,
                EndDate = campaign.EndDate.HasValue ? campaign.EndDate.Value.GetFormatDateTime() : null,
                DueDate = campaign.CampaignRequest?.DueDate.HasValue == true ? campaign.CampaignRequest.DueDate.Value.ToString("dd/MM/yyyy") : null,
                ApprovedAt = campaign.ApprovedAt.HasValue ? campaign.ApprovedAt.Value.GetFormatDateTime() : null,
                RejectedAt = campaign.RejectedAt.HasValue ? campaign.RejectedAt.Value.GetFormatDateTime() : null,
                ReviewedBy = campaign.ReviewedBy,
                RejectedReason = campaign.RejectedReason,
                RejectedCount = campaign.RejectedCount,
                Rounds = campaign.CampaignRounds.Select(r =>
                {
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

            foreach (var item in result.Rounds)
            {
                item.HasApplied = await _unitOfWork.UserHistories.HasAppliedToCampainRound(userId, item.CampaignRoundId);
            }

            return result;
        }
    }
}
