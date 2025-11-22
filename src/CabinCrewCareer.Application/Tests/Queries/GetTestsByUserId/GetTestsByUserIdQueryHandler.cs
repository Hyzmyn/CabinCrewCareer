using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.Tests.Queries.GetTestsByUserId
{
    public class GetTestsByUserIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<GetTestsByUserIdQuery, Response<GetTestsByUserIdResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<GetTestsByUserIdResponse>> Handle(GetTestsByUserIdQuery request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new GetTestsByUserIdQueryValidator();
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

            var userId = int.Parse(currentUserId);

            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ApiException(ResponseCode.USER_NOT_FOUND);
            }

            // Get all RoundActivities for this user
            var roundActivities = await _unitOfWork.RoundActivites.GetAllAsync(
                expression: ra => ra.UserId == userId,
                includeFunc: query => query
                    .Include(ra => ra.Round)
                    .ThenInclude(r => r.Test)
            );

            var roundActivitiesList = roundActivities.ToList();

            // Get all test sessions for this user to check completion status
            var testSessions = await _unitOfWork.TestSessions.GetAllAsync(
                expression: ts => ts.UserId == userId
            );
            var testSessionsList = testSessions.ToList();

            // Map RoundType to TestType for filtering
            var userTests = new List<UserTestDto>();

            foreach (var activity in roundActivitiesList)
            {
                if (activity.Round?.Test == null)
                    continue;

                // Logic: Map RoundType to TestType
                // Only include test if RoundType matches TestType
                var isMatch = activity.Round.RoundType switch
                {
                    RoundType.EnglishListeningTest => activity.Round.Test.TestType == TestType.EnglishListening,
                    RoundType.EnglishSpeakingTest => activity.Round.Test.TestType == TestType.EnglishSpeaking,
                    RoundType.PracticalTest => activity.Round.Test.TestType == TestType.Practical,
                    _ => false // Other RoundTypes don't have tests
                };

                if (!isMatch)
                    continue;

                // Check if user has completed this test (has at least one test session)
                var hasCompleted = testSessionsList.Any(ts => ts.TestId == activity.Round.Test.TestId);

                // Add test to list (avoid duplicates)
                if (!userTests.Any(t => t.TestId == activity.Round.Test.TestId && t.RoundId == activity.Round.RoundId))
                {
                    userTests.Add(new UserTestDto
                    {
                        TestId = activity.Round.Test.TestId,
                        TestName = activity.Round.Test.TestName,
                        TestType = activity.Round.Test.TestType.ToString(),
                        MaxScore = activity.Round.Test.MaxScore,
                        DurationInMinutes = activity.Round.Test.DurationInMinutes,
                        JoinCode = activity.Round.Test.JoinCode,
                        RoundId = activity.Round.RoundId,
                        RoundType = activity.Round.RoundType.ToString(),
                        RoundStartDate = activity.Round.StartDate,
                        RoundEndDate = activity.Round.EndDate,
                        HasCompleted = hasCompleted
                    });
                }
            }

            // Create response
            var response = new GetTestsByUserIdResponse
            {
                UserId = userId,
                TotalTests = userTests.Count,
                Tests = userTests.OrderBy(t => t.RoundStartDate).ToList()
            };

            return new Response<GetTestsByUserIdResponse>(
                data: response,
                code: (int)ResponseCode.GET_SUCCESS,
                message: $"Retrieved {userTests.Count} test(s) for user successfully");
        }
    }
}
