using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Commands.CreateAppearanceEvaluationForm
{
    public record CreateAppearanceEvaluationFormCommand(
        int ActivityId,
        string Comment,
        List<CreateAppearanceEvaluationRequest> Choices) : IRequest<Response<string>>;
}
