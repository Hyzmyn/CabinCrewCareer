using AutoMapper;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Users.Queries.GetUserHistory
{
    public class GetUserHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService) : IRequestHandler<GetUserHistoryQuery, Response<List<GetUserCampaignResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<List<GetUserCampaignResponse>>> Handle(GetUserHistoryQuery request, CancellationToken cancellationToken)
        {
            int userId = 0;

            var user = _currentUserService.CurrentPrincipal;

            if (user is not null) userId = Int32.Parse(user);

            var campaigns = await _unitOfWork.CampaignRounds.GetUserHistoryAsync();
            List<GetUserCampaignResponse> list = [];

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
                            list.Add(new GetUserCampaignResponse
                            {
                                CampaignRoundId = campaigns[i].CampaignRoundId,
                                CampaignName = campaigns[i].Campaign.CampaignName,
                                AirlinePartner = campaigns[i].Campaign.CampaignRequest.AirlinePartner.PartnerName,
                                RoundStatus = campaigns[i].RoundStatus.GetDescription(),
                                RoundName = campaigns[i].RoundName,
                                Description = campaigns[i].Description,
                            });

                            foreach (var item in list)
                            {
                                item.Rounds.Add(new GetRoundActivityResponse
                                {
                                    RoundId = round.RoundId,
                                    RoundName = round.RoundType.GetDescription(),
                                    Status = activity.Status.GetDescription()
                                });
                            }
                        }
                    }
                }
            }

            return new Response<List<GetUserCampaignResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: list,
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
