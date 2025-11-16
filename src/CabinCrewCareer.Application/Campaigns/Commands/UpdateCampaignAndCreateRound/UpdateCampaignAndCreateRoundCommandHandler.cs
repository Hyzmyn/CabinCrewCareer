using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignAndCreateRound
{
    public class UpdateCampaignAndCreateRoundCommandHandler : IRequestHandler<UpdateCampaignAndCreateRoundCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICampaignRoundStatusService _campaignRoundStatusService;

        public UpdateCampaignAndCreateRoundCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ICampaignRoundStatusService campaignRoundStatusService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _campaignRoundStatusService = campaignRoundStatusService;
        }

        public async Task<Response<bool>> Handle(UpdateCampaignAndCreateRoundCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId);
            if (campaign == null)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.CAMPAIGN_NOT_FOUND,
                    data: false,
                    message: ResponseCode.CAMPAIGN_NOT_FOUND.GetDescription());
            }

            // Chỉ cho phép cập nhật campaign ở trạng thái Draft hoặc Rejected
            if (campaign.CampaignStatus != CabinCrewCareer.Domain.Common.CampaignStatus.Draft && 
                campaign.CampaignStatus != CabinCrewCareer.Domain.Common.CampaignStatus.Rejected)
            {
                return new Response<bool>(
                    code: (int)ResponseCode.FAIL,
                    data: false,
                    message: "Can only update campaigns in Draft or Rejected status");
            }
            // Get SeniorId from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                campaign.SeniorId = int.Parse(currentUserId);
            }
            // Update campaign fields
            campaign.StartDate = request.StartDate;
            campaign.EndDate = request.EndDate;
            // After successful update and round creation, change to Pending for Director approval
            campaign.CampaignStatus = CabinCrewCareer.Domain.Common.CampaignStatus.Pending;
            // Add other campaign fields if needed
            await _unitOfWork.Campaigns.UpdateAsync(campaign);

            // If there is a list of rounds, handle automatic quantity division or check total quantity
            if (request.Rounds != null && request.Rounds.Count > 0)
            {

                int campaignTarget = campaign.TargetQuantity;
                var rounds = request.Rounds;


                // Require targetQuantity for all rounds
                if (rounds.Any(r => r.TargetQuantity <= 0))
                {
                    return new Response<bool>(
                        code: (int)ResponseCode.FAIL,
                        data: false,
                        message: "Each round must have a positive targetQuantity");
                }

                int remain = campaignTarget;
                for (int i = 0; i < rounds.Count; i++)
                {
                    if (i == 0)
                    {
                        int minQty = (int)Math.Round(campaignTarget * 0.6);
                        int maxQty = (int)Math.Round(campaignTarget * 0.7);
                        if (rounds[i].TargetQuantity < minQty || rounds[i].TargetQuantity > maxQty)
                        {
                            return new Response<bool>(
                                code: (int)ResponseCode.FAIL,
                                data: false,
                                message: $"Round 1 targetQuantity must be between 60% and 70% of campaign target ({minQty} - {maxQty})");
                        }
                        remain -= rounds[i].TargetQuantity;
                    }
                    else if (i == rounds.Count - 1)
                    {
                        int minQty = (int)Math.Round(remain * 0.8);
                        if (rounds[i].TargetQuantity < minQty)
                        {
                            return new Response<bool>(
                                code: (int)ResponseCode.FAIL,
                                data: false,
                                message: $"Last round targetQuantity must be at least 80% of remaining ({minQty})");
                        }
                        remain -= rounds[i].TargetQuantity;
                    }
                    else
                    {
                        int minQty = (int)Math.Round(remain * 0.6);
                        int maxQty = (int)Math.Round(remain * 0.7);
                        if (rounds[i].TargetQuantity < minQty || rounds[i].TargetQuantity > maxQty)
                        {
                            return new Response<bool>(
                                code: (int)ResponseCode.FAIL,
                                data: false,
                                message: $"Round {i+1} targetQuantity must be between 60% and 70% of remaining ({minQty} - {maxQty})");
                        }
                        remain -= rounds[i].TargetQuantity;
                    }
                }

                foreach (var round in rounds)
                {
                    var campaignRound = new CampaignRound
                    {
                        CampaignId = request.CampaignId,
                        RoundName = round.RoundName,
                        Description = round.Description ?? string.Empty,
                        TargetQuantity = round.TargetQuantity,
                        ActualQuantiy = 0,
                        StartDate = round.RoundStartDate,
                        EndDate = round.RoundEndDate,
                        RoundStatus = CabinCrewCareer.Domain.Common.RoundStatus.Upcoming,
                        IsActive = true
                    };
                    campaign.CampaignRounds.Add(campaignRound);
                }
                await _unitOfWork.Campaigns.UpdateAsync(campaign);

                // Tự động cập nhật status cho tất cả campaign rounds vừa tạo
                foreach (var round in campaign.CampaignRounds.Where(r => r.IsActive))
                {
                    await _campaignRoundStatusService.UpdateCampaignRoundStatusByDateAsync(round.CampaignRoundId);
                }
            }
            await _unitOfWork.CompleteAsync();
            return new Response<bool>(
                code: (int)ResponseCode.UPDATED_SUCCESS,
                data: true,
                message: "Updated campaign and created round if provided");
        }
    }
}
