using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetMyPracticalTestSessions
{
    public class GetMyPracticalTestSessionsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<GetMyPracticalTestSessionsQuery, Response<PaginatedList<GetTestSessionResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<PaginatedList<GetTestSessionResponse>>> Handle(
            GetMyPracticalTestSessionsQuery request,
            CancellationToken cancellationToken)
        {
            // Get current user from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            var userId = int.Parse(currentUserId);

            // Build query with filters for current user and Practical test type
            var query = await _unitOfWork.TestSessions.GetAllAsync(
                expression: ts => 
                    ts.UserId == userId &&
                    ts.Test != null &&
                    ts.Test.TestType == TestType.Practical,
                includeFunc: q => q
                    .Include(ts => ts.User)
                    .Include(ts => ts.Test)
                        .ThenInclude(t => t.Rounds)
                            .ThenInclude(r => r.RoundActivities)
                    .Include(ts => ts.TestAnswers)
            );

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(ts => 
                    ts.Test.TestName.ToLower().Contains(searchTerm)
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

            // Map to response DTOs
            var testSessionResponses = testSessions.Select(ts => 
            {
                // Find RoundActivity for this user in any round using this test
                var roundActivity = ts.Test?.Rounds
                    ?.SelectMany(r => r.RoundActivities ?? new List<Domain.Entities.RoundActivity>())
                    .FirstOrDefault(ra => ra.UserId == userId);

                // Check if user has been evaluated (Pass or Fail = true, Ongoing = false)
                bool isPassedOrFailed = false;
                if (roundActivity != null)
                {
                    isPassedOrFailed = roundActivity.Status == ActivityStatus.Passed || 
                                       roundActivity.Status == ActivityStatus.Failed;
                }

                return new GetTestSessionResponse
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
                    IsPassedOrFailed = isPassedOrFailed
                };
            }).ToList();

            var paginatedResponse = new PaginatedList<GetTestSessionResponse>(
                testSessionResponses,
                request.Page,
                request.PageSize,
                totalRecords)
            {
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
            };

            return new Response<PaginatedList<GetTestSessionResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: paginatedResponse,
                message: "Practical test sessions retrieved successfully");
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
                "testid" => isDescending ? query.OrderByDescending(ts => ts.TestId) : query.OrderBy(ts => ts.TestId),
                "totalscore" => isDescending ? query.OrderByDescending(ts => ts.TotalScore) : query.OrderBy(ts => ts.TotalScore),
                "starttime" => isDescending ? query.OrderByDescending(ts => ts.StartTime) : query.OrderBy(ts => ts.StartTime),
                "endtime" => isDescending ? query.OrderByDescending(ts => ts.EndTime) : query.OrderBy(ts => ts.EndTime),
                "createdat" => isDescending ? query.OrderByDescending(ts => ts.CreatedAt) : query.OrderBy(ts => ts.CreatedAt),
                _ => query.OrderByDescending(ts => ts.CreatedAt) // Default sort by CreatedAt descending
            };
        }
    }
}
