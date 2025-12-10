using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.InterviewCriterias.Commands.DeleteInterviewCriteria
{
    public record DeleteInterviewCriteriaCommand(int InterviewCriteriaId, int InterviewCriteriaItemId) : IRequest<Response<string>>;
}
