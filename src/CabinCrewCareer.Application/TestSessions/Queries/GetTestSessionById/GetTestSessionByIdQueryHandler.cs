using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetTestSessionById
{
    public class GetTestSessionByIdQueryHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<GetTestSessionByIdQuery, Response<GetTestSessionResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<GetTestSessionResponse>> Handle(GetTestSessionByIdQuery request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Get test session with includes
            var testSessions = await _unitOfWork.TestSessions.GetAllAsync(
                expression: ts => ts.TestSessionId == request.TestSessionId,
                includeFunc: query => query
                    .Include(ts => ts.User)
                    .Include(ts => ts.Test)
                    .Include(ts => ts.TestAnswers)
            );

            var testSession = testSessions.FirstOrDefault();
            if (testSession == null)
            {
                errorMessages.Add("Test session not found.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Map to response
            var response = new GetTestSessionResponse
            {
                TestSessionId = testSession.TestSessionId,
                UserId = testSession.UserId ?? 0,
                UserFullName = testSession.User?.FullName ?? string.Empty,
                UserEmail = testSession.User?.Email ?? string.Empty,
                ImgURL = testSession.User?.ImgURL,
                TestId = testSession.TestId ?? 0,
                TestName = testSession.Test?.TestName ?? string.Empty,
                TestType = testSession.Test?.TestType.ToString() ?? string.Empty,
                StartTime = testSession.StartTime,
                EndTime = testSession.EndTime,
                Status = testSession.Status,
                TotalScore = testSession.TotalScore,
                MaxScore = testSession.Test?.MaxScore ?? 0,
                CreatedAt = testSession.CreatedAt,
                TotalAnswers = testSession.TestAnswers?.Count ?? 0
            };

            return new Response<GetTestSessionResponse>
            {
                Data = response,
                Message = ResponseCode.SUCCESS.ToString()
            };
        }
    }
}
