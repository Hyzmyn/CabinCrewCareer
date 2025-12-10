using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.InterviewCriterias.Commands.UpdateInterviewCriteria
{
    public record UpdateInterviewCriteriaCommand(int InterviewCriteriaId, int InterviewCriteriaItemId, string Criteria) : IRequest<Response<string>>;
}
