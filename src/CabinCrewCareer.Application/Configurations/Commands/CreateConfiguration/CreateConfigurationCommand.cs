using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Configurations.Commands.CreateConfiguration
{
    public record CreateConfigurationCommand(
        int RoundTypeId,
        int Benchmark) : IRequest<Response<string>>;
}
