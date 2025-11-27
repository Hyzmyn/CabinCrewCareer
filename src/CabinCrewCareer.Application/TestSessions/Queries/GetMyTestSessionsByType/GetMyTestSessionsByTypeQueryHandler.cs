using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestSessions.Queries.GetMyTestSessionsByType
{
    public class GetMyTestSessionsByTypeQueryHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<GetMyTestSessionsByTypeQuery, Response<PaginatedList<GetTestSessionResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<PaginatedList<GetTestSessionResponse>>> Handle(
            GetMyTestSessionsByTypeQuery request,
            CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate test type
            if (!Enum.IsDefined(typeof(TestType), request.TestType))
            {
                errorMessages.Add("Invalid test type. Valid values: 1 (English Listening), 2 (English Speaking), 3 (Practical)");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            var testType = (TestType)request.TestType;

            // Build query with filters for specified test type, optional user filter, and optional round filter
            var query = await _unitOfWork.TestSessions.GetAllAsync(
                expression: ts => 
                    ts.Test != null &&
                    ts.Test.TestType == testType &&
                    (request.UserId == null || ts.UserId == request.UserId) &&
                    (request.RoundId == null || ts.Test.Rounds.Any(r => r.RoundId == request.RoundId)),
                includeFunc: q => q
                    .Include(ts => ts.User)
                    .Include(ts => ts.Test)
                        .ThenInclude(t => t.Rounds)
                    .Include(ts => ts.TestAnswers)
            );

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(ts => 
                    ts.Test.TestName.ToLower().Contains(searchTerm) ||
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

            // Map to response DTOs
            var testSessionResponses = testSessions.Select(ts => new GetTestSessionResponse
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

            var testTypeName = testType switch
            {
                TestType.EnglishListening => "English Listening",
                TestType.EnglishSpeaking => "English Speaking",
                TestType.Practical => "Practical",
                _ => "Unknown"
            };

            return new Response<PaginatedList<GetTestSessionResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: paginatedResponse,
                message: $"{testTypeName} test sessions retrieved successfully");
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
