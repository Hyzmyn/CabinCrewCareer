using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CabinCrewCareer.Application.TestSessions.Commands.ScoreTestAnswer
{
    public class ScoreTestAnswerCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<ScoreTestAnswerCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(ScoreTestAnswerCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate test session exists
            var testSessions = await _unitOfWork.TestSessions.GetAllAsync(
                expression: ts => ts.TestSessionId == request.TestSessionId,
                includeFunc: query => query
                    .Include(ts => ts.Test)
                        .ThenInclude(t => t.Rounds)
                            .ThenInclude(r => r.RoundActivities)
                    .Include(ts => ts.User)
            );

            var testSession = testSessions.FirstOrDefault();
            if (testSession == null)
            {
                errorMessages.Add("Test session not found.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Validate that answer scores dictionary is not empty
            if (request.AnswerScores == null || request.AnswerScores.Count == 0)
            {
                errorMessages.Add("Answer scores cannot be empty.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Get all test answers for this session with related data
            var testAnswers = await _unitOfWork.TestAnswers.GetAllAsync(
                expression: ta => ta.TestSessionId == request.TestSessionId && request.AnswerScores.Keys.Contains(ta.AnswerId),
                includeFunc: query => query
                    .Include(ta => ta.SelectedOption)
                        .ThenInclude(so => so.TestQuestion)
            );

            var testAnswersList = testAnswers.ToList();

            // Validate all answers exist
            if (testAnswersList.Count != request.AnswerScores.Count)
            {
                errorMessages.Add("Some test answers not found or do not belong to this test session.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            int totalScore = 0;

            // Process each answer
            foreach (var testAnswer in testAnswersList)
            {
                var answerScore = request.AnswerScores[testAnswer.AnswerId];

                // Validate test answer has not been scored yet
                if (testAnswer.Score > 0)
                {
                    errorMessages.Add($"Test answer {testAnswer.AnswerId} has already been scored. Cannot update score.");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                // Validate that criteria scores dictionary is not empty
                if (answerScore.CriteriaScores == null || answerScore.CriteriaScores.Count == 0)
                {
                    errorMessages.Add($"Criteria scores for answer {testAnswer.AnswerId} cannot be empty.");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                // Validate all criteria scores are non-negative
                foreach (var criteriaScore in answerScore.CriteriaScores.Values)
                {
                    if (criteriaScore < 0)
                    {
                        errorMessages.Add($"All criteria scores for answer {testAnswer.AnswerId} must be non-negative.");
                        throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                    }
                }

                // Calculate total score for this answer by summing all criteria scores
                var calculatedScore = answerScore.CriteriaScores.Values.Sum();

                // Get max score for this question
                int questionMaxScore = 0;
                if (testAnswer.SelectedOption?.TestQuestion != null)
                {
                    questionMaxScore = testAnswer.SelectedOption.TestQuestion.Score;
                }

                // Validate score does not exceed max score of the question
                if (questionMaxScore > 0 && calculatedScore > questionMaxScore)
                {
                    errorMessages.Add($"Total score ({calculatedScore}) for answer {testAnswer.AnswerId} cannot exceed the maximum score ({questionMaxScore}) for this question.");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                // Save criteria scores as JSON in EssayAnswer field for later review
                var criteriaScoresJson = JsonSerializer.Serialize(answerScore.CriteriaScores, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                // Update test answer score and correctness
                testAnswer.Score = calculatedScore;
                testAnswer.IsCorrect = answerScore.IsCorrect;
                
                // Store criteria scores in EssayAnswer field
                // If EssayAnswer already has content (from student's essay), append criteria scores
                if (!string.IsNullOrEmpty(testAnswer.EssayAnswer))
                {
                    testAnswer.EssayAnswer = $"{{\"studentAnswer\":\"{testAnswer.EssayAnswer.Replace("\"", "\\\"")}\",\"criteriaScores\":{criteriaScoresJson}}}";
                }
                else
                {
                    testAnswer.EssayAnswer = $"{{\"criteriaScores\":{criteriaScoresJson}}}";
                }

                // Add to total score
                totalScore += calculatedScore;

                await _unitOfWork.TestAnswers.UpdateAsync(testAnswer);
            }

            // Update test session total score
            testSession.TotalScore = totalScore;
            await _unitOfWork.TestSessions.UpdateAsync(testSession);

            // Determine pass/fail status (80% threshold)
            var maxScore = testSession.Test.MaxScore;
            var isPassed = totalScore >= (maxScore * 0.8);

            // Find the correct round based on user's current RoundActivity
            // A user should have an Ongoing RoundActivity for the current round they're in
            Domain.Entities.Round? round = null;
            Domain.Entities.RoundActivity? roundActivity = null;

            if (testSession.UserId.HasValue)
            {
                // Get all rounds for this test with their activities
                var allRoundsForTest = await _unitOfWork.Rounds.GetAllAsync(
                    expression: r => r.TestId == testSession.TestId,
                    includeFunc: q => q.Include(r => r.RoundActivities.Where(ra => ra.UserId == testSession.UserId))
                );

                // Find the round where user has Ongoing status (current active round)
                round = allRoundsForTest.FirstOrDefault(r => 
                    r.RoundActivities.Any(ra => ra.UserId == testSession.UserId && ra.Status == ActivityStatus.Ongoing));

                if (round != null)
                {
                    roundActivity = round.RoundActivities
                        .FirstOrDefault(ra => ra.UserId == testSession.UserId && ra.Status == ActivityStatus.Ongoing);
                }
            }

            if (round != null && roundActivity != null)
            {
                // Update RoundActivity status
                roundActivity.Status = isPassed ? ActivityStatus.Passed : ActivityStatus.Failed;
                await _unitOfWork.RoundActivites.UpdateAsync(roundActivity);

                // If passed, create RoundActivity for next round (Interview)
                if (isPassed && round.CampaignRoundId.HasValue)
                {
                    // Find Interview round in the same CampaignRound
                    var interviewRoundQuery = await _unitOfWork.Rounds.GetAsync(
                        expression: r => r.CampaignRoundId == round.CampaignRoundId 
                                      && r.RoundType == RoundType.Interview);

                    var interviewRound = await interviewRoundQuery.FirstOrDefaultAsync();

                    if (interviewRound != null)
                    {
                        // Check for English tests - need both to pass before creating Interview activity
                        bool canCreateInterviewActivity = false;

                        if (testSession.Test.TestType == TestType.EnglishListening || 
                            testSession.Test.TestType == TestType.EnglishSpeaking)
                        {
                            // For English tests, check if the other English test is also passed
                            var otherEnglishRoundType = testSession.Test.TestType == TestType.EnglishListening 
                                ? RoundType.EnglishSpeakingTest 
                                : RoundType.EnglishListeningTest;

                            var otherEnglishRoundQuery = await _unitOfWork.Rounds.GetAsync(
                                expression: r => r.CampaignRoundId == round.CampaignRoundId 
                                              && r.RoundType == otherEnglishRoundType,
                                includeFunc: q => q.Include(r => r.RoundActivities
                                    .Where(ra => ra.UserId == testSession.UserId)));

                            var otherEnglishRound = await otherEnglishRoundQuery.FirstOrDefaultAsync();

                            if (otherEnglishRound != null)
                            {
                                var otherRoundActivity = otherEnglishRound.RoundActivities
                                    .FirstOrDefault(ra => ra.UserId == testSession.UserId);

                                // Only create Interview activity if other English test is also Passed
                                if (otherRoundActivity != null && otherRoundActivity.Status == ActivityStatus.Passed)
                                {
                                    canCreateInterviewActivity = true;
                                }
                            }
                        }
                        else if (testSession.Test.TestType == TestType.Practical)
                        {
                            // For Practical test, can move to Interview immediately
                            canCreateInterviewActivity = true;
                        }

                        // Create Interview RoundActivity if conditions met
                        if (canCreateInterviewActivity)
                        {
                            // Check if Interview activity already exists
                            var existingInterviewActivityQuery = await _unitOfWork.RoundActivites.GetAsync(
                                expression: ra => ra.RoundId == interviewRound.RoundId 
                                               && ra.UserId == testSession.UserId);

                            if (!await existingInterviewActivityQuery.AnyAsync())
                            {
                                await _unitOfWork.RoundActivites.AddAsync(new Domain.Entities.RoundActivity
                                {
                                    RoundId = interviewRound.RoundId,
                                    UserId = testSession.UserId,
                                    Status = ActivityStatus.Ongoing
                                });
                            }
                        }
                    }
                }

                // Handle UserHistory for failed tests
                if (!isPassed && testSession.UserId.HasValue)
                {
                    await HandleFailedTestHistory(
                        testSession.UserId.Value,
                        testSession.Test.TestType,
                        round);
                }
            }

            // Save all changes
            await _unitOfWork.CompleteAsync();

            return new Response<string>
            {
                Data = $"Test answers scored successfully. Total answers: {testAnswersList.Count}, Total score: {totalScore}/{maxScore} - {(isPassed ? "PASSED" : "FAILED")}",
                Message = ResponseCode.SUCCESS.ToString()
            };
        }

        /// <summary>
        /// Handle UserHistory creation when a test is failed
        /// </summary>
        private async Task HandleFailedTestHistory(int userId, TestType testType, Domain.Entities.Round currentRound)
        {
            var campaignRoundId = currentRound.CampaignRoundId;
            if (!campaignRoundId.HasValue)
                return;

            bool shouldCreateHistory = false;

            // For Cabin Crew (Practical Test) - only 1 round, fail immediately
            if (testType == TestType.Practical)
            {
                shouldCreateHistory = true;
            }
            // For Candidate (English Tests) - need to check both Listening and Speaking
            else if (testType == TestType.EnglishListening || testType == TestType.EnglishSpeaking)
            {
                // Get the CampaignRound with all related rounds
                var campaignRound = await _unitOfWork.CampaignRounds.GetAllAsync(
                    expression: cr => cr.CampaignRoundId == campaignRoundId.Value,
                    includeFunc: q => q.Include(cr => cr.Rounds)
                        .ThenInclude(r => r.RoundActivities.Where(ra => ra.UserId == userId))
                );

                var campaignRoundData = campaignRound.FirstOrDefault();
                if (campaignRoundData != null)
                {
                    // Find English test rounds (Listening and Speaking)
                    var englishRounds = campaignRoundData.Rounds
                        .Where(r => r.RoundType == RoundType.EnglishListeningTest || 
                                   r.RoundType == RoundType.EnglishSpeakingTest)
                        .ToList();

                    // Check if any of the English rounds is failed
                    var hasFailedRound = englishRounds
                        .SelectMany(r => r.RoundActivities)
                        .Any(ra => ra.UserId == userId && ra.Status == ActivityStatus.Failed);

                    // If any English round failed, create history
                    if (hasFailedRound)
                    {
                        shouldCreateHistory = true;
                    }
                }
            }

            // Create UserHistory if should
            if (shouldCreateHistory)
            {
                // Check if history already exists to avoid duplicates
                var existingHistory = await _unitOfWork.UserHistories.GetAllAsync(
                    expression: h => h.UserId == userId && 
                                   h.CampaignRoundId == campaignRoundId.Value &&
                                   h.Type == HistoryType.NotCompleted
                );

                if (!existingHistory.Any())
                {
                    var userHistory = new Domain.Entities.UserHistory
                    {
                        UserId = userId,
                        CampaignRoundId = campaignRoundId.Value,
                        ParticipatedDate = DateOnly.FromDateTime(DateTime.Now),
                        Type = HistoryType.NotCompleted
                    };

                    await _unitOfWork.UserHistories.AddAsync(userHistory);
                }
            }
        }
    }
}
