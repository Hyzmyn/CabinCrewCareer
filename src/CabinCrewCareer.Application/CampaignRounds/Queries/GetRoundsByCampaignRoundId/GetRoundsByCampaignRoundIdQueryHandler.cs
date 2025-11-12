using AutoMapper;
using CabinCrewCareer.Application.CampaignRounds.Common;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.CampaignRounds.Queries.GetRoundsByCampaignRoundId
{
    public class GetRoundsByCampaignRoundIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<GetRoundsByCampaignRoundIdQuery, Response<List<GetRoundResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<List<GetRoundResponse>>> Handle(GetRoundsByCampaignRoundIdQuery request, CancellationToken cancellationToken)
        {
            var roundQuery = await _unitOfWork.Rounds.GetAllAsync(
                    expression: r => r.CampaignRoundId == request.CampaignRoundId);

            return new Response<List<GetRoundResponse>>(
                code: (int)ResponseCode.SUCCESS,
                data: _mapper.Map<List<GetRoundResponse>>(roundQuery.ToList()),
                message: ResponseCode.SUCCESS.ToString());
        }
    }
}
