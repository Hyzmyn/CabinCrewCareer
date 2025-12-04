using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignAssignments.Commands.AssignRecruitersAndExaminers
{
    public class AssignRecruitersAndExaminersCommandHandler
        : IRequestHandler<AssignRecruitersAndExaminersCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICampaignStatusService _campaignStatusService;
        private readonly INotificationService _notificationService;
        private readonly ICampaignRoundStatusService _campaignRoundStatusService;

        public AssignRecruitersAndExaminersCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICampaignStatusService campaignStatusService,
            INotificationService notificationService,
            ICampaignRoundStatusService campaignRoundStatusService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _campaignStatusService = campaignStatusService;
            _notificationService = notificationService;
            _campaignRoundStatusService = campaignRoundStatusService;
        }

        public async Task<Response<bool>> Handle(
            AssignRecruitersAndExaminersCommand request,
            CancellationToken cancellationToken)
        {
            // Kiểm tra campaign tồn tại
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId);
            if (campaign == null)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.CAMPAIGN_NOT_FOUND,
                    data: false,
                    message: ResponseCode.CAMPAIGN_NOT_FOUND.GetDescription());
            }

            // Kiểm tra trạng thái campaign (chỉ campaign Approved mới có thể giao việc)
            // Sau khi giao việc sẽ tự động chuyển sang Published
            if (campaign.CampaignStatus != CampaignStatus.Approved)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.CAMPAIGN_NOT_APPROVED,
                    data: false,
                    message: "Chỉ có thể giao việc cho campaign ở trạng thái Approved");
            }

            // Kiểm tra quyền và lấy thông tin Senior đăng nhập
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out var seniorUserId))
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAILED_AUTHENTICATION,
                    data: false,
                    message: ResponseCode.FAILED_AUTHENTICATION.GetDescription());
            }

            // Lấy thông tin Senior để có username
            var seniorUser = await _unitOfWork.Users.GetByIdAsync(seniorUserId);
            if (seniorUser == null)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.USER_NOT_FOUND,
                    data: false,
                    message: ResponseCode.USER_NOT_FOUND.GetDescription());
            }

            // Kiểm tra đã giao việc cho campaign này chưa
            var existingAssignments = await _unitOfWork.CampaignAssignments
                .GetAssignmentsByCampaignIdAsync(request.CampaignId);

            if (existingAssignments.Any())
            {
                return new Response<bool>(
                    code: (int)ResponseCode.CAMPAIGN_ALREADY_ASSIGNED,
                    data: false,
                    message: ResponseCode.CAMPAIGN_ALREADY_ASSIGNED.GetDescription());
            }

            // Kiểm tra tất cả users trong assignments có tồn tại không
            foreach (var assignment in request.Assignments)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(assignment.UserId);
                if (user == null)
                {
                    return new Response<bool>(
                        code: (int)ResponseCode.USER_NOT_FOUND,
                        data: false,
                        message: $"{ResponseCode.USER_NOT_FOUND.GetDescription()} (ID: {assignment.UserId})");
                }

                // ========== VALIDATION LOGIC MỚI ==========
                // Chỉ kiểm tra overlap thời gian với các campaigns khác
                // var hasOverlap = await _unitOfWork.CampaignAssignments
                //     .HasOverlappingCampaignAsync(assignment.UserId, request.CampaignId);
                // if (hasOverlap)
                // {
                //     return new Response<bool>(
                //         code: (int)ResponseCode.USER_HAS_OVERLAPPING_CAMPAIGN,
                //         data: false,
                //         message: $"{user.UserName}: {ResponseCode.USER_HAS_OVERLAPPING_CAMPAIGN.GetDescription()}");
                // }
                    // ...existing code...
            }

            // ...existing code...
            // Tạo assignments theo danh sách
            foreach (var assignment in request.Assignments)
            {
                var campaignAssignment = new CampaignAssignment
                {
                    CampaignId = request.CampaignId,
                    UserId = assignment.UserId,
                    Task = assignment.TaskType,
                    TaskDescription = assignment.TaskDescription ?? string.Empty,
                    Status = AssignmentStatus.Assigned,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = seniorUser.UserName
                };

                await _unitOfWork.CampaignAssignments.AddAsync(campaignAssignment);
            }

            // Cập nhật trạng thái campaign thành Published
            campaign.CampaignStatus = CampaignStatus.Ongoing;
            await _unitOfWork.Campaigns.UpdateAsync(campaign);

            await _unitOfWork.CompleteAsync();

            // Cập nhật trạng thái round cho đợt đầu tiên
            var allCampaignRounds = await _unitOfWork.CampaignRounds.GetAllAsync(x => x.CampaignId == campaign.CampaignId);
            var firstRound = allCampaignRounds
                .Where(x => x.StartDate.HasValue)
                .OrderBy(x => x.StartDate ?? DateTime.MaxValue)
                .FirstOrDefault();
            if (firstRound != null)
            {
                await _campaignRoundStatusService.UpdateCampaignRoundStatusByDateAsync(firstRound.CampaignRoundId, cancellationToken);
            }

            // Gửi notification cho tất cả user được giao việc
            try
            {
                foreach (var assignment in request.Assignments)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(assignment.UserId);
                    if (user != null)
                    {
                        await _notificationService.SendTaskAssignmentNotificationAsync(
                            userId: assignment.UserId,
                            campaignName: campaign.CampaignName,
                            taskDescription: assignment.TaskType.ToString(),
                            assignedBy: seniorUser.UserName
                        );
                    }
                }

                // Lưu tất cả notifications vào DB
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                // Notification failed but assignment already saved
                // Continue without failing the whole request
            }

            // Cập nhật status dựa trên StartDate và EndDate
            try
            {
                await _campaignStatusService.UpdateCampaignStatusByDateAsync(request.CampaignId, cancellationToken);
            }
            catch (Exception)
            {
                // Status update failed but assignment already saved
                // Background service will update it later
            }

            return new Response<bool>(
                code: (int)ResponseCode.CREATED_SUCCESS,
                data: true,
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}