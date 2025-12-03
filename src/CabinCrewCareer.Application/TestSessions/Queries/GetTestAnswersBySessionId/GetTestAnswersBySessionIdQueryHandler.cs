using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetTestAnswersBySessionId
{
    public class GetTestAnswersBySessionIdQueryHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<GetTestAnswersBySessionIdQuery, Response<List<TestAnswerDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<List<TestAnswerDto>>> Handle(GetTestAnswersBySessionIdQuery request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate test session exists and get test info
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

            // Get all test answers for this session with related data
            var testAnswers = await _unitOfWork.TestAnswers.GetAllAsync(
                expression: ta => ta.TestSessionId == request.TestSessionId,
                includeFunc: query => query
                    .Include(ta => ta.SelectedOption)
                        .ThenInclude(so => so.TestQuestion)
            );

            // Convert to list to avoid expression tree limitation
            var testAnswersList = testAnswers.OrderBy(ta => ta.AnswerId).ToList();

            // Get all questions for this test (for speaking/essay questions without SelectedOption)
            Dictionary<int, Domain.Entities.TestQuestion> questionsByIndex = new();
            if (testSession.TestId.HasValue)
            {
                var testQuestions = await _unitOfWork.TestQuestions.GetAllAsync(
                    expression: tq => tq.TestId == testSession.TestId.Value
                );
                var orderedQuestions = testQuestions.OrderBy(q => q.OrderNumber).ToList();
                
                // Map questions by their index (assuming answers are submitted in order)
                for (int i = 0; i < orderedQuestions.Count; i++)
                {
                    questionsByIndex[i] = orderedQuestions[i];
                }
            }

            // Map to DTOs
            var answerDtos = testAnswersList.Select((ta, index) =>
            {
                var selectedOption = ta.SelectedOption;
                var testQuestion = selectedOption?.TestQuestion;
                
                // If no question from SelectedOption (speaking/essay), use index to find question
                if (testQuestion == null && questionsByIndex.ContainsKey(index))
                {
                    testQuestion = questionsByIndex[index];
                }
                
                return new TestAnswerDto
                {
                    AnswerId = ta.AnswerId,
                    QuestionId = testQuestion?.QuestionId ?? (selectedOption?.QuestionId ?? 0),
                    SelectedOptionId = ta.SelectedOptionId,
                    SelectedOptionContent = selectedOption != null ? selectedOption.OptionContent : string.Empty,
                    EssayAnswer = ta.EssayAnswer,
                    AnswerAudioFileURL = ta.AnswerAudioFileURL,
                    IsCorrect = ta.IsCorrect,
                    Score = ta.Score,
                    MaxScore = testQuestion?.Score ?? 0,
                    QuestionContent = testQuestion != null ? testQuestion.QuestionContent : string.Empty
                };
            }).ToList();

            return new Response<List<TestAnswerDto>>
            {
                Data = answerDtos,
                Message = ResponseCode.SUCCESS.ToString()
            };
        }
    }
}
