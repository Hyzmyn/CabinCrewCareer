using MediatR;
using CabinCrewCareer.Domain.Wrappers;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignFields
{
    public class UpdateCampaignFieldsCommand : IRequest<Response<bool>>
    {
        public int CampaignId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        // Thêm các trường khác nếu cần

        public UpdateCampaignFieldsCommand(int campaignId, DateTime? startDate, DateTime? endDate)
        {
            CampaignId = campaignId;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
