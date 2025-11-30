using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetTestAnswersWithCriteria
{
    public class GetTestAnswersWithCriteriaQueryHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<GetTestAnswersWithCriteriaQuery, Response<List<TestAnswerWithCriteriaResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<List<TestAnswerWithCriteriaResponse>>> Handle(GetTestAnswersWithCriteriaQuery request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate test session exists
            var testSession = await _unitOfWork.TestSessions.GetByIdAsync(request.TestSessionId);
            if (testSession == null)
            {
                errorMessages.Add("Test session not found.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Get all test answers with related data
            var testAnswers = await _unitOfWork.TestAnswers.GetAllAsync(
                expression: ta => ta.TestSessionId == request.TestSessionId,
                includeFunc: query => query
                    .Include(ta => ta.SelectedOption)
                        .ThenInclude(so => so.TestQuestion)
            );

            var testAnswersList = testAnswers.ToList();

            if (!testAnswersList.Any())
            {
                return new Response<List<TestAnswerWithCriteriaResponse>>
                {
                    Code = (int)ResponseCode.SUCCESS,
                    Data = new List<TestAnswerWithCriteriaResponse>(),
                    Message = "No test answers found for this test session."
                };
            }

            // Map to response
            var response = testAnswersList.Select(ta => MapToResponse(ta)).ToList();

            return new Response<List<TestAnswerWithCriteriaResponse>>
            {
                Code = (int)ResponseCode.SUCCESS,
                Data = response,
                Message = "Test answers retrieved successfully."
            };
        }

        private TestAnswerWithCriteriaResponse MapToResponse(Domain.Entities.TestAnswer testAnswer)
        {
            var response = new TestAnswerWithCriteriaResponse
            {
                AnswerId = testAnswer.AnswerId,
                Score = testAnswer.Score,
                IsCorrect = testAnswer.IsCorrect,
                EssayAnswer = testAnswer.EssayAnswer,
                AnswerAudioFileURL = testAnswer.AnswerAudioFileURL,
                SelectedOptionId = testAnswer.SelectedOptionId
            };

            // Add question info if available
            if (testAnswer.SelectedOption?.TestQuestion != null)
            {
                response.Question = new QuestionInfo
                {
                    QuestionId = testAnswer.SelectedOption.TestQuestion.QuestionId,
                    QuestionContent = testAnswer.SelectedOption.TestQuestion.QuestionContent,
                    OrderNumber = testAnswer.SelectedOption.TestQuestion.OrderNumber,
                    MaxScore = testAnswer.SelectedOption.TestQuestion.Score
                };
            }

            // Parse EssayAnswer JSON to extract criteria scores and student answer
            if (!string.IsNullOrEmpty(testAnswer.EssayAnswer))
            {
                try
                {
                    using var doc = JsonDocument.Parse(testAnswer.EssayAnswer);
                    var root = doc.RootElement;

                    // Try to get criteriaScores
                    if (root.TryGetProperty("criteriaScores", out var criteriaScoresElement))
                    {
                        response.CriteriaScores = JsonSerializer.Deserialize<Dictionary<string, int>>(
                            criteriaScoresElement.GetRawText());
                    }

                    // Try to get studentAnswer
                    if (root.TryGetProperty("studentAnswer", out var studentAnswerElement))
                    {
                        response.StudentAnswer = studentAnswerElement.GetString();
                    }
                }
                catch (JsonException)
                {
                    // If EssayAnswer is not JSON, treat it as plain student answer
                    response.StudentAnswer = testAnswer.EssayAnswer;
                }
            }

            return response;
        }
    }
}
