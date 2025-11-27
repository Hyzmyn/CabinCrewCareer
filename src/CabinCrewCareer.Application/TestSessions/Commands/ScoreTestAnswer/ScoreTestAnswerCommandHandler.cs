using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
                includeFunc: query => query.Include(ts => ts.Test)
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
                int maxScore = 0;
                if (testAnswer.SelectedOption?.TestQuestion != null)
                {
                    maxScore = testAnswer.SelectedOption.TestQuestion.Score;
                }

                // Validate score does not exceed max score of the question
                if (maxScore > 0 && calculatedScore > maxScore)
                {
                    errorMessages.Add($"Total score ({calculatedScore}) for answer {testAnswer.AnswerId} cannot exceed the maximum score ({maxScore}) for this question.");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                // Update test answer score and correctness
                testAnswer.Score = calculatedScore;
                testAnswer.IsCorrect = answerScore.IsCorrect;

                // Add to total score
                totalScore += calculatedScore;

                await _unitOfWork.TestAnswers.UpdateAsync(testAnswer);
            }

            // Update test session total score
            testSession.TotalScore = totalScore;
            await _unitOfWork.TestSessions.UpdateAsync(testSession);

            // Save all changes
            await _unitOfWork.CompleteAsync();

            return new Response<string>
            {
                Data = $"Test answers scored successfully. Total answers: {testAnswersList.Count}, Total score: {totalScore}",
                Message = ResponseCode.SUCCESS.ToString()
            };
        }
    }
}
