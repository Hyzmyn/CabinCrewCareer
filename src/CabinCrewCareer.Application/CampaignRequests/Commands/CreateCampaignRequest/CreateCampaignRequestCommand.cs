using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.CreateCampaignRequest
{
    public record CreateCampaignRequestCommand(
        string CampaignName,
        string Description,
        string JobDescription,
        string JobRequirement,
        int TargetQuantity,
        CampaignType RequestType) : IRequest<Response<GetCampaignRequestResponse>>;
}
