using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.EvaluationForms.Commands.CreateInterviewEvaluationForm
{
    public class CreateInterviewEvaluationFormCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IEmailService emailService) : IRequestHandler<CreateInterviewEvaluationFormCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<Response<string>> Handle(CreateInterviewEvaluationFormCommand request, CancellationToken cancellationToken)
        {
            double finalScore = 0;
            double totalCriteriasScore = request.Choices.Count * 10;

            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            if (await _unitOfWork.EvaluationForms.HasEvaluatedRound(request.ActivityId, Int32.Parse(currentUserId))) throw new ApiException(ResponseCode.CANDIDATE_HAS_BEEN_EVALUATED);

            EvaluationForm form = new()
            {
                ActivityId = request.ActivityId,
                Comment = request.Comment,
                ExaminerId = Int32.Parse(currentUserId),
                Type = EvaluationType.Interview,
                EvaluatedDate = _dateTimeProvider.UtcNow
            };

            foreach (var item in request.Choices)
            {
                form.InterviewResults.Add(new InterviewResult
                {
                    Score = item.Score,
                    InterviewCriteriaItemId = item.InterviewCriteriaItemId,
                    Comment = item.Comment
                });

                finalScore += item.Score;
            }

            double finalScorePercentage = (finalScore / totalCriteriasScore) * 100;

            var roundActivityQuery = await _unitOfWork.RoundActivites.GetAsync(
                expression: x => x.ActivityId == request.ActivityId,
                includeFunc: x => x.Include(x => x.Round));

            var roundActivity = await roundActivityQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var evaluationForms = await _unitOfWork.EvaluationForms.GetInterviewFormsAsync(request.ActivityId);

            if (evaluationForms.Count == 3) throw new ApiException(ResponseCode.EXCEED_FORM_LIMIT);

            // Gán IsPassed cho form trước khi tính toán pass/fail
            if (request.Type == (int)CampaignType.Recruitment)
            {
                var round = await _unitOfWork.RoundConfigurations.GetCurrentlyEffectiveConfiguration((int)RoundConfigurationType.RecruitmentInterview) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

                form.ConfigurationId = round.RoundConfigurationId;

                if (finalScorePercentage > await _unitOfWork.RoundConfigurations.RecruitmentInterviewBenchmark())
                {
                    form.IsPassed = true;
                }
                else
                {
                    form.IsPassed = false;
                }
            }
            else
            {
                var round = await _unitOfWork.RoundConfigurations.GetCurrentlyEffectiveConfiguration((int)RoundConfigurationType.PromotionInterview) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

                form.ConfigurationId = round.RoundConfigurationId;

                if (finalScorePercentage > await _unitOfWork.RoundConfigurations.PromotionInterviewBenchmark()) form.IsPassed = true;
                else form.IsPassed = false;
            }

            // Thêm form mới vào danh sách để tính toán pass/fail
            var tempForms = evaluationForms.ToList();
            tempForms.Add(form);

            int passCount = tempForms.Count(x => x.IsPassed);
            int failCount = tempForms.Count(x => x.IsPassed == false);
            int totalCount = tempForms.Count;

            if (request.Type == (int)CampaignType.Recruitment)
            {
                if (roundActivity is not null)
                {
                    // Chỉ khi đủ 3 phiếu và pass >= 2
                    if (totalCount == 3 && passCount >= 2)
                    {
                        roundActivity.Status = ActivityStatus.Passed;

                        // Kiểm tra đã có RoundActivity cho vòng cuối chưa
                        var finalRoundQuery = await _unitOfWork.Rounds.GetAsync(
                            expression: x => x.CampaignRoundId == roundActivity.Round.CampaignRoundId && x.RoundType == RoundType.Final);
                        var finalRound = await finalRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        string nextRoundInfo = "";
                        if (finalRound is not null)
                        {
                            var finalRoundActivityQuery = await _unitOfWork.RoundActivites.GetAsync(
                                expression: x => x.RoundId == finalRound.RoundId && x.UserId == roundActivity.UserId);
                            var existedFinalRoundActivity = await finalRoundActivityQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                            if (existedFinalRoundActivity is null)
                            {
                                await _unitOfWork.RoundActivites.AddAsync(new RoundActivity { RoundId = finalRound.RoundId, UserId = roundActivity.UserId, Status = ActivityStatus.Ongoing, ApplicationSnapshotId = roundActivity.ApplicationSnapshotId });
                            }
                            // Lấy thông tin thời gian vòng final
                            string finalStartDate = finalRound.StartDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                            string finalEndDate = finalRound.EndDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                            nextRoundInfo = $"Next round: <strong>Final</strong><br>Time: <strong>{finalStartDate}</strong> - <strong>{finalEndDate}</strong>";
                        }
                        await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
                        // Gửi email PASS
                        try
                        {
                            var user = await _unitOfWork.Users.GetByIdAsync(roundActivity.UserId);
                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                string locationInfo = await GetLocationInfoAsync(roundActivity, cancellationToken);
                                var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                                string subject = "Interview result";
                                string extraMessage = "Congratulations! You have passed the Interview round";
                                string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                                body = body.Replace("{FullName}", user.FullName)
                                           .Replace("{Result}", "PASSED")
                                           .Replace("{RoundName}", "Interview")
                                           .Replace("{CampaignName}", roundActivity.Round?.CampaignRound?.Campaign?.CampaignName ?? "")
                                           .Replace("{ExtraMessage}", extraMessage)
                                           .Replace("{Round}", "Interview")
                                           .Replace("{NextRoundInfo}", nextRoundInfo)
                                           .Replace("{LocationInfo}", locationInfo);
                                var mailRequest = new CabinCrewCareer.Application.Common.Models.Requests.MailRequest
                                {
                                    ToEmail = user.Email,
                                    Subject = subject,
                                    Body = body
                                };
                                await emailService.SendEmailAsync(mailRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Interview Email PASS] {ex.Message}");
                        }
                    }
                    // Chỉ khi đủ 3 phiếu và fail >= 2
                    else if (totalCount == 3 && failCount >= 2)
                    {
                        roundActivity.Status = ActivityStatus.Failed;
                        // Không tạo mới RoundActivity cho vòng cuối
                        var history = await _unitOfWork.UserHistories.GetAsync(x => x.UserId == (int)roundActivity.UserId && x.Type == HistoryType.Ongoing);
                        var rehe = await history.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                        rehe.Type = HistoryType.NotCompleted;
                        await _unitOfWork.UserHistories.UpdateAsync(rehe);
                        await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
                        // Gửi email FAIL
                        try
                        {
                            var user = await _unitOfWork.Users.GetByIdAsync(roundActivity.UserId);
                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                string locationInfo = await GetLocationInfoAsync(roundActivity, cancellationToken);
                                var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                                string subject = "Interview result";
                                string extraMessage = "Unfortunately! You did not pass the Interview round.";
                                string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                                body = body.Replace("{FullName}", user.FullName)
                                           .Replace("{Result}", "FAILED")
                                           .Replace("{RoundName}", "Interivew")
                                           .Replace("{CampaignName}", roundActivity.Round?.CampaignRound?.Campaign?.CampaignName ?? "")
                                           .Replace("{ExtraMessage}", extraMessage)
                                           .Replace("{Round}", "Interview")
                                           .Replace("{NextRoundInfo}", "")
                                           .Replace("{LocationInfo}", locationInfo);
                                var mailRequest = new CabinCrewCareer.Application.Common.Models.Requests.MailRequest
                                {
                                    ToEmail = user.Email,
                                    Subject = subject,
                                    Body = body
                                };
                                await emailService.SendEmailAsync(mailRequest);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Interview Email FAIL] {ex.Message}");
                        }
                    }
                    else
                    {
                        await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
                    }
                }
                await _unitOfWork.EvaluationForms.AddAsync(form);
                await _unitOfWork.CompleteAsync();
                return new Response<string>(
                    code: (int)ResponseCode.CREATED_SUCCESS,
                    data: ResponseCode.CREATED_SUCCESS.ToString(),
                    message: ResponseCode.CREATED_SUCCESS.GetDescription());
            }

            // Loại khác
            if (roundActivity is not null)
            {
                // Chỉ khi đủ 3 phiếu và pass >= 2
                if (totalCount == 3 && passCount >= 2)
                {
                    roundActivity.Status = ActivityStatus.Passed;

                    var finalRoundQuery = await _unitOfWork.Rounds.GetAsync(
                        expression: x => x.CampaignRoundId == roundActivity.Round.CampaignRoundId && x.RoundType == RoundType.Final);
                    var finalRound = await finalRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    string nextRoundInfo = "";

                    if (finalRound is not null)
                    {
                        var finalRoundActivityQuery = await _unitOfWork.RoundActivites.GetAsync(
                            expression: x => x.RoundId == finalRound.RoundId && x.UserId == roundActivity.UserId);
                        var existedFinalRoundActivity = await finalRoundActivityQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                        if (existedFinalRoundActivity is null)
                        {
                            await _unitOfWork.RoundActivites.AddAsync(new RoundActivity { RoundId = finalRound.RoundId, UserId = roundActivity.UserId, Status = ActivityStatus.Ongoing, ApplicationSnapshotId = roundActivity.ApplicationSnapshotId });
                        }

                        // Lấy thông tin thời gian vòng final
                        string finalStartDate = finalRound.StartDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                        string finalEndDate = finalRound.EndDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                        nextRoundInfo = $"Next round: <strong>Final</strong><br>Time: <strong>{finalStartDate}</strong> - <strong>{finalEndDate}</strong>";
                    }

                    await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);

                    try
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(roundActivity.UserId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            string locationInfo = await GetLocationInfoAsync(roundActivity, cancellationToken);
                            var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                            string subject = "Interview result";
                            string extraMessage = "Congratulations! You have passed the Interview round.";
                            string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                            body = body.Replace("{FullName}", user.FullName)
                                       .Replace("{Result}", "PASSED")
                                       .Replace("{RoundName}", "Interview")
                                       .Replace("{CampaignName}", roundActivity.Round?.CampaignRound?.Campaign?.CampaignName ?? "")
                                       .Replace("{ExtraMessage}", extraMessage)
                                       .Replace("{Round}", "Interview")
                                       .Replace("{NextRoundInfo}", nextRoundInfo)
                                       .Replace("{LocationInfo}", locationInfo);
                            var mailRequest = new CabinCrewCareer.Application.Common.Models.Requests.MailRequest
                            {
                                ToEmail = user.Email,
                                Subject = subject,
                                Body = body
                            };
                            await emailService.SendEmailAsync(mailRequest);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Interview Email PASS] {ex.Message}");
                    }
                }
                // Chỉ khi đủ 3 phiếu và fail >= 2
                else if (totalCount == 3 && failCount >= 2)
                {
                    roundActivity.Status = ActivityStatus.Failed;
                    // Không tạo mới RoundActivity cho vòng cuối
                    var history = await _unitOfWork.UserHistories.GetAsync(x => x.UserId == (int)roundActivity.UserId && x.Type == HistoryType.Ongoing);
                    var rehe = await history.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    rehe.Type = HistoryType.NotCompleted;
                    await _unitOfWork.UserHistories.UpdateAsync(rehe);
                    await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
                    // Gửi email FAIL
                    try
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(roundActivity.UserId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            string locationInfo = await GetLocationInfoAsync(roundActivity, cancellationToken);
                            var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                            string subject = "Interview result";
                            string extraMessage = "Unfortunately! You did not pass the Interview round.";
                            string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                            body = body.Replace("{FullName}", user.FullName)
                                       .Replace("{Result}", "FAILED")
                                       .Replace("{RoundName}", "Interview")
                                       .Replace("{CampaignName}", roundActivity.Round?.CampaignRound?.Campaign?.CampaignName ?? "")
                                       .Replace("{ExtraMessage}", extraMessage)
                                       .Replace("{Round}", "Interview")
                                       .Replace("{NextRoundInfo}", "")
                                       .Replace("{LocationInfo}", locationInfo);
                            var mailRequest = new CabinCrewCareer.Application.Common.Models.Requests.MailRequest
                            {
                                ToEmail = user.Email,
                                Subject = subject,
                                Body = body
                            };
                            await emailService.SendEmailAsync(mailRequest);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Interview Email FAIL] {ex.Message}");
                    }
                }
                else
                {
                    await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
                }
            }

            await _unitOfWork.EvaluationForms.AddAsync(form);
            await _unitOfWork.CompleteAsync();

            return new Response<string>(
                    code: (int)ResponseCode.CREATED_SUCCESS,
                    data: ResponseCode.CREATED_SUCCESS.ToString(),
                    message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }

        private async Task<string> GetLocationInfoAsync(RoundActivity roundActivity, CancellationToken cancellationToken)
        {
            string locationInfo = "";
            if (roundActivity.Round?.CampaignRound?.Campaign != null)
            {
                var campaignType = roundActivity.Round.CampaignRound.Campaign.CampaignType;
                var locationQuery = await _unitOfWork.Locations.GetAsync(
                    expression: x => x.CampaignType == campaignType,
                    includeFunc: x => x.Include(l => l.Ward).ThenInclude(w => w.City));
                var location = await locationQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (location != null)
                {
                    string fullAddress = location.Address;
                    if (location.Ward != null)
                    {
                        fullAddress += $", {location.Ward.WardName}";
                        if (location.Ward.City != null)
                        {
                            fullAddress += $", {location.Ward.City.CityName}";
                        }
                    }
                    locationInfo = $"<p><strong>📍 Interview Location:</strong> {fullAddress}</p>";
                }
            }
            return locationInfo;
        }
    }
}
