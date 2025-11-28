using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.UserHistories.Queries.GetUserCompletedCampaigns
{
    public class GetUserCompletedCampaignsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<GetUserCompletedCampaignsQuery, Response<List<GetUserCampaignResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<List<GetUserCampaignResponse>>> Handle(GetUserCompletedCampaignsQuery request, CancellationToken cancellationToken)
        {
            int userId = 0;

            var user = _currentUserService.CurrentPrincipal;

            if (user is not null) userId = Int32.Parse(user);

            var result = await _unitOfWork.UserHistories.GetUserCompletedCampaignsAsync(userId);

            List<GetUserCampaignResponse> response = [];

            foreach (var history in result)
            {
                GetUserCampaignResponse temp = new()
                {
                    CampaignRoundId = history.CampaignRoundId,
                    ParticipatedDate = history.ParticipatedDate.GetFormatDate(),
                    RoundName = history.CampaignRound.RoundName,
                    RoundStatus = history.Type.ToString(),
                    Description = history.CampaignRound.Description,
                    CampaignType = history.CampaignRound.Campaign.CampaignType.GetDescription(),
                    CampaignName = history.CampaignRound.Campaign.CampaignName,
                    AirlinePartner = history.CampaignRound.Campaign.CampaignRequest.AirlinePartner.PartnerName
                };

                foreach (var item in history.CampaignRound.Rounds)
                {
                    var activity = await _unitOfWork.RoundActivites.GetRoundActivityAsync(item.RoundId, userId);

                    if (activity is not null)
                    {
                        temp.Rounds.Add(new GetRoundActivityResponse
                        {
                            ActivityId = activity.ActivityId,
                            RoundId = item.RoundId,
                            RoundName = item.RoundType.GetDescription(),
                            Status = activity.Status.GetDescription()
                        });
                    }
                }
                response.Add(temp);
            }

            return new Response<List<GetUserCampaignResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: response,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
