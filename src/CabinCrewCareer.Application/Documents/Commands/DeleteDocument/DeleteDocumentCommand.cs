using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Documents.Commands.DeleteDocument
{
    public record DeleteDocumentCommand(int DocumentId) : IRequest<Response<string>>;
}
