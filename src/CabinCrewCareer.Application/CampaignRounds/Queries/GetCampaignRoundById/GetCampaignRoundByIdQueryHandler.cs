using AutoMapper;
using CabinCrewCareer.Application.CampaignRounds.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRounds.Queries.GetCampaignRoundById
{
    public class GetCampaignRoundByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetCampaignRoundByIdQuery, Response<GetCampaignRoundResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<GetCampaignRoundResponse>> Handle(GetCampaignRoundByIdQuery request, CancellationToken cancellationToken)
        {
            var campaignRoundQuery = await _unitOfWork.CampaignRounds.GetAsync(campaignRound => campaignRound.CampaignRoundId == request.CampaignRoundId);

            var campaignRound = campaignRoundQuery
            .FirstOrDefault() ?? throw new ApiException(ResponseCode.CAMPAIGN_ROUND_NOT_FOUND);

            return new Response<GetCampaignRoundResponse>(code: (int)ResponseCode.SUCCESS, data: _mapper.Map<GetCampaignRoundResponse>(campaignRound), message: ResponseCode.SUCCESS.ToString());
        }
    }
}
