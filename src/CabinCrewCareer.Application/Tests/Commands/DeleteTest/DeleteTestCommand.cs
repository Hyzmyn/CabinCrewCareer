using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Tests.Commands.DeleteTest
{
    public record DeleteTestCommand(int TestId) : IRequest<Response<bool>>;
}