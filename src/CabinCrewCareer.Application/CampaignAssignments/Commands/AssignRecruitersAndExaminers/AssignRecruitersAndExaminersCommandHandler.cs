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

        public AssignRecruitersAndExaminersCommandHandler(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUserService,
            ICampaignStatusService campaignStatusService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _campaignStatusService = campaignStatusService;
        }

        public async Task<Response<bool>> Handle(
            AssignRecruitersAndExaminersCommand request, 
            CancellationToken cancellationToken)
        {
            // Kiểm tra campaign tồn tại
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId);
            if (campaign == null)
            {
                return new Response<bool>
                {
                    Code = 404,
                    Message = "Không tìm thấy chiến dịch",
                    Data = false
                };
            }

            // Kiểm tra trạng thái campaign (chỉ campaign Approved mới có thể giao việc)
            if (campaign.CampaignStatus != CampaignStatus.Approved)
            {
                return new Response<bool>
                {
                    Code = 400,
                    Message = "Chỉ có thể giao việc cho chiến dịch đã được duyệt",
                    Data = false
                };
            }

            // Kiểm tra quyền và lấy thông tin Senior đăng nhập
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out var seniorUserId))
            {
                return new Response<bool>
                {
                    Code = 401,
                    Message = "Không xác định được người giao việc",
                    Data = false
                };
            }

            // Lấy thông tin Senior để có username
            var seniorUser = await _unitOfWork.Users.GetByIdAsync(seniorUserId);
            if (seniorUser == null)
            {
                return new Response<bool>
                {
                    Code = 401,
                    Message = "Không tìm thấy thông tin Senior",
                    Data = false
                };
            }

            // Kiểm tra đã giao việc cho campaign này chưa
            var existingAssignments = await _unitOfWork.CampaignAssignments
                .GetAssignmentsByCampaignIdAsync(request.CampaignId);
            
            if (existingAssignments.Any())
            {
                return new Response<bool>
                {
                    Code = 400,
                    Message = "Chiến dịch này đã được giao việc",
                    Data = false
                };
            }

            // Kiểm tra tất cả users trong assignments có tồn tại không
            foreach (var assignment in request.Assignments)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(assignment.UserId);
                if (user == null)
                {
                    return new Response<bool>
                    {
                        Code = 404,
                        Message = $"Không tìm thấy user với ID {assignment.UserId}",
                        Data = false
                    };
                }

                // Kiểm tra workload - Recruiter/Examiner không được có quá 3 campaigns "Ongoing"
                var ongoingCampaignsCount = await _unitOfWork.CampaignAssignments
                    .GetOngoingCampaignsCountByUserIdAsync(assignment.UserId);
                
                if (ongoingCampaignsCount >= 3)
                {
                    return new Response<bool>
                    {
                        Code = 400,
                        Message = $"{user.UserName} đã có {ongoingCampaignsCount} campaigns đang diễn ra. Không thể giao thêm nhiệm vụ.",
                        Data = false
                    };
                }
            }

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
            campaign.CampaignStatus = CampaignStatus.Published;
            await _unitOfWork.Campaigns.UpdateAsync(campaign);

            await _unitOfWork.CompleteAsync();

            // Cập nhật status dựa trên StartDate và EndDate
            await _campaignStatusService.UpdateCampaignStatusByDateAsync(request.CampaignId);

            return new Response<bool>
            {
                Code = 200,
                Message = "Giao việc thành công. Trạng thái chiến dịch được cập nhật tự động.",
                Data = true
            };
        }


    }
}