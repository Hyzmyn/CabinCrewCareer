using CabinCrewCareer.Application.ScoringCriterias.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.ScoringCriterias.Commands.CreateScoringCriteria
{
    public record CreateScoringCriteriaCommand(int ScoringCriteriaId, CreateScoringCriteriaRequest CreateScoringCriteriaRequest) : IRequest<Response<string>>;
}
