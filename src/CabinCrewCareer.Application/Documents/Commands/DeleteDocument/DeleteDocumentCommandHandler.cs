using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandHandler(
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteDocumentCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Response<string>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.Documents.GetByIdAsync(request.DocumentId);

            if (result is not null) await _unitOfWork.Documents.DeleteAsync(result);

            return new Response<string>(code: (int)ResponseCode.DELETED_SUCCESS,
                data: ResponseCode.DELETED_SUCCESS.ToString(),
                message: ResponseCode.DELETED_SUCCESS.GetDescription());
        }
    }
}
