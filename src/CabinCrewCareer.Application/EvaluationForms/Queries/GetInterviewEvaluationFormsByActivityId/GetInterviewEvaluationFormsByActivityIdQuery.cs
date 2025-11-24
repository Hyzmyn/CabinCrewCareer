using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Queries.GetInterviewEvaluationFormsByActivityId
{
    public record GetInterviewEvaluationFormsByActivityIdQuery(int ActivityId) : IRequest<Response<List<GetInterviewEvaluationFormResponse>>>;
}
