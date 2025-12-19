using CabinCrewCareer.Application.CampaignAssignments.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.CampaignAssignments.Queries.GetMyCampaigns
{
    public class GetMyCampaignsQueryHandler 
        : IRequestHandler<GetMyCampaignsQuery, Response<List<SimpleCampaignDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMyCampaignsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<List<SimpleCampaignDto>>> Handle(
            GetMyCampaignsQuery request, 
            CancellationToken cancellationToken)
        {
            // Get both role names from database in a single query
            var rolesQuery = await _unitOfWork.Roles.GetAsync(r => r.RoleId == 6 || r.RoleId == 9);
            var rolesList = await rolesQuery.ToListAsync(cancellationToken);
            var roles = rolesList.ToDictionary(r => r.RoleId, r => r.RoleName);
            
            var cabinCrewRoleName = roles.GetValueOrDefault(6, string.Empty);
            var purserRoleName = roles.GetValueOrDefault(9, string.Empty);

            // Lấy userId từ token - không cần phân biệt role
            var currentUserIdString = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out var currentUserId))
            {
                return new Response<List<SimpleCampaignDto>>(
                    code: (int)ResponseCode.FAILED_AUTHENTICATION,
                    data: [],
                    message: "Không xác định được người dùng hiện tại");
            }

            // Lấy tất cả assignments của user (Recruiter hoặc Examiner)
            var assignments = await _unitOfWork.CampaignAssignments
                .GetAssignmentsByUserIdAsync(currentUserId);

            if (!assignments.Any())
            {
                return new Response<List<SimpleCampaignDto>>(
                    code: (int)ResponseCode.SUCCESS,
                    data: [],
                    message: "Bạn chưa được giao việc nào");
            }

            // Group assignments theo campaign và chỉ lấy thông tin campaign
            var campaignGroups = assignments.GroupBy(a => a.CampaignId);
            var result = new List<SimpleCampaignDto>();

            foreach (var group in campaignGroups)
            {
                var campaignId = group.Key;
                if (campaignId == null) continue;

                // Include CampaignRequest và AirlinePartner khi lấy campaign
                var campaignQuery = await _unitOfWork.Campaigns.GetAsync(
                    c => c.CampaignId == campaignId.Value,
                    includeFunc: q => q
                        .Include(c => c.CampaignRequest)
                        .ThenInclude(cr => cr.AirlinePartner),
                    disableTracking: true);
                var campaign = await campaignQuery.FirstOrDefaultAsync(cancellationToken);
                if (campaign == null) continue;

                string partnerName = string.Empty;
                if (campaign.CampaignRequest != null && campaign.CampaignRequest.AirlinePartner != null)
                {
                    partnerName = campaign.CampaignRequest.AirlinePartner.PartnerName;
                }

                var campaignDto = new SimpleCampaignDto
                {
                    CampaignId = campaign.CampaignId,
                    CampaignName = campaign.CampaignName,
                    Description = campaign.Description,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    PartnerName = partnerName,
                    CampaignType = campaign.CampaignType.ToString(),
                    Position = campaign.CampaignType == Domain.Common.CampaignType.Recruitment ? cabinCrewRoleName : purserRoleName
                };

                result.Add(campaignDto);
            }

            return new Response<List<SimpleCampaignDto>>(
                code: (int)ResponseCode.SUCCESS,
                data: result,
                message: "Lấy danh sách campaign thành công");
        }
    }
}