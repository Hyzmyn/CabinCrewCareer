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

        public AssignRecruitersAndExaminersCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICampaignStatusService campaignStatusService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _campaignStatusService = campaignStatusService;
            _notificationService = notificationService;
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
                // ======= TỰ ĐỘNG TẠO ROUNDS CHO ĐỢT ĐẦU TIÊN =======
                try
                {
                    var campaignRounds = await _unitOfWork.CampaignRounds.GetAllAsync(x => x.CampaignId == campaign.CampaignId);
                    var firstCampaignRound = campaignRounds
                        .Where(x => x.StartDate.HasValue)
                        .OrderBy(x => x.StartDate.Value)
                        .FirstOrDefault();

                    if (firstCampaignRound != null)
                    {
                        List<RoundType> roundTypes;
                        if (campaign.CampaignType == CampaignType.Recruitment)
                        {
                            roundTypes =
                        [
                            RoundType.Screening,
                            RoundType.Appearance,
                            RoundType.EnglishListeningTest,
                            RoundType.EnglishSpeakingTest,
                            RoundType.Interview,
                            RoundType.Final
                        ];
                        }
                        else if (campaign.CampaignType == CampaignType.Promotion)
                        {
                            roundTypes =
                        [
                            RoundType.Screening,
                            RoundType.FlightHoursConfirmation,
                            RoundType.PracticalTest,
                            RoundType.Interview,
                            RoundType.Final
                        ];
                        }
                        else
                        {
                            roundTypes = [];
                        }

                        var existingRounds = await _unitOfWork.Rounds.GetAllAsync(r => r.CampaignRoundId == firstCampaignRound.CampaignRoundId);

                        var newRounds = new List<Round>();
                        foreach (var roundType in roundTypes)
                        {
                            if (!existingRounds.Any(r => r.RoundType == roundType))
                            {
                                newRounds.Add(new Round
                                {
                                    CampaignRoundId = firstCampaignRound.CampaignRoundId,
                                    RoundType = roundType,
                                    StartDate = firstCampaignRound.StartDate,
                                    EndDate = firstCampaignRound.EndDate,
                                    TestId = null,
                                    Notes = string.Empty
                                });
                            }
                        }
                        if (newRounds.Count > 0)
                        {
                            await _unitOfWork.Rounds.AddRangeAsync(newRounds);
                            await _unitOfWork.CompleteAsync();
                        }
                    }
                }
                catch (Exception)
                {
                    // Log lỗi tạo rounds nếu cần
                }
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
            }

            // ======= TỰ ĐỘNG TẠO ROUNDS CHO ĐỢT ĐẦU TIÊN =======
            try
            {
                // Lấy tất cả campaign rounds của campaign, sắp xếp theo startDate tăng dần
                var campaignRounds = await _unitOfWork.CampaignRounds.GetAllAsync(x => x.CampaignId == campaign.CampaignId);
                var firstCampaignRound = campaignRounds
                    .Where(x => x.StartDate.HasValue)
                    .OrderBy(x => x.StartDate.Value)
                    .FirstOrDefault();

                if (firstCampaignRound != null)
                {
                    List<RoundType> roundTypes;
                    if (campaign.CampaignType == CampaignType.Recruitment)
                    {
                        roundTypes =
                        [
                            RoundType.Screening,
                            RoundType.Appearance,
                            RoundType.EnglishListeningTest,
                            RoundType.EnglishSpeakingTest,
                            RoundType.Interview,
                            RoundType.Final
                        ];
                    }
                    else if (campaign.CampaignType == CampaignType.Promotion)
                    {
                        roundTypes =
                        [
                            RoundType.Screening,
                            RoundType.FlightHoursConfirmation,
                            RoundType.PracticalTest,
                            RoundType.Interview,
                            RoundType.Final
                        ];
                    }
                    else
                    {
                        roundTypes = [];
                    }

                    // Lấy các rounds đã tồn tại của đợt đầu tiên
                    var existingRounds = await _unitOfWork.Rounds.GetAllAsync(r => r.CampaignRoundId == firstCampaignRound.CampaignRoundId);

                    var newRounds = new List<Round>();
                    foreach (var roundType in roundTypes)
                    {
                        // Nếu chưa tồn tại round này thì tạo mới
                        if (!existingRounds.Any(r => r.RoundType == roundType))
                        {
                            newRounds.Add(new Round
                            {
                                CampaignRoundId = firstCampaignRound.CampaignRoundId,
                                RoundType = roundType,
                                StartDate = firstCampaignRound.StartDate,
                                EndDate = firstCampaignRound.EndDate,
                                TestId = null,
                                Notes = string.Empty
                            });
                        }
                    }
                    if (newRounds.Count > 0)
                    {
                        await _unitOfWork.Rounds.AddRangeAsync(newRounds);
                        await _unitOfWork.CompleteAsync();
                    }
                }
                // else: không tìm thấy đợt đầu tiên, không tạo rounds
            }
            catch (Exception)
            {
                // Log lỗi tạo rounds
                // TODO: Thêm logger nếu cần
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
            campaign.CampaignStatus = CampaignStatus.Ongoing;
            await _unitOfWork.Campaigns.UpdateAsync(campaign);

            await _unitOfWork.CompleteAsync();

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