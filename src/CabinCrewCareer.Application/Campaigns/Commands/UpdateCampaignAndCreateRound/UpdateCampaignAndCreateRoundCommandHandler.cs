using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignAndCreateRound
{
    using CabinCrewCareer.Application.Common.Interfaces.Services;
    public class UpdateCampaignAndCreateRoundCommandHandler : IRequestHandler<UpdateCampaignAndCreateRoundCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public UpdateCampaignAndCreateRoundCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Response<bool>> Handle(UpdateCampaignAndCreateRoundCommand request, CancellationToken cancellationToken)
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
            // Lấy SeniorId từ token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                campaign.SeniorId = int.Parse(currentUserId);
            }
            // Update campaign fields
            campaign.StartDate = request.StartDate;
            campaign.EndDate = request.EndDate;
            // Thêm các trường campaign khác nếu cần
            await _unitOfWork.Campaigns.UpdateAsync(campaign);

            // Nếu có danh sách round thì xử lý chia số lượng tự động hoặc kiểm tra tổng số lượng
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
                        RoundStatus = CabinCrewCareer.Domain.Common.RoundStatus.Upcoming,
                        IsActive = true
                    };
                    campaign.CampaignRounds.Add(campaignRound);
                }
                await _unitOfWork.Campaigns.UpdateAsync(campaign);
            }
            await _unitOfWork.CompleteAsync();
            return new Response<bool>(true, "Updated campaign and created round if provided", 200);
        }
    }
}
