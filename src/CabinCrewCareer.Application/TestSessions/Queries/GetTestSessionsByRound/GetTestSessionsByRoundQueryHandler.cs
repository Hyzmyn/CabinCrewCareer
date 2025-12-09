using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetTestSessionsByRound
{
    public class GetTestSessionsByRoundQueryHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<GetTestSessionsByRoundQuery, Response<PaginatedList<TestSessionWithStatusResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<PaginatedList<TestSessionWithStatusResponse>>> Handle(
            GetTestSessionsByRoundQuery request,
            CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate round exists
            var roundQuery = await _unitOfWork.Rounds.GetAllAsync(
                expression: r => r.RoundId == request.RoundId,
                includeFunc: q => q
                    .Include(r => r.Test)
                    .Include(r => r.RoundActivities)
            );

            var round = await roundQuery.FirstOrDefaultAsync(cancellationToken);

            if (round == null)
            {
                errorMessages.Add($"Round with ID {request.RoundId} not found");
                throw new ValidationException(ResponseCode.FAIL, errorMessages);
            }

            // Validate test type if provided
            if (request.TestType.HasValue && 
                (round.Test == null || round.Test.TestType != request.TestType.Value))
            {
                errorMessages.Add($"Round {request.RoundId} does not match the specified test type");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Get user IDs from RoundActivities in this round
            var userIdsInRound = round.RoundActivities?
                .Where(ra => ra.UserId.HasValue)
                .Select(ra => ra.UserId!.Value)
                .ToList() ?? new List<int>();

            if (!userIdsInRound.Any())
            {
                // No users in this round, return empty result
                var emptyResponse = new PaginatedList<TestSessionWithStatusResponse>(
                    new List<TestSessionWithStatusResponse>(),
                    request.Page,
                    request.PageSize,
                    0)
                {
                    TotalPages = 0
                };

                return new Response<PaginatedList<TestSessionWithStatusResponse>>(
                    code: (int)ResponseCode.SUCCESS,
                    data: emptyResponse,
                    message: $"No users found in round {request.RoundId}");
            }

            // Get test sessions ONLY for users in this round's RoundActivities
            var query = await _unitOfWork.TestSessions.GetAllAsync(
                expression: ts => 
                    ts.TestId == round.TestId && 
                    ts.UserId.HasValue && 
                    userIdsInRound.Contains(ts.UserId.Value),
                includeFunc: q => q
                    .Include(ts => ts.User)
                    .Include(ts => ts.Test)
                    .Include(ts => ts.TestAnswers)
            );

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(ts => 
                    ts.User.FullName.ToLower().Contains(searchTerm) ||
                    ts.User.Email.ToLower().Contains(searchTerm)
                );
            }

            // Apply sorting
            query = ApplySorting(query, request.SortColumn, request.SortOrder);

            // Get total count
            var totalRecords = await query.CountAsync(cancellationToken);

            // Apply pagination
            var testSessions = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Map to response DTOs with RoundActivity status
            var testSessionResponses = testSessions.Select(ts =>
            {
                // Find the corresponding RoundActivity for this user in this round
                var roundActivity = round.RoundActivities?
                    .FirstOrDefault(ra => ra.UserId == ts.UserId);

                var activityStatus = "Ongoing"; // Default
                if (roundActivity != null)
                {
                    activityStatus = roundActivity.Status switch
                    {
                        ActivityStatus.Ongoing => "Ongoing",
                        ActivityStatus.Passed => "Passed",
                        ActivityStatus.Failed => "Failed",
                        _ => "Ongoing"
                    };
                }

                return new TestSessionWithStatusResponse
                {
                    TestSessionId = ts.TestSessionId,
                    UserId = ts.UserId ?? 0,
                    UserFullName = ts.User?.FullName ?? string.Empty,
                    UserEmail = ts.User?.Email ?? string.Empty,
                    ImgURL = ts.User?.ImgURL,
                    TestId = ts.TestId ?? 0,
                    TestName = ts.Test?.TestName ?? string.Empty,
                    TestType = ts.Test?.TestType.ToString() ?? string.Empty,
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,
                    TotalScore = ts.TotalScore,
                    MaxScore = ts.Test?.MaxScore ?? 0,
                    Status = ts.Status,
                    CreatedAt = ts.CreatedAt,
                    TotalAnswers = ts.TestAnswers?.Count ?? 0,
                    ActivityStatus = activityStatus,
                    RoundId = round.RoundId,
                    RoundType = round.RoundType.ToString(),
                    ActivityId = roundActivity?.ActivityId
                };
            }).ToList();

            var paginatedResponse = new PaginatedList<TestSessionWithStatusResponse>(
                testSessionResponses,
                request.Page,
                request.PageSize,
                totalRecords)
            {
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };

            return new Response<PaginatedList<TestSessionWithStatusResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: paginatedResponse,
                message: $"Test sessions for round {request.RoundId} retrieved successfully");
        }

        private static IQueryable<Domain.Entities.TestSession> ApplySorting(
            IQueryable<Domain.Entities.TestSession> query,
            string? sortColumn,
            string? sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortColumn?.ToLower() switch
            {
                "testsessionid" => isDescending ? query.OrderByDescending(ts => ts.TestSessionId) : query.OrderBy(ts => ts.TestSessionId),
                "username" => isDescending ? query.OrderByDescending(ts => ts.User.FullName) : query.OrderBy(ts => ts.User.FullName),
                "totalscore" => isDescending ? query.OrderByDescending(ts => ts.TotalScore) : query.OrderBy(ts => ts.TotalScore),
                "starttime" => isDescending ? query.OrderByDescending(ts => ts.StartTime) : query.OrderBy(ts => ts.StartTime),
                "endtime" => isDescending ? query.OrderByDescending(ts => ts.EndTime) : query.OrderBy(ts => ts.EndTime),
                "createdat" => isDescending ? query.OrderByDescending(ts => ts.CreatedAt) : query.OrderBy(ts => ts.CreatedAt),
                _ => query.OrderByDescending(ts => ts.CreatedAt) // Default sort by CreatedAt descending
            };
        }
    }
}
