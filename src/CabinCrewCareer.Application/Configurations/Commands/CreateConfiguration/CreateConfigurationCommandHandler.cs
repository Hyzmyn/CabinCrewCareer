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

            var round = await _unitOfWork.RoundConfigurations.GetCurrentlyEffectiveConfiguration(request.RoundTypeId) ?? throw new ApiException(ResponseCode.ROUND_NOT_FOUND);

            // Check if today <= EffectiveDate of the latest configuration
            if (round.EffectiveDate.HasValue && _dateTimeProvider.Today <= round.EffectiveDate.Value)
            {
                throw new ApiException(ResponseCode.INVALID_EFFECTIVE_DATE);
            }

            // Set old configuration's ExpiredDate = Today
            round.Status = false;
            round.ExpiredDate = _dateTimeProvider.Today;

            await _unitOfWork.RoundConfigurations.UpdateAsync(round);

            // New EffectiveDate = Today + 1
            var newEffectiveDate = _dateTimeProvider.Today;

            RoundConfiguration ward = new()
            {
                RoundTypeId = request.RoundTypeId,
                Benchmark = request.Benchmark,
                EffectiveDate = newEffectiveDate,
                ExpiredDate = null // ExpiredDate is null when creating, will be set when next configuration is created
            };

            // Validation: ExpiredDate should be greater than EffectiveDate (this will be checked when updating)
            // For new configuration, ExpiredDate is null, so no validation needed at creation time

            await _unitOfWork.RoundConfigurations.AddAsync(ward);
            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.CREATED_SUCCESS, data: ResponseCode.CREATED_SUCCESS.ToString(), message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
