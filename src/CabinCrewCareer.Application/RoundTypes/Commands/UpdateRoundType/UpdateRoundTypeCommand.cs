using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundTypes.Commands.UpdateRoundType
{
    public record UpdateRoundTypeCommand(int RoundTypeId, string RoundTypeName) : IRequest<Response<string>>;
}
