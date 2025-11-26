using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Extensions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
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
            UserApplication? application = new();

            var result = await _unitOfWork.Documents.GetByIdAsync(request.DocumentId);

            if (result is null) throw new ApiException(ResponseCode.DOCUMENT_NOT_FOUND);

            if (result.Type == DocumentType.EnglishCertificate)
            {
                if (result.ApplicationId is not null)
                {
                    application = await _unitOfWork.UserApplications.GetUserApplicationByIdAsync((int)result.ApplicationId);

                    if (application is not null)
                    {
                        application.EnglishTestDate = null;
                        application.EnglishDegreeNumber = string.Empty;
                        application.EndDate = null;
                        application.ListeningScore = 0;
                        application.ReadingScore = 0;
                        application.TotalScore = 0;

                        await _unitOfWork.UserApplications.UpdateAsync(application);
                    }
                }

            }

            if (result.Type == DocumentType.PassportOrID)
            {
                if (result.ApplicationId is not null)
                {
                    application = await _unitOfWork.UserApplications.GetUserApplicationByIdAsync((int)result.ApplicationId);

                    if (application is not null)
                    {
                        application.CitizenId = string.Empty;

                        await _unitOfWork.UserApplications.UpdateAsync(application);
                    }
                }
            }

            await _unitOfWork.Documents.DeleteAsync(result);

            await _unitOfWork.CompleteAsync();


            return new Response<string>(code: (int)ResponseCode.DELETED_SUCCESS,
                data: ResponseCode.DELETED_SUCCESS.ToString(),
                message: ResponseCode.DELETED_SUCCESS.GetDescription());
        }
    }
}
