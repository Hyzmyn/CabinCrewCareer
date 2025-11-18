using CabinCrewCareer.Application.ScoringCriterias.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.ScoringCriterias.Queries.GetScoringCriterias
{
    public record GetScoringCriteriasQuery : IRequest<Response<List<GetScoringCriteriaResponse>>>;
}
