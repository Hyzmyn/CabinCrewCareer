using CabinCrewCareer.Application.CampaignAssignments.Common;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignAssignments.Queries.GetMyTasks
{
    public class GetMyTasksQueryHandler 
        : IRequestHandler<GetMyTasksQuery, Response<List<AssignedCampaignDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMyTasksQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<List<AssignedCampaignDto>>> Handle(
            GetMyTasksQuery request, 
            CancellationToken cancellationToken)
        {
            // Lấy userId từ token - không cần phân biệt role
            var currentUserIdString = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserIdString) || !int.TryParse(currentUserIdString, out var currentUserId))
            {
                return new Response<List<AssignedCampaignDto>>
                {
                    Code = 401,
                    Message = "Không xác định được người dùng hiện tại",
                    Data = []
                };
            }

            // Lấy tất cả assignments của user (Recruiter hoặc Examiner)
            var assignments = await _unitOfWork.CampaignAssignments
                .GetAssignmentsByUserIdAsync(currentUserId);

            if (!assignments.Any())
            {
                return new Response<List<AssignedCampaignDto>>
                {
                    Code = 200,
                    Message = "Bạn chưa được giao việc nào",
                    Data = []
                };
            }

            // Group assignments theo campaign
            var campaignGroups = assignments.GroupBy(a => a.CampaignId);
            var result = new List<AssignedCampaignDto>();

            foreach (var group in campaignGroups)
            {
                var campaignId = group.Key;
                if (campaignId == null) continue;
                
                var campaign = await _unitOfWork.Campaigns.GetByIdAsync(campaignId.Value);
                if (campaign == null) continue;

                var assignmentDtos = group.Select(a => new AssignmentDto
                {
                    AssignmentId = a.AssignmentId,
                    Task = a.Task,
                    TaskDescription = a.TaskDescription,
                    Status = GetStatusText(a.Status),
                    AssignedAt = a.AssignedAt,
                    AssignedBy = a.AssignedBy,
                    CompletedAt = a.CompletedAt,
                    Notes = a.Notes
                }).ToList();

                var campaignDto = new AssignedCampaignDto
                {
                    CampaignId = campaign.CampaignId,
                    CampaignName = campaign.CampaignName,
                    Description = campaign.Description,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    Assignments = assignmentDtos
                };

                result.Add(campaignDto);
            }

            return new Response<List<AssignedCampaignDto>>
            {
                Code = 200,
                Message = "Lấy danh sách task thành công",
                Data = result
            };
        }

        private static string GetStatusText(AssignmentStatus status)
        {
            return status switch
            {
                AssignmentStatus.Assigned => "Assigned",
                AssignmentStatus.InProgress => "In Progress", 
                AssignmentStatus.Completed => "Completed",
                AssignmentStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}