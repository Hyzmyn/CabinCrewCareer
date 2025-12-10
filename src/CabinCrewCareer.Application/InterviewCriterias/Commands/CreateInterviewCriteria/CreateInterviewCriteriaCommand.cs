using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.InterviewCriterias.Commands.CreateInterviewCriteria
{
    public record CreateInterviewCriteriaCommand(string Criteria, int InterviewCriteriaId) : IRequest<Response<string>>;
}
