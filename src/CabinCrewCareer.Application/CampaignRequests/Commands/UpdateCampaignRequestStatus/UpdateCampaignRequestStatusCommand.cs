using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRequests.Commands.UpdateCampaignRequestStatus
{
    public record UpdateCampaignRequestStatusCommand(
        int RequestId,
        int Status,
        string? RejectReason) : IRequest<Response<GetCampaignRequestResponse>>;
}
