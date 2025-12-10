using CabinCrewCareer.Application.Requirements.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Requirements.Queries.GetRequirementsByType
{
    public record GetRequirementsByTypeQuery(CampaignType CampaignType) : IRequest<Response<GetRequirementResponse>>;
}
