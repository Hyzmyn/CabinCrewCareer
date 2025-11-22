using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Queries.GetUserOngoingCampaign
{
    public class GetUserOngoingCampaignQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService) : IRequestHandler<GetUserOngoingCampaignQuery, Response<GetUserCampaignResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<GetUserCampaignResponse>> Handle(GetUserOngoingCampaignQuery request, CancellationToken cancellationToken)
        {
            int userId = 0;

            var user = _currentUserService.CurrentPrincipal;

            if (user is not null) userId = Int32.Parse(user);

            var campaigns = await _unitOfWork.CampaignRounds.GetUserOngoingCampaignAsync();
            var campaignResponse = new GetUserCampaignResponse();

            for (int i = 0; i < campaigns.Count; i++)
            {
                if (campaigns[i].Rounds != null && campaigns[i].Rounds.Count != 0)
                {
                    foreach (var round in campaigns[i].Rounds)
                    {
                        var rawActivity = await _unitOfWork.RoundActivites.GetAsync(r => r.UserId == userId && r.RoundId == round.RoundId);

                        var activity = rawActivity.FirstOrDefault();

                        if (activity != null)
                        {
                            campaignResponse.CampaignRoundId = campaigns[i].CampaignRoundId;
                            campaignResponse.CampaignName = campaigns[i].Campaign.CampaignName;
                            campaignResponse.AirlinePartner = campaigns[i].Campaign.CampaignRequest.AirlinePartner.PartnerName;
                            campaignResponse.RoundStatus = campaigns[i].RoundStatus.GetDescription();
                            campaignResponse.RoundName = campaigns[i].RoundName;
                            campaignResponse.Description = campaigns[i].Description;
                            campaignResponse.Rounds.Add(new GetRoundActivityResponse
                            {
                                RoundId = round.RoundId,
                                RoundName = round.RoundType.GetDescription(),
                                Status = activity.Status.GetDescription()
                            });
                        }
                    }
                }
            }

            return new Response<GetUserCampaignResponse>(
                code: (int)ResponseCode.SUCCESS,
                data: campaignResponse,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
