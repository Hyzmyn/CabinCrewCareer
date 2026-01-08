using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Configurations.Commands.CreateConfiguration
{
    public class CreateConfigurationCommandHandler(
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateConfigurationCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<Response<string>> Handle(CreateConfigurationCommand request, CancellationToken cancellationToken)
        {
            var roundType = await _unitOfWork.RoundTypes.GetByIdAsync(request.RoundTypeId) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            switch ((RoundConfigurationType)roundType.RoundTypeId)
            {
                case RoundConfigurationType.Appearance:
                    if (await _unitOfWork.RoundConfigurations.RecruitmentAppearanceBenchmark() != request.Benchmark) break;
                    throw new ApiException(ResponseCode.INVALID_BENCHMARK);
                case RoundConfigurationType.EnglishListeningTest:
                    if (await _unitOfWork.RoundConfigurations.EnglishListeningTestBenchmark() != request.Benchmark) break;
                    throw new ApiException(ResponseCode.INVALID_BENCHMARK);
                case RoundConfigurationType.EnglishSpeakingTest:
                    if (await _unitOfWork.RoundConfigurations.EnglishSpeakingTestBenchmark() != request.Benchmark) break;
                    throw new ApiException(ResponseCode.INVALID_BENCHMARK);
                case RoundConfigurationType.RecruitmentInterview:
                    if (await _unitOfWork.RoundConfigurations.RecruitmentInterviewBenchmark() != request.Benchmark) break;
                    throw new ApiException(ResponseCode.INVALID_BENCHMARK);
                case RoundConfigurationType.PracticalTest:
                    if (await _unitOfWork.RoundConfigurations.PracticalTestBenchmark() != request.Benchmark) break;
                    throw new ApiException(ResponseCode.INVALID_BENCHMARK);
                case RoundConfigurationType.PromotionInterview:
                    if (await _unitOfWork.RoundConfigurations.PromotionInterviewBenchmark() != request.Benchmark) break;
                    throw new ApiException(ResponseCode.INVALID_BENCHMARK);
                default: break;
            }

            RoundConfiguration ward = new()
            {
                RoundTypeId = request.RoundTypeId,
                Benchmark = request.Benchmark,
                EffectiveDate = _dateTimeProvider.Today.AddDays(1)
            };

            await _unitOfWork.RoundConfigurations.AddAsync(ward);
            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.CREATED_SUCCESS, data: ResponseCode.CREATED_SUCCESS.ToString(), message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
