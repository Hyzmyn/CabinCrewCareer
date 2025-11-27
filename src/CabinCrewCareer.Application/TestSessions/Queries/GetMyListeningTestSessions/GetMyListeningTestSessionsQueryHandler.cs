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

namespace CabinCrewCareer.Application.TestSessions.Queries.GetMyListeningTestSessions
{
    public class GetMyListeningTestSessionsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<GetMyListeningTestSessionsQuery, Response<PaginatedList<GetTestSessionResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<PaginatedList<GetTestSessionResponse>>> Handle(
            GetMyListeningTestSessionsQuery request,
            CancellationToken cancellationToken)
        {
            // Get current user from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            var userId = int.Parse(currentUserId);

            // Build query with filters for current user and Listening test type
            var query = await _unitOfWork.TestSessions.GetAllAsync(
                expression: ts => 
                    ts.UserId == userId &&
                    ts.Test != null &&
                    ts.Test.TestType == TestType.EnglishListening,
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
            var testSessionResponses = testSessions.Select(ts => new GetTestSessionResponse
            {
                TestSessionId = ts.TestSessionId,
                UserId = ts.UserId ?? 0,
                UserFullName = ts.User?.FullName ?? string.Empty,
                UserEmail = ts.User?.Email ?? string.Empty,
                TestId = ts.TestId ?? 0,
                TestName = ts.Test?.TestName ?? string.Empty,
                TestType = ts.Test?.TestType.ToString() ?? string.Empty,
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                TotalScore = ts.TotalScore,
                MaxScore = ts.Test?.MaxScore ?? 0,
                Status = ts.Status,
                CreatedAt = ts.CreatedAt,
                TotalAnswers = ts.TestAnswers?.Count ?? 0
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
                message: "Listening test sessions retrieved successfully");
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
