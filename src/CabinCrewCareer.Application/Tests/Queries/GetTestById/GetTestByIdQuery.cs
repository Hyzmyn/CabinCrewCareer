using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Tests.Queries.GetTestById
{
    public record GetTestByIdQuery(int TestId) : IRequest<Response<GetTestByIdResponse>>;
}
