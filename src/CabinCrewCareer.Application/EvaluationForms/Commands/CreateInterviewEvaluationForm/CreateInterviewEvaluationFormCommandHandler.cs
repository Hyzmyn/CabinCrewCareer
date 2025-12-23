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
            var finalScore = 0;

            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

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

            var roundActivityQuery = await _unitOfWork.RoundActivites.GetAsync(
                expression: x => x.ActivityId == request.ActivityId,
                includeFunc: x => x.Include(x => x.Round));

            var roundActivity = await roundActivityQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            var evaluationForms = await _unitOfWork.EvaluationForms.GetInterviewFormsAsync(request.ActivityId);

            if (evaluationForms.Count == 3) throw new ApiException(ResponseCode.EXCEED_FORM_LIMIT);

            // Gán IsPassed cho form trước khi tính toán pass/fail
            if (request.Type == (int)CampaignType.Recruitment)
            {
                if (finalScore > 120)
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
                if (finalScore > 70) form.IsPassed = true;
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
                            nextRoundInfo = $"Vòng tiếp theo: <strong>Final</strong><br>Thời gian: <strong>{finalStartDate}</strong> - <strong>{finalEndDate}</strong>";
                        }
                        await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
                        // Gửi email PASS
                        try
                        {
                            var user = await _unitOfWork.Users.GetByIdAsync(roundActivity.UserId);
                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                                string subject = "Kết quả vòng Phỏng vấn";
                                string extraMessage = "Chúc mừng bạn đã vượt qua vòng Phỏng vấn.";
                                string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                                body = body.Replace("{FullName}", user.FullName)
                                           .Replace("{Result}", "Đậu (PASS)")
                                           .Replace("{RoundName}", "Phỏng vấn")
                                           .Replace("{CampaignName}", roundActivity.Round?.CampaignRound?.Campaign?.CampaignName ?? "")
                                           .Replace("{ExtraMessage}", extraMessage)
                                           .Replace("{NextRoundInfo}", nextRoundInfo);
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
                                var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                                string subject = "Kết quả vòng Phỏng vấn";
                                string extraMessage = "Rất tiếc bạn chưa vượt qua vòng Phỏng vấn.";
                                string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                                body = body.Replace("{FullName}", user.FullName)
                                           .Replace("{Result}", "Rớt (FAIL)")
                                           .Replace("{RoundName}", "Phỏng vấn")
                                           .Replace("{CampaignName}", roundActivity.Round?.CampaignRound?.Campaign?.CampaignName ?? "")
                                           .Replace("{ExtraMessage}", extraMessage)
                                           .Replace("{NextRoundInfo}", "");
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
                        nextRoundInfo = $"Vòng tiếp theo: <strong>Final</strong><br>Thời gian: <strong>{finalStartDate}</strong> - <strong>{finalEndDate}</strong>";
                    }

                    await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);

                    try
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(roundActivity.UserId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                            string subject = "Kết quả vòng Phỏng vấn";
                            string extraMessage = "Chúc mừng bạn đã vượt qua vòng Phỏng vấn.";
                            string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                            body = body.Replace("{FullName}", user.FullName)
                                       .Replace("{Result}", "Đậu (PASS)")
                                       .Replace("{RoundName}", "Phỏng vấn")
                                       .Replace("{CampaignName}", roundActivity.Round?.CampaignRound?.Campaign?.CampaignName ?? "")
                                       .Replace("{ExtraMessage}", extraMessage)
                                       .Replace("{NextRoundInfo}", nextRoundInfo);
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
                            var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
                            string subject = "Kết quả vòng Phỏng vấn";
                            string extraMessage = "Rất tiếc bạn chưa vượt qua vòng Phỏng vấn.";
                            string body = await System.IO.File.ReadAllTextAsync(templatePath, cancellationToken);
                            body = body.Replace("{FullName}", user.FullName)
                                       .Replace("{Result}", "Rớt (FAIL)")
                                       .Replace("{RoundName}", "Phỏng vấn")
                                       .Replace("{CampaignName}", roundActivity.Round?.CampaignRound?.Campaign?.CampaignName ?? "")
                                       .Replace("{ExtraMessage}", extraMessage)
                                       .Replace("{NextRoundInfo}", "");
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
    }
}
