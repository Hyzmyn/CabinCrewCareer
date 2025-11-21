using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Tests.Queries.GetTestById
{
    public class GetTestByIdQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetTestByIdQuery, Response<GetTestByIdResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<GetTestByIdResponse>> Handle(GetTestByIdQuery request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new GetTestByIdQueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.FAIL, errorMessages);
            }

            // Get test by id
            var test = await _unitOfWork.Tests.GetByIdAsync(request.TestId);
            if (test == null)
            {
                throw new ApiException(ResponseCode.TEST_NOT_FOUND);
            }

            // Count questions for this test
            var questions = await _unitOfWork.TestQuestions.GetAsync(q => q.TestId == test.TestId);
            var questionCount = questions.Count();

            // Create response
            var response = new GetTestByIdResponse
            {
                TestId = test.TestId,
                TestName = test.TestName,
                Purpose = test.Purpose,
                TestType = test.TestType.GetDescription(),
                MaxScore = test.MaxScore,
                DurationInMinutes = test.DurationInMinutes,
                TotalQuestions = questionCount,
                JoinCode = test.JoinCode,
                CreatedAt = test.CreatedAt?.GetFormatDateTime(),
                CreatedBy = test.CreatedBy,
                UpdatedAt = test.UpdatedAt?.GetFormatDateTime(),
                UpdatedBy = test.UpdatedBy
            };

            // Only include AudioFileURL for EnglishListening (TestType = 1)
            // For EnglishSpeaking (2) and Practical (3), set to null (will be ignored by JsonIgnore)
            if (test.TestType == TestType.EnglishListening && !string.IsNullOrEmpty(test.AudioFileURL))
            {
                response.AudioFileURL = test.AudioFileURL;
            }
            else
            {
                response.AudioFileURL = null; // Will be omitted from JSON response
            }

            return new Response<GetTestByIdResponse>(
                data: response,
                code: (int)ResponseCode.SUCCESS,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
