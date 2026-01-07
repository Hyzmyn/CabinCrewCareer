using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Configurations.Commands.CreateConfiguration
{
    public record CreateConfigurationCommand(
        int ConfigurationId,
        CampaignType CampaignType,
        RoundConfigurationType ConfigurationType,
        int Benchmark) : IRequest<Response<string>>;
}
