using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.TestTypes.Commands.DeleteTestType
{
    public record DeleteTestTypeCommand(int TestTypeId) : IRequest<Response<string>>;
}
