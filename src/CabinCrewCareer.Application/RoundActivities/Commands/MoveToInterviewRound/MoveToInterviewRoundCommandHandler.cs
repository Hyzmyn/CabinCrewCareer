using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.RoundActivities.Commands.MoveToInterviewRound
{
    public class MoveToInterviewRoundCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<MoveToInterviewRoundCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

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

                // Check if all test sessions meet 80% threshold
                bool allSessionsPassed = true;
                var maxScore = round.Test.MaxScore;
                var requiredScore = maxScore * 0.8;
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
                    detailResults.Add($"User {roundActivity.UserId}: FAILED ({string.Join(", ", sessionScores)}) - Below 80% threshold");
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
                        var existingInterviewActivity = await _unitOfWork.RoundActivites.GetAsync(
                            expression: x => x.RoundId == interviewRound.RoundId 
                                          && x.UserId == roundActivity.UserId);

                        if (!await existingInterviewActivity.AnyAsync(cancellationToken: cancellationToken))
                        {
                            await _unitOfWork.RoundActivites.AddAsync(new RoundActivity
                            {
                                RoundId = interviewRound.RoundId,
                                UserId = roundActivity.UserId,
                                Status = ActivityStatus.Ongoing
                            });
                        }
                    }
                }
                else
                {
                    roundActivity.Status = ActivityStatus.Failed;
                    totalFailed++;
                }

                await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);
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
    }
}
