using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
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
            var userQuery = await _unitOfWork.RoundConfigurations.GetAsync(user => user.RoundConfigurationId == request.ConfigurationId);

            var user = userQuery
                .FirstOrDefault() ?? throw new ApiException(ResponseCode.USER_NOT_FOUND);

            if (user.Benchmark == request.Benchmark) throw new ApiException(ResponseCode.INVALID_BENCHMARK);

            RoundConfiguration ward = new()
            {
                CampaignType = request.CampaignType,
                ConfigurationType = request.ConfigurationType,
                Benchmark = request.Benchmark,
                EffectiveDate = _dateTimeProvider.Today.AddDays(1)
            };

            await _unitOfWork.RoundConfigurations.AddAsync(ward);
            await _unitOfWork.CompleteAsync();

            return new Response<string>(code: (int)ResponseCode.CREATED_SUCCESS, data: ResponseCode.CREATED_SUCCESS.ToString(), message: ResponseCode.CREATED_SUCCESS.GetDescription());
        }
    }
}
