using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Commands.CreateInterviewEvaluationForm
{
    public record CreateInterviewEvaluationFormCommand(
        int ActivityId,
        int Type,
        List<CreateInterviewEvaluationRequest> Choices) : IRequest<Response<string>>;
}
