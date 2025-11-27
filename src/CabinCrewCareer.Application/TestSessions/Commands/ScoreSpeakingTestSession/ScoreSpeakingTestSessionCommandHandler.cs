using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestSessions.Commands.ScoreSpeakingTestSession
{
    public class ScoreSpeakingTestSessionCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<ScoreSpeakingTestSessionCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(ScoreSpeakingTestSessionCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate test session exists
            var testSession = await _unitOfWork.TestSessions.GetByIdAsync(request.TestSessionId);
            if (testSession == null)
            {
                errorMessages.Add("Test session not found.");
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
