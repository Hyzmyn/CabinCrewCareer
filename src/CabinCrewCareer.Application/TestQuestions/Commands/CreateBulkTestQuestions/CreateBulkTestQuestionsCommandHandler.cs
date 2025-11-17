using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestQuestions.Commands.CreateBulkTestQuestions
{
    public class CreateBulkTestQuestionsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<CreateBulkTestQuestionsCommand, Response<CreateBulkTestQuestionsResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<CreateBulkTestQuestionsResponse>> Handle(CreateBulkTestQuestionsCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new CreateBulkTestQuestionsCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Get current user from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            // Get user and check role
            var user = await _unitOfWork.Users.GetByIdAsync(int.Parse(currentUserId));
            if (user == null)
            {
                throw new ApiException(ResponseCode.USER_NOT_FOUND);
            }

            // Check if user is Examiner
            if (user.RoleId != (int)AvailableRole.Examiner)
            {
                throw new ApiException(ResponseCode.UNAUTHORIZED_EXAMINER_ONLY);
            }

            // Check if test exists
            var test = await _unitOfWork.Tests.GetByIdAsync(request.TestId);
            if (test == null)
            {
                throw new ApiException(ResponseCode.TEST_NOT_FOUND);
            }

            // Validate all questions based on TestType
            foreach (var questionDto in request.Questions)
            {
                if (test.TestType == TestType.EnglishListening || test.TestType == TestType.Practical)
                {
                    // EnglishListening and Practical MUST have options (4-6)
                    if (questionDto.Options == null || !questionDto.Options.Any())
                    {
                        errorMessages.Add($"Question '{questionDto.QuestionContent}': Options are required for {test.TestType} test type");
                    }
                    else if (questionDto.Options.Count < 4)
                    {
                        errorMessages.Add($"Question '{questionDto.QuestionContent}': At least 4 options are required");
                    }
                    else if (questionDto.Options.Count > 6)
                    {
                        errorMessages.Add($"Question '{questionDto.QuestionContent}': Maximum 6 options are allowed");
                    }
                    else if (questionDto.Options.Count(o => o.IsCorrect) != 1)
                    {
                        errorMessages.Add($"Question '{questionDto.QuestionContent}': Exactly one option must be marked as correct");
                    }
                }
                else if (test.TestType == TestType.EnglishSpeaking)
                {
                    // EnglishSpeaking should NOT have options
                    if (questionDto.Options != null && questionDto.Options.Any())
                    {
                        errorMessages.Add($"Question '{questionDto.QuestionContent}': {test.TestType} test type should not have options");
                    }
                }
            }

            if (errorMessages.Any())
            {
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Create all questions
            List<QuestionCreatedDto> createdQuestions = [];

            foreach (var questionDto in request.Questions)
            {
                // Create TestQuestion
                var testQuestion = new TestQuestion
                {
                    QuestionContent = questionDto.QuestionContent,
                    Score = questionDto.Score,
                    OrderNumber = questionDto.OrderNumber,
                    TestId = request.TestId
                };

                await _unitOfWork.TestQuestions.AddAsync(testQuestion);
                await _unitOfWork.CompleteAsync();

                // Create TestOptions if provided
                List<TestOptionResponse> optionResponses = [];
                if (questionDto.Options != null && questionDto.Options.Any())
                {
                    foreach (var optionDto in questionDto.Options)
                    {
                        var testOption = new TestOption
                        {
                            OptionContent = optionDto.OptionContent,
                            IsCorrect = optionDto.IsCorrect,
                            QuestionId = testQuestion.QuestionId
                        };

                        await _unitOfWork.TestOptions.AddAsync(testOption);
                        await _unitOfWork.CompleteAsync();

                        optionResponses.Add(new TestOptionResponse
                        {
                            OptionId = testOption.OptionId,
                            OptionContent = testOption.OptionContent,
                            IsCorrect = testOption.IsCorrect
                        });
                    }
                }

                createdQuestions.Add(new QuestionCreatedDto
                {
                    QuestionId = testQuestion.QuestionId,
                    QuestionContent = testQuestion.QuestionContent,
                    Score = testQuestion.Score,
                    OrderNumber = testQuestion.OrderNumber,
                    Options = optionResponses
                });
            }

            // Create response
            var response = new CreateBulkTestQuestionsResponse
            {
                TestId = test.TestId,
                TestName = test.TestName,
                TestType = test.TestType.ToString(),
                TotalQuestionsCreated = createdQuestions.Count,
                Questions = createdQuestions
            };

            return new Response<CreateBulkTestQuestionsResponse>(
                data: response,
                code: (int)ResponseCode.CREATED_SUCCESS,
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
