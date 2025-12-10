using CabinCrewCareer.Application.TestTypes.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestTypes.Queries.GetTestTypes
{
    public record GetTestTypesQuery : IRequest<Response<List<GetTestTypeResponse>>>;
}
