using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Queries.GetInterviewEvaluationFormByActivityId
{
    public record GetInterviewEvaluationFormByActivityIdQuery(int ActivityId) : IRequest<Response<GetInterviewEvaluationFormResponse>>;
}
