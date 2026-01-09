using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Models.Requests;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.RoundActivities.Commands.MoveToInterviewRound
{
    public class MoveToInterviewRoundCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService) : IRequestHandler<MoveToInterviewRoundCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmailService _emailService = emailService;

        public async Task<Response<string>> Handle(MoveToInterviewRoundCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // 1. Get Round with Test information
            var roundQuery = await _unitOfWork.Rounds.GetAsync(
                expression: r => r.RoundId == request.RoundId,
                includeFunc: query => query
                    .Include(r => r.Test)
                    .Include(r => r.RoundActivities)
            );

            var round = await roundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (round == null)
            {
                errorMessages.Add("Round not found.");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // 2. Validate round is a test round
            var validTestRoundTypes = new[] { RoundType.EnglishListeningTest, RoundType.EnglishSpeakingTest, RoundType.PracticalTest };
            if (!validTestRoundTypes.Contains(round.RoundType))
            {
                errorMessages.Add($"This round is not a test round. Current round type: {round.RoundType}");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            if (round.Test == null)
            {
                errorMessages.Add("Test not found for this round.");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // 3. Get all RoundActivities with status Ongoing
            var roundActivities = round.RoundActivities
                .Where(ra => ra.Status == ActivityStatus.Ongoing)
                .ToList();

            if (!roundActivities.Any())
            {
                errorMessages.Add("No ongoing round activities found for this round.");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // 4. Get Interview Round for later
            var interviewRoundQuery = await _unitOfWork.Rounds.GetAsync(
                expression: x => x.CampaignRoundId == round.CampaignRoundId
                              && x.RoundType == RoundType.Interview);

            var interviewRound = await interviewRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

            // 5. Determine if this is English test (need to check both Listening and Speaking)
            bool isEnglishTest = round.RoundType == RoundType.EnglishListeningTest || round.RoundType == RoundType.EnglishSpeakingTest;

            // 6. Process each RoundActivity
            int totalProcessed = 0;
            int totalPassed = 0;
            int totalFailed = 0;
            List<string> detailResults = new();

            foreach (var roundActivity in roundActivities)
            {
                totalProcessed++;

                // Get all test sessions for this user in this test
                var testSessions = await _unitOfWork.TestSessions.GetAllAsync(
                    expression: ts => ts.UserId == roundActivity.UserId
                                    && ts.TestId == round.TestId,
                    includeFunc: query => query.Include(ts => ts.Test)
                );

                if (!testSessions.Any())
                {
                    detailResults.Add($"User {roundActivity.UserId}: No test sessions found - SKIPPED");
                    continue;
                }

                // Check if all sessions are finalized (Status = true)
                var unfinalizedSessions = testSessions.Where(ts => ts.Status == false).ToList();
                if (unfinalizedSessions.Any())
                {
                    detailResults.Add($"User {roundActivity.UserId}: {unfinalizedSessions.Count} test session(s) not finalized - SKIPPED");
                    continue;
                }

                // Check if all test sessions meet benchmark threshold from database
                bool allSessionsPassed = true;
                var maxScore = round.Test.MaxScore;
                int benchmarkPercentage = round.RoundType switch
                {
                    RoundType.EnglishListeningTest => await _unitOfWork.RoundConfigurations.EnglishListeningTestBenchmark(),
                    RoundType.EnglishSpeakingTest => await _unitOfWork.RoundConfigurations.EnglishSpeakingTestBenchmark(),
                    RoundType.PracticalTest => await _unitOfWork.RoundConfigurations.PracticalTestBenchmark(),
                    _ => 0
                };
                var requiredScore = (maxScore * benchmarkPercentage) / 100;
                List<string> sessionScores = new();

                foreach (var session in testSessions)
                {
                    var isPassed = session.TotalScore >= requiredScore;
                    sessionScores.Add($"{session.TotalScore}/{maxScore}");

                    if (!isPassed)
                    {
                        allSessionsPassed = false;
                    }
                }

                // For English tests, need to check BOTH Listening and Speaking rounds
                bool canMoveToInterview = false;

                if (isEnglishTest && allSessionsPassed)
                {
                    // Check the other English round (Listening or Speaking)
                    var otherEnglishRoundType = round.RoundType == RoundType.EnglishListeningTest
                        ? RoundType.EnglishSpeakingTest
                        : RoundType.EnglishListeningTest;

                    var otherEnglishRoundQuery = await _unitOfWork.Rounds.GetAsync(
                        expression: r => r.CampaignRoundId == round.CampaignRoundId
                                      && r.RoundType == otherEnglishRoundType,
                        includeFunc: query => query.Include(r => r.RoundActivities)
                    );

                    var otherEnglishRound = await otherEnglishRoundQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (otherEnglishRound != null)
                    {
                        // Check if user has Passed status in the other English round
                        var otherRoundActivity = otherEnglishRound.RoundActivities
                            .FirstOrDefault(ra => ra.UserId == roundActivity.UserId);

                        if (otherRoundActivity != null && otherRoundActivity.Status == ActivityStatus.Passed)
                        {
                            canMoveToInterview = true;
                            detailResults.Add($"User {roundActivity.UserId}: PASSED ({string.Join(", ", sessionScores)}) - Both English tests passed, moved to Interview");
                        }
                        else
                        {
                            detailResults.Add($"User {roundActivity.UserId}: PASSED ({string.Join(", ", sessionScores)}) - Waiting for other English test");
                        }
                    }
                    else
                    {
                        detailResults.Add($"User {roundActivity.UserId}: PASSED ({string.Join(", ", sessionScores)}) - Other English round not found");
                    }
                }
                else if (round.RoundType == RoundType.PracticalTest && allSessionsPassed)
                {
                    // Practical test: Pass immediately
                    canMoveToInterview = true;
                    detailResults.Add($"User {roundActivity.UserId}: PASSED ({string.Join(", ", sessionScores)}) - Moved to Interview");
                }
                else if (!allSessionsPassed)
                {
                    detailResults.Add($"User {roundActivity.UserId}: FAILED ({string.Join(", ", sessionScores)}) - Below benchmark threshold");
                }

                // Update RoundActivity status
                if (allSessionsPassed)
                {
                    roundActivity.Status = ActivityStatus.Passed;
                    totalPassed++;

                    // Create Interview RoundActivity only if conditions are met
                    if (canMoveToInterview && interviewRound != null)
                    {
                        // Check if interview round activity already exists
                        var existingInterviewActivityQuery = await _unitOfWork.RoundActivites.GetAsync(
                            expression: x => x.RoundId == interviewRound.RoundId
                                          && x.UserId == roundActivity.UserId);

                        var existingInterviewActivity = await existingInterviewActivityQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if (existingInterviewActivity == null)
                        {
                            // Create new Interview RoundActivity
                            await _unitOfWork.RoundActivites.AddAsync(new RoundActivity
                            {
                                RoundId = interviewRound.RoundId,
                                UserId = roundActivity.UserId,
                                Status = ActivityStatus.Ongoing,
                                ApplicationSnapshotId = roundActivity.ApplicationSnapshotId
                            });
                        }
                        else if (existingInterviewActivity.ApplicationSnapshotId == null)
                        {
                            // Update existing Interview RoundActivity with ApplicationSnapshotId if missing
                            existingInterviewActivity.ApplicationSnapshotId = roundActivity.ApplicationSnapshotId;
                            await _unitOfWork.RoundActivites.UpdateAsync(existingInterviewActivity);
                        }
                    }
                }
                else
                {
                    roundActivity.Status = ActivityStatus.Failed;
                    totalFailed++;

                    // Update UserHistory to NotCompleted
                    var userHistoryQuery = await _unitOfWork.UserHistories.GetAsync(
                        expression: x => x.UserId == roundActivity.UserId && x.Type == HistoryType.Ongoing
                    );
                    var userHistory = await userHistoryQuery.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    if (userHistory is not null)
                    {
                        userHistory.Type = HistoryType.NotCompleted;
                        await _unitOfWork.UserHistories.UpdateAsync(userHistory);
                    }
                }

                await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);

                // Send email notification (only include next round if user can move to interview)
                await SendResultEmailAsync(roundActivity, allSessionsPassed, canMoveToInterview, round, interviewRound, cancellationToken);
            }

            await _unitOfWork.CompleteAsync();

            // 7. Build response message
            string testTypeInfo = isEnglishTest
                ? "English Test Round (Both Listening and Speaking must pass)"
                : "Practical Test Round";

            string summaryMessage = $"Processed {totalProcessed} round activities in {testTypeInfo}:\n" +
                                   $"- Passed: {totalPassed}\n" +
                                   $"- Failed: {totalFailed}\n";

            if (interviewRound == null && totalPassed > 0)
            {
                summaryMessage += "\nWarning: Interview round not found. Activities marked as Passed but no interview round created.\n";
            }

            string detailMessage = string.Join("\n", detailResults);

            return new Response<string>(
                code: (int)ResponseCode.SUCCESS,
                data: $"Passed: {totalPassed}, Failed: {totalFailed}",
                message: $"{summaryMessage}\nDetails:\n{detailMessage}");
        }

        private async Task SendResultEmailAsync(
            RoundActivity roundActivity,
            bool isPassed,
            bool canMoveToInterview,
            Round round,
            Round? nextRound,
            CancellationToken cancellationToken)
        {
            if (roundActivity.UserId == null) return;

            var user = await _unitOfWork.Users.GetByIdAsync(roundActivity.UserId.Value);
            if (user == null || string.IsNullOrEmpty(user.Email)) return;

            // Get campaign name
            var roundWithCampaign = await _unitOfWork.Rounds.GetAsync(
                expression: r => r.RoundId == round.RoundId,
                includeFunc: q => q.Include(r => r.CampaignRound).ThenInclude(cr => cr.Campaign)
            );
            var roundData = await roundWithCampaign.FirstOrDefaultAsync(cancellationToken);
            string campaignName = roundData?.CampaignRound?.Campaign?.CampaignName ?? "";

            // Determine round name
            string roundName = round.RoundType switch
            {
                RoundType.EnglishListeningTest => "English Listening Test",
                RoundType.EnglishSpeakingTest => "English Speaking Test",
                RoundType.PracticalTest => "Practical Test",
                _ => "Test"
            };

            string resultText = isPassed ? "PASSED" : "FAILED";

            // For English tests, add note if waiting for other test
            bool isEnglishTest = round.RoundType == RoundType.EnglishListeningTest || round.RoundType == RoundType.EnglishSpeakingTest;
            string extraMessage;
            if (isPassed && isEnglishTest && !canMoveToInterview)
            {
                extraMessage = $"Congratulations! You have passed the {roundName}. You need to complete both English rounds to advance to the interview round.";
            }
            else if (isPassed)
            {
                extraMessage = $"Congratulations! You have passed the {roundName}.";
            }
            else
            {
                extraMessage = $"Unfortunately! You did not pass the {roundName}.";
            }

            // Build next round info - Only show if user can actually move to next round
            string nextRoundInfo = "";
            if (isPassed && canMoveToInterview && nextRound != null)
            {
                string nextRoundName = nextRound.RoundType switch
                {
                    RoundType.Interview => "Interview",
                    RoundType.Final => "Final",
                    _ => "Next round"
                };

                string startDate = nextRound.StartDate?.ToString("dd/MM/yyyy HH:mm") ?? "Undefined";
                nextRoundInfo = $"<p><strong>Next round:</strong> {nextRoundName}</p>" +
                               $"<p><strong>Start date:</strong> {startDate}</p>";
            }

            // Read template and replace placeholders
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Common", "Templates", "TestResultTemplate.html");
            string templateBody = await File.ReadAllTextAsync(templatePath, cancellationToken);
            
            // Get Location Info
            string locationInfo = await GetLocationInfoAsync(round, cancellationToken);
            
            templateBody = templateBody
                .Replace("{FullName}", user.FullName ?? "")
                .Replace("{Result}", resultText)
                .Replace("{RoundName}", roundName)
                .Replace("{CampaignName}", campaignName)
                .Replace("{ExtraMessage}", extraMessage)
                .Replace("{Round}", "Test")
                .Replace("{NextRoundInfo}", nextRoundInfo)
                .Replace("{LocationInfo}", locationInfo);

            var mailRequest = new MailRequest
            {
                ToEmail = user.Email,
                Subject = $"{roundName} result - Cabin Crew Career",
                Body = templateBody
            };

            await _emailService.SendEmailAsync(mailRequest);
        }

        private async Task<string> GetLocationInfoAsync(Round round, CancellationToken cancellationToken)
        {
            string locationInfo = "";
            if (round?.CampaignRound?.Campaign != null)
            {
                var campaignType = round.CampaignRound.Campaign.CampaignType;
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
                    locationInfo = $"<p><strong>📍 Location:</strong> {fullAddress}</p>";
                }
            }
            return locationInfo;
        }
    }
}
