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

            return new Response<GetUserCampaignResponse>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<GetUserCampaignResponse>(await _unitOfWork.CampaignRounds.GetUserOngoingCampaignAsync(userId)),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
