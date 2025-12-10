using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestTypes.Commands.CreateTestType
{
    public record CreateTestTypeCommand(string TestTypeName) : IRequest<Response<string>>;
}
