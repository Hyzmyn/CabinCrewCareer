using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestQuestions.Queries.GetTestQuestionsForExam
{
    public class GetTestQuestionsForExamQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<GetTestQuestionsForExamQuery, Response<GetTestQuestionsForExamResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<GetTestQuestionsForExamResponse>> Handle(GetTestQuestionsForExamQuery request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new GetTestQuestionsForExamQueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.GET_UNSUCC, errorMessages);
            }

            // Get current user from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            // Get user and check role (must be CabinCrew or Candidate)
            var user = await _unitOfWork.Users.GetByIdAsync(int.Parse(currentUserId));
            if (user == null)
            {
                throw new ApiException(ResponseCode.USER_NOT_FOUND);
            }

            // Only CabinCrew and Candidate can take exams
            if (user.RoleId != (int)AvailableRole.CabinCrew && user.RoleId != (int)AvailableRole.Candidate)
            {
                errorMessages.Add("Only Cabin Crew and Candidate can access exam questions");
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

            // Verify join code
            if (!testEntity.JoinCode.Equals(request.JoinCode, StringComparison.OrdinalIgnoreCase))
            {
                errorMessages.Add("Invalid join code for this test");
                throw new ValidationException(ResponseCode.GET_UNSUCC, errorMessages);
            }

            // Prepare questions list
            List<ExamQuestionDto> questions = [];

            foreach (var question in testEntity.TestQuestions.OrderBy(q => q.OrderNumber))
            {
                var questionDto = new ExamQuestionDto
                {
                    QuestionId = question.QuestionId,
                    QuestionContent = question.QuestionContent,
                    Score = question.Score,
                    OrderNumber = question.OrderNumber
                };

                // Logic: Show options only for EnglishListening and Practical test types
                // BUT DO NOT include IsCorrect flag (for security during exam)
                if (testEntity.TestType == TestType.EnglishListening || 
                    testEntity.TestType == TestType.Practical)
                {
                    questionDto.Options = question.TestOptions
                        .Select(o => new ExamOptionDto
                        {
                            OptionId = o.OptionId,
                            OptionContent = o.OptionContent
                            // IsCorrect is intentionally NOT included
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
            var response = new GetTestQuestionsForExamResponse
            {
                TestId = testEntity.TestId,
                TestName = testEntity.TestName,
                TestType = testEntity.TestType.ToString(),
                MaxScore = testEntity.MaxScore,
                DurationInMinutes = testEntity.DurationInMinutes,
                AudioFileURL = testEntity.TestType == TestType.EnglishListening ? testEntity.AudioFileURL : null,
                TotalQuestions = questions.Count,
                Questions = questions
            };

            return new Response<GetTestQuestionsForExamResponse>(
                data: response,
                code: (int)ResponseCode.GET_SUCCESS,
                message: $"Retrieved {questions.Count} exam questions successfully");
        }
    }
}
