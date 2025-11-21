using MediatR;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Rounds.Queries;
using CabinCrewCareer.Application.Common.Enums;

namespace CabinCrewCareer.Application.Rounds.Queries
{
    public class GetRoundTestInfoQueryHandler : IRequestHandler<GetRoundTestInfoQuery, RoundTestInfoResponse?>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetRoundTestInfoQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RoundTestInfoResponse?> Handle(GetRoundTestInfoQuery request, CancellationToken cancellationToken)
        {
            var round = await _unitOfWork.Rounds.GetByIdAsync(request.RoundId);
            if (round == null || round.TestId == null)
                throw new ApiException(ResponseCode.GET_UNSUCC);

            var test = await _unitOfWork.Tests.GetByIdAsync(round.TestId.Value);
            if (test == null)
                throw new ApiException(ResponseCode.GET_UNSUCC);

            return new RoundTestInfoResponse
            {
                TestId = test.TestId,
                TestName = test.TestName,
                Purpose = test.Purpose,
                TestType = test.TestType.ToString(),
                MaxScore = test.MaxScore,
                AudioFileURL = test.AudioFileURL,
                JoinCode = test.JoinCode,
                DurationInMinutes = test.DurationInMinutes
            };
        }
    }
}
