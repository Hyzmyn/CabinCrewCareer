using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignFields
{
    public class UpdateCampaignFieldsCommandHandler : IRequestHandler<UpdateCampaignFieldsCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCampaignFieldsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<bool>> Handle(UpdateCampaignFieldsCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId);
            if (campaign == null)
            {
                return new Response<bool>(false, "Campaign not found", 404);
            }
            campaign.StartDate = request.StartDate;
            campaign.EndDate = request.EndDate;
            // Cập nhật các trường khác nếu cần
            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CompleteAsync();
            return new Response<bool>(true, "Updated successfully", 200);
        }
    }
}
