using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestQuestions.Queries.GetTestQuestions
{
    public class GetTestQuestionsQueryHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<GetTestQuestionsQuery, Response<GetTestQuestionsResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<GetTestQuestionsResponse>> Handle(GetTestQuestionsQuery request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new GetTestQuestionsQueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.GET_UNSUCC, errorMessages);
            }

            // Get test with questions and options
            var tests = await _unitOfWork.Tests.GetAllAsync(
                expression: t => t.TestId == request.TestId,
                includeFunc: query => query
                    .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.TestOptions)
            );

            var testEntity = tests.FirstOrDefault();
            if (testEntity == null)
            {
                throw new ApiException(ResponseCode.TEST_NOT_FOUND);
            }

            // Prepare questions list
            List<TestQuestionDto> questions = [];

            foreach (var question in testEntity.TestQuestions.OrderBy(q => q.OrderNumber))
            {
                var questionDto = new TestQuestionDto
                {
                    QuestionId = question.QuestionId,
                    QuestionContent = question.QuestionContent,
                    Score = question.Score,
                    OrderNumber = question.OrderNumber
                };

                // Logic: Show options only for EnglishListening and Practical test types
                if (testEntity.TestType == TestType.EnglishListening || 
                    testEntity.TestType == TestType.Practical)
                {
                    questionDto.Options = question.TestOptions
                        .Select(o => new TestOptionResponse
                        {
                            OptionId = o.OptionId,
                            OptionContent = o.OptionContent,
                            IsCorrect = o.IsCorrect
                        })
                        .ToList();
                }
                else if (testEntity.TestType == TestType.EnglishSpeaking)
                {
                    // For EnglishSpeaking, set Options to null (no options needed)
                    questionDto.Options = null;
                }

                questions.Add(questionDto);
            }

            // Create response
            var response = new GetTestQuestionsResponse
            {
                TestId = testEntity.TestId,
                TestName = testEntity.TestName,
                TestType = testEntity.TestType.ToString(),
                TotalQuestions = questions.Count,
                Questions = questions
            };

            return new Response<GetTestQuestionsResponse>(
                data: response,
                code: (int)ResponseCode.GET_SUCCESS,
                message: $"Retrieved {questions.Count} questions successfully");
        }
    }
}
