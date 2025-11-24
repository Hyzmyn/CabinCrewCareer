using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Queries.GetAppearanceEvaluationFormByActivityId
{
    public record GetAppearanceEvaluationFormByActivityIdQuery(int ActivityId) : IRequest<Response<GetAppearanceEvaluationFormResponse>>;
}
