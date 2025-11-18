using CabinCrewCareer.Application.ScoringCriterias.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.ScoringCriterias.Queries
{
    public record GetScoringCriteriasQuery : IRequest<Response<List<GetScoringCriteriaResponse>>>;
}
