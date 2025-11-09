using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.CreateCampaignRound
{
    public class CreateCampaignRoundCommandHandler : IRequestHandler<CreateCampaignRoundCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateCampaignRoundCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<bool>> Handle(CreateCampaignRoundCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId);
            if (campaign == null)
            {
                return new Response<bool>(false, "Campaign not found", 404);
            }
            var campaignRound = new CampaignRound
            {
                CampaignId = request.CampaignId,
                RoundName = request.RoundName,
                Description = request.Description,
                TargetQuantity = request.TargetQuantity,
                ActualQuantiy = 0,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RoundStatus = CabinCrewCareer.Domain.Common.RoundStatus.Upcoming,
                IsActive = true
            };
            campaign.CampaignRounds.Add(campaignRound);
            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CompleteAsync();
            return new Response<bool>(true, "Campaign round created successfully", 200);
        }
    }
}
