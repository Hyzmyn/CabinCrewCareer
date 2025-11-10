using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Tests.Queries.GetTests
{
    public class GetTestsQueryHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<GetTestsQuery, Response<PaginatedList<GetTestResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<PaginatedList<GetTestResponse>>> Handle(
            GetTestsQuery request,
            CancellationToken cancellationToken)
        {
            // Get paginated tests from repository
            var tests = await _unitOfWork.Tests.PaginatedListAsync(
                request.SearchTerm,
                request.SortColumn,
                request.SortOrder,
                request.TestType,
                request.Page,
                request.PageSize);

            // Map to response DTOs
            var testResponses = tests.Items.Select(test => new GetTestResponse
            {
                TestId = test.TestId,
                TestName = test.TestName,
                Purpose = test.Purpose,
                TestType = test.TestType.ToString(),
                MaxScore = test.MaxScore,
                AudioFileURL = string.IsNullOrEmpty(test.AudioFileURL) ? null : test.AudioFileURL,
                JoinCode = test.JoinCode,
                CreatedAt = test.CreatedAt?.GetFormatDateTime(),
                CreatedBy = test.CreatedBy
            }).ToList();

            var paginatedResponse = new PaginatedList<GetTestResponse>(
                testResponses,
                tests.CurrentPage,
                tests.PageSize,
                tests.TotalRecords)
            {
                TotalPages = tests.TotalPages
            };

            return new Response<PaginatedList<GetTestResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: paginatedResponse,
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
