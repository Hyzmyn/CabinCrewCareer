using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.EvaluationForms.Queries.GetInterviewEvaluationFormById
{
    public record GetInterviewEvaluationFormByIdQuery(int Id) : IRequest<Response<GetInterviewEvaluationFormDetailResponse>>;
}
