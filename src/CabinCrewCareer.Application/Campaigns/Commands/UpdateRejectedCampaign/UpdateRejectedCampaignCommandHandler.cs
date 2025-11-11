using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateRejectedCampaign
{
    public class UpdateRejectedCampaignCommandHandler : IRequestHandler<UpdateRejectedCampaignCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateRejectedCampaignCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<bool>> Handle(UpdateRejectedCampaignCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(request.CampaignId);
            if (campaign == null)
            {
                return new Response<bool>
                {
                    Code = 404,
                    Message = "Campaign not found",
                    Data = false
                };
            }

            // Chỉ cho phép cập nhật campaign ở trạng thái Rejected
            if (campaign.CampaignStatus != CampaignStatus.Rejected)
            {
                return new Response<bool>
                {
                    Code = 400,
                    Message = "Can only update campaigns with Rejected status",
                    Data = false
                };
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
            // After update, change to Pending for Director re-approval
            campaign.CampaignStatus = CampaignStatus.Pending;
            // Clear previous review data when resubmit
            campaign.RejectedReason = string.Empty;
            campaign.RejectedAt = null;
            campaign.ApprovedAt = null;
            campaign.ReviewedBy = string.Empty;

            // Delete all old rounds before adding new ones
            var existingRounds = campaign.CampaignRounds.ToList();
            foreach (var existingRound in existingRounds)
            {
                campaign.CampaignRounds.Remove(existingRound);
            }

            // If there is a list of rounds, handle validation and create new ones
            if (request.Rounds != null && request.Rounds.Count > 0)
            {
                int campaignTarget = campaign.TargetQuantity;
                var rounds = request.Rounds;

                // Require targetQuantity for all rounds
                if (rounds.Any(r => r.TargetQuantity <= 0))
                {
                    return new Response<bool>
                    {
                        Code = 400,
                        Message = "Each round must have a positive targetQuantity",
                        Data = false
                    };
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
                            return new Response<bool>
                            {
                                Code = 400,
                                Message = $"Round 1 targetQuantity must be between 60% and 70% of campaign target ({minQty} - {maxQty})",
                                Data = false
                            };
                        }
                        remain -= rounds[i].TargetQuantity;
                    }
                    else if (i == rounds.Count - 1)
                    {
                        int minQty = (int)Math.Round(remain * 0.8);
                        if (rounds[i].TargetQuantity < minQty)
                        {
                            return new Response<bool>
                            {
                                Code = 400,
                                Message = $"Last round targetQuantity must be at least 80% of remaining ({minQty})",
                                Data = false
                            };
                        }
                        remain -= rounds[i].TargetQuantity;
                    }
                    else
                    {
                        int minQty = (int)Math.Round(remain * 0.6);
                        int maxQty = (int)Math.Round(remain * 0.7);
                        if (rounds[i].TargetQuantity < minQty || rounds[i].TargetQuantity > maxQty)
                        {
                            return new Response<bool>
                            {
                                Code = 400,
                                Message = $"Round {i+1} targetQuantity must be between 60% and 70% of remaining ({minQty} - {maxQty})",
                                Data = false
                            };
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
                        RoundStatus = RoundStatus.Upcoming,
                        IsActive = true
                    };
                    campaign.CampaignRounds.Add(campaignRound);
                }
            }

            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.CompleteAsync();

            return new Response<bool>
            {
                Code = 200,
                Message = "Campaign updated successfully, resubmitted for Director approval",
                Data = true
            };
        }
    }
}