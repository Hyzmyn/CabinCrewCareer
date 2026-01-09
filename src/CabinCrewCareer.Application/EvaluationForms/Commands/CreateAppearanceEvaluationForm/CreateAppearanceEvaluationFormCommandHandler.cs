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
            string listeningStartDate = "";
            string listeningEndDate = "";
            string speakingStartDate = "";
            string speakingEndDate = "";

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

            if (count > await _unitOfWork.RoundConfigurations.RecruitmentAppearanceBenchmark()) form.IsPassed = true;
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
                    resultText = "PASSED";
                    extraMessage = "Congratulations! You have passed the Appearance round.";

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
                            listeningStartDate = listeningRound.StartDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                            listeningEndDate = listeningRound.EndDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
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
                            speakingStartDate = speakingRound.StartDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                            speakingEndDate = speakingRound.EndDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                        }
                    }
                }
                else
                {
                    roundActivity.Status = ActivityStatus.Failed;
                    resultText = "FAILED";
                    extraMessage = "Unfortunately! You did not pass the Appearance round.";

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
                        // Get Location based on CampaignType
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
                                locationInfo = $"<div style=\"background: #f8f9fa; padding: 15px; margin: 15px 0; border-radius: 5px;\">" +
                                             $"<h4 style=\"margin-top: 0; color: #555;\">📍 Interview Location</h4>" +
                                             $"<p>{fullAddress}</p>" +
                                             $"</div>";
                            }
                        }
                        
                        var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "AppearanceResultTemplate.html");
                        string templateBody = await File.ReadAllTextAsync(templatePath, cancellationToken);
                        templateBody = templateBody.Replace("{FullName}", user.FullName ?? "")
                                                 .Replace("{Result}", resultText)
                                                 .Replace("{CampaignName}", campaignName)
                                                 //.Replace("{ListeningJoinCode}", listeningJoinCode)
                                                 .Replace("{ListeningStartDate}", listeningStartDate)
                                                 .Replace("{ListeningEndDate}", listeningEndDate)
                                                 //.Replace("{SpeakingJoinCode}", speakingJoinCode)
                                                 .Replace("{SpeakingStartDate}", speakingStartDate)
                                                 .Replace("{SpeakingEndDate}", speakingEndDate)
                                                 .Replace("{LocationInfo}", locationInfo);
                        var mailRequest = new CabinCrewCareer.Application.Common.Models.Requests.MailRequest
                        {
                            ToEmail = user.Email,
                            Subject = "Appearance result - Cabin Crew Career",
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
