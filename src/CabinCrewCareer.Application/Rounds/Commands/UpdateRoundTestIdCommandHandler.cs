using CabinCrewCareer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Enums;

namespace CabinCrewCareer.Application.Rounds.Commands
{
    public class UpdateRoundTestIdCommandHandler : IRequestHandler<UpdateRoundTestIdCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateRoundTestIdCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateRoundTestIdCommand request, CancellationToken cancellationToken)
        {
            var round = await _unitOfWork.Rounds.GetByIdAsync(request.RoundId);
            if (round == null)
                throw new ApiException(ResponseCode.GET_UNSUCC);

            if (round.RoundType != CabinCrewCareer.Domain.Common.RoundType.Test)
                throw new ApiException((int)ResponseCode.FAIL, "INVALID_ROUNDTYPE", "Only rounds of type 'Test' can have their testId updated.");

            var test = await _unitOfWork.Tests.GetByIdAsync(request.TestId);
            if (test == null)
                throw new ApiException((int)ResponseCode.FAIL, "INVALID_TESTID", "TestId is invalid or does not exist.");

            round.TestId = request.TestId;
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
