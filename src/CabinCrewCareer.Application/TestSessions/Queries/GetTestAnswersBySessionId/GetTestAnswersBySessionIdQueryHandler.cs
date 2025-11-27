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

            // Validate test session exists
            var testSession = await _unitOfWork.TestSessions.GetByIdAsync(request.TestSessionId);
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
            var testAnswersList = testAnswers.ToList();

            // Map to DTOs
            var answerDtos = testAnswersList.Select(ta =>
            {
                var selectedOption = ta.SelectedOption;
                var testQuestion = selectedOption?.TestQuestion;
                
                return new TestAnswerDto
                {
                    AnswerId = ta.AnswerId,
                    QuestionId = selectedOption != null ? selectedOption.QuestionId ?? 0 : 0,
                    SelectedOptionId = ta.SelectedOptionId,
                    SelectedOptionContent = selectedOption != null ? selectedOption.OptionContent : string.Empty,
                    EssayAnswer = ta.EssayAnswer,
                    AnswerAudioFileURL = ta.AnswerAudioFileURL,
                    IsCorrect = ta.IsCorrect,
                    Score = ta.Score,
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
