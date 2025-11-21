using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Tests.Queries.GetAvailableTests
{
    public record GetAvailableTestsQuery(int? TestType) : IRequest<Response<List<GetAvailableTestResponse>>>;
}
