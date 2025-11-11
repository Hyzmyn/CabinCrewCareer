using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignStatus
{
    public record UpdateCampaignStatusCommand(
        int CampaignId,
        int Status,
        string? RejectReason) : IRequest<Response<bool>>;
}