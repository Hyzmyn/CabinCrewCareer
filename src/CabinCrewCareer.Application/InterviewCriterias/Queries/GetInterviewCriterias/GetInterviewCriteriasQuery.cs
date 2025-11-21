using CabinCrewCareer.Application.InterviewCriterias.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.InterviewCriterias.Queries.GetInterviewCriterias
{
    public record GetInterviewCriteriasQuery(CampaignType Type) : IRequest<Response<List<GetInterviewCriteriaResponse>>>;
}
