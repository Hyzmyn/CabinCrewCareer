using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.UserHistories.Queries.GetUserOngoingCampaign
{
    public class GetUserOngoingCampaignQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<GetUserOngoingCampaignQuery, Response<GetUserCampaignResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<GetUserCampaignResponse>> Handle(GetUserOngoingCampaignQuery request, CancellationToken cancellationToken)
        {
            int userId = 0;

            var user = _currentUserService.CurrentPrincipal;

            if (user is not null) userId = Int32.Parse(user);

            var result = await _unitOfWork.UserHistories.GetUserOngoingCampaignAsync(userId);

            GetUserCampaignResponse response = new()
            {
                CampaignRoundId = result.CampaignRoundId,
                ParticipatedDate = result.ParticipatedDate.GetFormatDate(),
                RoundName = result.CampaignRound.RoundName,
                RoundStatus = result.Type.ToString(),
                Description = result.CampaignRound.Description,
                CampaignType = result.CampaignRound.Campaign.CampaignType.GetDescription(),
                CampaignName = result.CampaignRound.Campaign.CampaignName,
                AirlinePartner = result.CampaignRound.Campaign.CampaignRequest.AirlinePartner.PartnerName
            };

            foreach (var item in result.CampaignRound.Rounds)
            {
                var temp = await _unitOfWork.RoundActivites.GetRoundActivityAsync(item.RoundId, userId);

                if (temp is not null)
                {
                    response.Rounds.Add(new GetRoundActivityResponse
                    {
                        ActivityId = temp.ActivityId,
                        RoundId = item.RoundId,
                        RoundName = item.RoundType.GetDescription(),
                        Status = temp.Status.GetDescription(),
                        ApplicationId = temp.ApplicationSnapshotId
                    });
                }
            }

            return new Response<GetUserCampaignResponse>(
                code: (int)ResponseCode.SUCCESS,
                data: response,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
