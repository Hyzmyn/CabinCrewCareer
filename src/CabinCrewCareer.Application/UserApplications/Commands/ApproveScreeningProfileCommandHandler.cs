using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class ApproveScreeningProfileCommandHandler : IRequestHandler<ApproveScreeningProfileCommand, Response<int?>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public ApproveScreeningProfileCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<Response<int?>> Handle(ApproveScreeningProfileCommand request, CancellationToken cancellationToken)
        {
            var activity = await _unitOfWork.RoundActivites.GetByIdAsync(request.ActivityId);
            if (activity == null)
                return new Response<int?>(null, ResponseCode.ROUND_ACTIVITY_NOT_FOUND.GetDescription(), (int)ResponseCode.ROUND_ACTIVITY_NOT_FOUND);

            var round = await _unitOfWork.Rounds.GetByIdAsync(activity.RoundId);
            if (round == null)
                return new Response<int?>(null, ResponseCode.ROUND_NOT_FOUND.GetDescription(), (int)ResponseCode.ROUND_NOT_FOUND);
            var campaignRound = await _unitOfWork.CampaignRounds.GetByIdAsync(round.CampaignRoundId);
            if (campaignRound == null)
                return new Response<int?>(null, ResponseCode.CAMPAIGN_ROUND_NOT_FOUND.GetDescription(), (int)ResponseCode.CAMPAIGN_ROUND_NOT_FOUND);
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(campaignRound.CampaignId);
            if (campaign == null)
                return new Response<int?>(null, ResponseCode.CAMPAIGN_NOT_FOUND.GetDescription(), (int)ResponseCode.CAMPAIGN_NOT_FOUND);

            int? nextActivityId = null;

            // Chỉ cho phép chuyển từ Ongoing sang Passed hoặc Failed
            if (activity.Status != ActivityStatus.Ongoing)
                return new Response<int?>(null, "Chỉ được duyệt activity đang ở trạng thái Ongoing", (int)ResponseCode.FAIL);

            if (request.Status == (int)ActivityStatus.Passed)
            {
                activity.Status = ActivityStatus.Passed;
                await _unitOfWork.RoundActivites.UpdateAsync(activity);

                if (campaign.CampaignType == CampaignType.Recruitment)
                {
                    var nextRoundQuery = await _unitOfWork.Rounds.GetAsync(r => r.CampaignRoundId == campaignRound.CampaignRoundId && r.RoundType == RoundType.Appearance);
                    var nextRoundList = nextRoundQuery.ToList();
                    var nextRound = nextRoundList.FirstOrDefault();
                    if (nextRound != null)
                    {
                        var existedActivityQuery = await _unitOfWork.RoundActivites.GetAsync(x => x.RoundId == nextRound.RoundId && x.UserId == activity.UserId);
                        var existedActivityList = existedActivityQuery.ToList();
                        var existedActivity = existedActivityList.FirstOrDefault();
                        if (existedActivity == null)
                        {
                            var newActivity = new RoundActivity { RoundId = nextRound.RoundId, UserId = activity.UserId, Status = ActivityStatus.Ongoing, ApplicationSnapshotId = activity.ApplicationSnapshotId };
                            await _unitOfWork.RoundActivites.AddAsync(newActivity);
                            await _unitOfWork.CompleteAsync();
                            nextActivityId = newActivity.ActivityId;
                        }
                    }
                }
                else if (campaign.CampaignType == CampaignType.Promotion)
                {
                    var nextRoundQuery = await _unitOfWork.Rounds.GetAsync(r => r.CampaignRoundId == campaignRound.CampaignRoundId && r.RoundType == RoundType.FlightHoursConfirmation);
                    var nextRoundList = nextRoundQuery.ToList();
                    var nextRound = nextRoundList.FirstOrDefault();
                    if (nextRound != null)
                    {
                        var existedActivityQuery = await _unitOfWork.RoundActivites.GetAsync(x => x.RoundId == nextRound.RoundId && x.UserId == activity.UserId);
                        var existedActivityList = existedActivityQuery.ToList();
                        var existedActivity = existedActivityList.FirstOrDefault();
                        if (existedActivity == null)
                        {
                            var newActivity = new RoundActivity { RoundId = nextRound.RoundId, UserId = activity.UserId, Status = ActivityStatus.Ongoing, ApplicationSnapshotId = activity.ApplicationSnapshotId };
                            await _unitOfWork.RoundActivites.AddAsync(newActivity);
                            await _unitOfWork.CompleteAsync();
                            nextActivityId = newActivity.ActivityId;
                        }
                    }
                }
                await _unitOfWork.CompleteAsync();
                // Gửi email thông báo PASS
                try
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(activity.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                        string subject = "Screening result - Cabin Crew Career";
                        string extraMessage = "Congratulations! you have passed the Screening round.";
                        string nextRoundInfo = "";
                        Round? nextRoundForInfo = null;
                        if (campaign.CampaignType == CampaignType.Recruitment)
                        {
                            var nextRoundQuery = await _unitOfWork.Rounds.GetAsync(r => r.CampaignRoundId == campaignRound.CampaignRoundId && r.RoundType == RoundType.Appearance);
                            var nextRoundList = nextRoundQuery.ToList();
                            nextRoundForInfo = nextRoundList.FirstOrDefault();
                        }
                        else if (campaign.CampaignType == CampaignType.Promotion)
                        {
                            var nextRoundQuery = await _unitOfWork.Rounds.GetAsync(r => r.CampaignRoundId == campaignRound.CampaignRoundId && r.RoundType == RoundType.FlightHoursConfirmation);
                            var nextRoundList = nextRoundQuery.ToList();
                            nextRoundForInfo = nextRoundList.FirstOrDefault();
                        }
                        if (nextRoundForInfo != null)
                        {
                            string nextRoundName = nextRoundForInfo.RoundType switch
                            {
                                RoundType.PracticalTest => "Practical Test",
                                RoundType.Interview => "Interview",
                                RoundType.Final => "Final",
                                RoundType.Appearance => "Appearance",
                                RoundType.FlightHoursConfirmation => "Flight Hours Confirmation",
                                _ => "Next round"
                            };
                            string startDate = nextRoundForInfo.StartDate?.ToString("dd/MM/yyyy HH:mm") ?? "Undefined";
                            string endDate = nextRoundForInfo.EndDate?.ToString("dd/MM/yyyy HH:mm") ?? "Undefined";
                            nextRoundInfo = $"<p><strong>Next round:</strong> {nextRoundName}</p>" +
                                           $"<p><strong>Start date:</strong> {startDate}</p>" +
                                           $"<p><strong>End date:</strong> {endDate}</p>";
                        }
                        string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                        body = body.Replace("{FullName}", user.FullName)
                                   .Replace("{Result}", "PASSED")
                                   .Replace("{RoundName}", "Screening")
                                   .Replace("{CampaignName}", campaign.CampaignName ?? "")
                                   .Replace("{ExtraMessage}", extraMessage)
                                   .Replace("{NextRoundInfo}", nextRoundInfo)
                                   .Replace("{Round}", "Screening");
                        var mailRequest = new CabinCrewCareer.Application.Common.Models.Requests.MailRequest
                        {
                            ToEmail = user.Email,
                            Subject = subject,
                            Body = body
                        };
                        await _emailService.SendEmailAsync(mailRequest);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Screening Email PASS] {ex.Message}");
                }
                return new Response<int?>(nextActivityId, ResponseCode.UPDATED_SUCCESS.GetDescription(), (int)ResponseCode.UPDATED_SUCCESS);
            }
            else if (request.Status == (int)ActivityStatus.Failed)
            {
                activity.Status = ActivityStatus.Failed;
                await _unitOfWork.RoundActivites.UpdateAsync(activity);
                // Cập nhật UserHistory sang NotCompleted
                var historyQuery = await _unitOfWork.UserHistories.GetAsync(x => x.UserId == (int)activity.UserId && x.Type == HistoryType.Ongoing);

                var history = await historyQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (history != null)
                {
                    history.Type = HistoryType.NotCompleted;
                    await _unitOfWork.UserHistories.UpdateAsync(history);
                }
                await _unitOfWork.CompleteAsync();
                // Gửi email thông báo FAIL
                try
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(activity.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                        string subject = "Screening round result";
                        string extraMessage = "Unfortunately, you did not pass the screening round.";
                        string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                        body = body.Replace("{FullName}", user.FullName)
                                   .Replace("{Result}", "FAILED")
                                   .Replace("{RoundName}", "Screening")
                                   .Replace("{CampaignName}", campaign.CampaignName ?? "")
                                   .Replace("{ExtraMessage}", extraMessage)
                                   .Replace("{Round}", "Screening")
                                   .Replace("{NextRoundInfo}", "");
                        var mailRequest = new CabinCrewCareer.Application.Common.Models.Requests.MailRequest
                        {
                            ToEmail = user.Email,
                            Subject = subject,
                            Body = body
                        };
                        await _emailService.SendEmailAsync(mailRequest);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Screening Email FAIL] {ex.Message}");
                }
                return new Response<int?>(null, ResponseCode.UPDATED_SUCCESS.GetDescription(), (int)ResponseCode.UPDATED_SUCCESS);
            }
            else
            {
                return new Response<int?>(null, "Chỉ được chuyển sang Passed hoặc Failed", (int)ResponseCode.FAIL);
            }
        }
    }
}
