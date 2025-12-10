using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.ScoringCriterias.Commands.DeleteScoringCriteria
{
    public record DeleteScoringCriteriaCommand(int ScoringCriteriaId, int ScoringCriteriaItemId) : IRequest<Response<string>>;
}
