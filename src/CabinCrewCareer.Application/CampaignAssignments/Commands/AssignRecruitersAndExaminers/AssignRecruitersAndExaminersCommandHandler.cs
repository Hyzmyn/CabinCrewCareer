using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Models.Requests;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        private readonly IEmailService _emailService;

        public AssignRecruitersAndExaminersCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICampaignStatusService campaignStatusService,
            INotificationService notificationService,
            ICampaignRoundStatusService campaignRoundStatusService,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _campaignStatusService = campaignStatusService;
            _notificationService = notificationService;
            _campaignRoundStatusService = campaignRoundStatusService;
            _emailService = emailService;
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

            // ========== GỬI EMAIL BROADCAST CHO PROMOTION CAMPAIGN ==========
            // Nếu là campaign Promotion, gửi email thông báo đến tất cả Cabin Crew của hãng
            if (campaign.CampaignType == CampaignType.Promotion)
            {
                
                try
                {
                    // LẤY TẤT CẢ DATA TRƯỚC KHI CHẠY BACKGROUND TASK để tránh disposed context
                    var campaignRequestQuery = await _unitOfWork.CampaignRequests.GetAsync(
                        cr => cr.RequestId == campaign.RequestId,
                        includeFunc: q => q.Include(cr => cr.AirlinePartner)
                    );
                    var campaignRequest = await campaignRequestQuery.FirstOrDefaultAsync(cancellationToken);
                    
                    if (campaignRequest == null || campaignRequest.PartnerId == null || campaignRequest.AirlinePartner == null)
                    {
                        // Campaign request not found or missing partner info
                    }
                    else
                    {
                        
                        // Lấy tất cả Cabin Crew
                        var cabinCrews = await _unitOfWork.Users.GetAllAsync(u =>
                            u.RoleId == 6 && 
                            u.PartnerId == campaignRequest.PartnerId &&
                            !string.IsNullOrEmpty(u.Email)
                        );
                        
                        if (cabinCrews.Any())
                        {
                            // Lấy role name cho Purser (RoleId = 9)
                            var purserRoleQuery = await _unitOfWork.Roles.GetAsync(r => r.RoleId == 9);
                            var purserRole = await purserRoleQuery.FirstOrDefaultAsync(cancellationToken);
                            var positionName = purserRole?.RoleName ?? "Purser";
                            
                            // Tạo snapshot data để dùng trong background task
                            var emailData = new
                            {
                                CampaignName = campaign.CampaignName,
                                Description = campaign.Description,
                                TargetQuantity = campaign.TargetQuantity,
                                Position = positionName,
                                StartDate = campaign.StartDate?.ToString("dd/MM/yyyy HH:mm") ?? "Đang cập nhật",
                                EndDate = campaign.EndDate?.ToString("dd/MM/yyyy HH:mm") ?? "Đang cập nhật",
                                PartnerName = campaignRequest.AirlinePartner.PartnerName,
                                CabinCrewEmails = cabinCrews.Select(c => new { 
                                    Email = c.Email, 
                                    FullName = c.FullName ?? c.UserName 
                                }).ToList()
                            };
                            
                            // Chạy trong background task với data đã snapshot
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await SendPromotionAnnouncementEmailsAsync(emailData);
                                }
                                catch (Exception)
                                {
                                    // Email sending failed in background task
                                }
                            }, CancellationToken.None); // Use None to avoid cancellation after request completes
                        }
                        else
                        {
                            // No cabin crews found for this partner
                        }
                    }
                }
                catch (Exception)
                {
                    // Error preparing promotion email data
                }
            }

            return new Response<bool>(
                code: (int)ResponseCode.CREATED_SUCCESS,
                data: true,
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }

        /// <summary>
        /// Gửi email thông báo promotion campaign đến tất cả Cabin Crew của hãng
        /// Chạy trong background task với data đã được snapshot trước
        /// </summary>
        private async Task SendPromotionAnnouncementEmailsAsync(dynamic emailData)
        {
            // Đọc template email
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "PromotionAnnouncementTemplate.html");
            
            if (!File.Exists(templatePath))
            {
                return;
            }
            
            string templateBody = await File.ReadAllTextAsync(templatePath);

            // Gửi email cho từng cabin crew theo batch để tránh quá tải
            var cabinCrewList = ((IEnumerable<dynamic>)emailData.CabinCrewEmails).Cast<dynamic>().ToList();
            
            var batchSize = 50;
            var batches = cabinCrewList
                .Select((crew, index) => new { crew, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.crew).ToList())
                .ToList();
            
            foreach (var batch in batches)
            {
                var emailTasks = new List<Task>();
                
                foreach (var crew in batch)
                {
                    var emailBody = templateBody
                        .Replace("{FullName}", crew.FullName)
                        .Replace("{PartnerName}", emailData.PartnerName)
                        .Replace("{CampaignName}", emailData.CampaignName)
                        .Replace("{Description}", emailData.Description)
                        .Replace("{Position}", emailData.Position)
                        .Replace("{TargetQuantity}", emailData.TargetQuantity.ToString())
                        .Replace("{StartDate}", emailData.StartDate)
                        .Replace("{EndDate}", emailData.EndDate);

                    var mailRequest = new MailRequest
                    {
                        ToEmail = crew.Email,
                        Subject = $"🎉 Promotion Opportunity - {emailData.CampaignName}",
                        Body = emailBody
                    };

                    emailTasks.Add(_emailService.SendEmailAsync(mailRequest));
                }

                // Gửi từng batch và delay một chút giữa các batch
                try
                {
                    await Task.WhenAll(emailTasks);
                }
                catch (Exception)
                {
                    // Batch email sending failed, continue with next batch
                }

                // Delay 1s giữa các batch để tránh spam
                if (batches.IndexOf(batch) < batches.Count - 1)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}