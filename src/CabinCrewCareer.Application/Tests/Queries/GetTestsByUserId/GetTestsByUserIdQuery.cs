using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Tests.Queries.GetTestsByUserId
{
    public record GetTestsByUserIdQuery() : IRequest<Response<GetTestsByUserIdResponse>>;
}
