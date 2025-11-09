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
                return new Response<bool>(false, "Campaign not found", 404);
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
                bool needAutoSplit = request.Rounds.Any(r => r.TargetQuantity <= 0);
                var rounds = request.Rounds;

                if (needAutoSplit)
                {
                    int remain = campaignTarget;
                    for (int i = 0; i < rounds.Count; i++)
                    {
                        int percent = (i == rounds.Count - 1) ? 100 : new Random().Next(60, 71); // Last round gets the remainder
                        int qty;
                        if (i == rounds.Count - 1)
                        {
                            qty = remain;
                        }
                        else
                        {
                            qty = (int)Math.Round(remain * percent / 100.0);
                            if (qty < 1) qty = 1;
                        }
                        rounds[i].TargetQuantity = qty;
                        remain -= qty;
                    }
                }

                int totalRoundQty = rounds.Sum(r => r.TargetQuantity);
                if (totalRoundQty != campaignTarget)
                {
                    return new Response<bool>(false, $"Total target quantity of rounds ({totalRoundQty}) must equal campaign target ({campaignTarget})", 400);
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
