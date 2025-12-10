using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.RoundTypes.Commands.DeleteRoundType
{
    public record DeleteRoundTypeCommand(int RoundTypeId) : IRequest<Response<string>>;
}
