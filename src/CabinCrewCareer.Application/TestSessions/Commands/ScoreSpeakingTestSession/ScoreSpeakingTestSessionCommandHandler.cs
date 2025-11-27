using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestSessions.Commands.ScoreSpeakingTestSession
{
    public class ScoreSpeakingTestSessionCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<ScoreSpeakingTestSessionCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(ScoreSpeakingTestSessionCommand request, CancellationToken cancellationToken)
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

            // Validate test session has not been scored yet
            if (testSession.TotalScore > 0)
            {
                errorMessages.Add("This test session has already been scored. Cannot update score.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Validate that criteria scores dictionary is not empty
            if (request.CriteriaScores == null || request.CriteriaScores.Count == 0)
            {
                errorMessages.Add("Criteria scores cannot be empty.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Validate all scores are non-negative
            foreach (var score in request.CriteriaScores.Values)
            {
                if (score < 0)
                {
                    errorMessages.Add("All scores must be non-negative.");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }
            }

            // Calculate total score by summing all criteria scores
            var totalScore = request.CriteriaScores.Values.Sum();

            // Validate total score does not exceed max score of the test
            if (testSession.Test != null && totalScore > testSession.Test.MaxScore)
            {
                errorMessages.Add($"Total score ({totalScore}) cannot exceed the maximum score ({testSession.Test.MaxScore}) of the test.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Update test session total score
            testSession.TotalScore = totalScore;

            // Save changes
            await _unitOfWork.TestSessions.UpdateAsync(testSession);
            await _unitOfWork.CompleteAsync();

            return new Response<string>
            {
                Data = $"Speaking test session scored successfully. Total score: {totalScore}",
                Message = ResponseCode.SUCCESS.ToString()
            };
        }
    }
}
