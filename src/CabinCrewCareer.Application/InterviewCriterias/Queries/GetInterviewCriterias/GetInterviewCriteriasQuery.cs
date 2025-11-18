using CabinCrewCareer.Application.InterviewCriterias.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.InterviewCriterias.Queries.GetInterviewCriterias
{
    public record GetInterviewCriteriasQuery : IRequest<Response<List<GetInterviewCriteriaResponse>>>;
}
