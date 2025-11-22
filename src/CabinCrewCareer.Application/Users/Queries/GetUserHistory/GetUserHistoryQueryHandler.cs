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

            return new Response<List<GetUserCampaignResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetUserCampaignResponse>>(await _unitOfWork.CampaignRounds.GetUserHistoryAsync()),
                message: ResponseCode.SUCCESS.GetDescription());
        }
    }
}
