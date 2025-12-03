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

namespace CabinCrewCareer.Application.EvaluationForms.Commands.CreateAppearanceEvaluationForm
{
    public class CreateAppearanceEvaluationFormCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IEmailService emailService) : IRequestHandler<CreateAppearanceEvaluationFormCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
        private readonly IEmailService _emailService = emailService;

        public async Task<Response<string>> Handle(CreateAppearanceEvaluationFormCommand request, CancellationToken cancellationToken)
        {
            int count = 0;
            string resultText = "Không đạt";
            string listeningJoinCode = "";
            string speakingJoinCode = "";
            string campaignName = "";
            string extraMessage = "";

            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            var evaluationQuery = await _unitOfWork.EvaluationForms.GetAsync(expression: x => x.ActivityId == request.ActivityId);

            var evaluation = await evaluationQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (evaluation is not null) throw new ApiException(ResponseCode.CANDIDATE_HAS_BEEN_EVALUATED);

            EvaluationForm form = new()
            {
                ActivityId = request.ActivityId,
                Comment = request.Comment,
                ExaminerId = Int32.Parse(currentUserId),
                Type = EvaluationType.Appearance,
                EvaluatedDate = _dateTimeProvider.UtcNow
            };

            foreach (var item in request.Choices)
            {
                form.AppearanceResults.Add(new AppearanceResult
                {
                    IsPassed = item.IsPassed,
                    ScoringCriteriaItemId = item.ScoringCriteriaItemId
                });

                if (item.IsPassed) count++;
            }

            if (count > 7) form.IsPassed = true;
            // Lấy thông tin user để gửi email (sẽ lấy sau khi roundActivity đã khởi tạo)

            var roundActivityQuery = await _unitOfWork.RoundActivites.GetAsync(
                expression: x => x.ActivityId == request.ActivityId,
                includeFunc: x => x
                    .Include(x => x.Round)
                        .ThenInclude(r => r.CampaignRound)
                            .ThenInclude(cr => cr.Campaign));

            var roundActivity = await roundActivityQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (roundActivity is not null)
            {
                if (form.IsPassed)
                {
                    roundActivity.Status = ActivityStatus.Passed;
                    resultText = "Đạt";
                    extraMessage = "Chúc mừng bạn đã vượt qua vòng Ngoại hình.";

                    // Lấy tên campaign
                    campaignName = roundActivity.Round?.CampaignRound?.Campaign?.CampaignName ?? "";

                    // Lấy mã join code vòng nghe
                    if (roundActivity.Round != null)
                    {
                        var listeningRoundQuery = await _unitOfWork.Rounds.GetAsync(
                            expression: x => x.CampaignRoundId == roundActivity.Round.CampaignRoundId && x.RoundType == RoundType.EnglishListeningTest,
                            includeFunc: x => x.Include(r => r.CampaignRound).ThenInclude(cr => cr.Campaign));
                        var listeningRound = await listeningRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                        if (listeningRound is not null)
                        {
                            await _unitOfWork.RoundActivites.AddAsync(new RoundActivity { RoundId = listeningRound.RoundId, UserId = roundActivity.UserId, Status = ActivityStatus.Ongoing, ApplicationSnapshotId = roundActivity.ApplicationSnapshotId });
                            if (listeningRound.TestId.HasValue)
                            {
                                var test = await _unitOfWork.Tests.GetByIdAsync(listeningRound.TestId.Value);
                                listeningJoinCode = test?.JoinCode ?? "";
                            }
                        }

                        // Lấy mã join code vòng nói
                        var speakingRoundQuery = await _unitOfWork.Rounds.GetAsync(
                            expression: x => x.CampaignRoundId == roundActivity.Round.CampaignRoundId && x.RoundType == RoundType.EnglishSpeakingTest,
                            includeFunc: x => x.Include(r => r.CampaignRound).ThenInclude(cr => cr.Campaign));
                        var speakingRound = await speakingRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                        if (speakingRound is not null)
                        {
                            await _unitOfWork.RoundActivites.AddAsync(new RoundActivity { RoundId = speakingRound.RoundId, UserId = roundActivity.UserId, Status = ActivityStatus.Ongoing, ApplicationSnapshotId = roundActivity.ApplicationSnapshotId });
                            if (speakingRound.TestId.HasValue)
                            {
                                var test = await _unitOfWork.Tests.GetByIdAsync(speakingRound.TestId.Value);
                                speakingJoinCode = test?.JoinCode ?? "";
                            }
                        }
                    }
                }
                else
                {
                    roundActivity.Status = ActivityStatus.Failed;
                    resultText = "Không đạt";
                    extraMessage = "Rất tiếc bạn chưa vượt qua vòng Ngoại hình.";

                    var history = await _unitOfWork.UserHistories.GetAsync(x => x.UserId == (int)roundActivity.UserId && x.Type == HistoryType.Ongoing);
                    var rehe = await history.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    rehe.Type = HistoryType.NotCompleted;
                    await _unitOfWork.UserHistories.UpdateAsync(rehe);
                }
                await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);

                // Gửi email kết quả cho user
                if (roundActivity.UserId != null)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(roundActivity.UserId);
                    if (user is not null && !string.IsNullOrEmpty(user.Email))
                    {
                        var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "AppearanceResultTemplate.html");
                        string templateBody = await File.ReadAllTextAsync(templatePath, cancellationToken);
                        templateBody = templateBody.Replace("{FullName}", user.FullName ?? "")
                                                 .Replace("{Result}", resultText)
                                                 .Replace("{CampaignName}", campaignName)
                                                 .Replace("{ListeningJoinCode}", listeningJoinCode)
                                                 .Replace("{SpeakingJoinCode}", speakingJoinCode);
                        var mailRequest = new CabinCrewCareer.Application.Common.Models.Requests.MailRequest
                        {
                            ToEmail = user.Email,
                            Subject = "Kết quả vòng Ngoại hình Cabin Crew",
                            Body = templateBody
                        };
                        await _emailService.SendEmailAsync(mailRequest);
                    }
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
