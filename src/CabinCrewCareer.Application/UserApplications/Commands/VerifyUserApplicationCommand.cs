using MediatR;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    using CabinCrewCareer.Application.UserApplications.Common;
    public class VerifyUserApplicationCommand : IRequest<Response<UserApplicationResponse>>
    {
        public int ApplicationId { get; set; }
        public int CampaignRoundId { get; set; }
        public VerifyUserApplicationCommand(int applicationId, int campaignRoundId)
        {
            ApplicationId = applicationId;
            CampaignRoundId = campaignRoundId;
        }
    }
}
