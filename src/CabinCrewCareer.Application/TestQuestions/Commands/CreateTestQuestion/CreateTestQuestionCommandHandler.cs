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

namespace CabinCrewCareer.Application.TestQuestions.Commands.CreateTestQuestion
{
    public class CreateTestQuestionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<CreateTestQuestionCommand, Response<CreateTestQuestionResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<CreateTestQuestionResponse>> Handle(CreateTestQuestionCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new CreateTestQuestionCommandValidator();
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

            // Validate Options based on TestType
            if (test.TestType == TestType.EnglishListening || test.TestType == TestType.Practical)
            {
                // EnglishListening and Practical MUST have options (4-6)
                if (request.Options == null || !request.Options.Any())
                {
                    errorMessages.Add("Options are required for EnglishListening and Practical test types");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                if (request.Options.Count < 4)
                {
                    errorMessages.Add("At least 4 options are required for multiple choice questions");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                if (request.Options.Count > 6)
                {
                    errorMessages.Add("Maximum 6 options are allowed");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                // Validate exactly one correct answer
                if (request.Options.Count(o => o.IsCorrect) != 1)
                {
                    errorMessages.Add("Exactly one option must be marked as correct");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }
            }
            else if (test.TestType == TestType.EnglishSpeaking)
            {
                // EnglishSpeaking should NOT have options
                if (request.Options != null && request.Options.Any())
                {
                    errorMessages.Add("EnglishSpeaking test type should not have options");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }
            }

            // Create TestQuestion
            var testQuestion = new TestQuestion
            {
                QuestionContent = request.QuestionContent,
                Score = request.Score,
                OrderNumber = request.OrderNumber,
                TestId = request.TestId
            };

            await _unitOfWork.TestQuestions.AddAsync(testQuestion);
            await _unitOfWork.CompleteAsync();

            // Create TestOptions if provided
            List<TestOptionResponse> optionResponses = [];
            if (request.Options != null && request.Options.Any())
            {
                foreach (var optionDto in request.Options)
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

            // Create response
            var response = new CreateTestQuestionResponse
            {
                QuestionId = testQuestion.QuestionId,
                QuestionContent = testQuestion.QuestionContent,
                Score = testQuestion.Score,
                OrderNumber = testQuestion.OrderNumber,
                TestId = test.TestId,
                TestName = test.TestName,
                TestType = test.TestType.ToString(),
                Options = optionResponses
            };

            return new Response<CreateTestQuestionResponse>(
                data: response,
                code: (int)ResponseCode.CREATED_SUCCESS,
                message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
