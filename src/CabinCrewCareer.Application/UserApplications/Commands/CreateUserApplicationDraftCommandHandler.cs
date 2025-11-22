using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.UserApplications.Commands.Requests;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using MediatR;
using CabinCrewCareer.Application.UserApplications.Common;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class CreateUserApplicationDraftCommandHandler : IRequestHandler<CreateUserApplicationDraftCommand, Response<UserApplicationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUploadFileService _uploadFileService;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
        public CreateUserApplicationDraftCommandHandler(IUnitOfWork unitOfWork, IUploadFileService uploadFileService, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _uploadFileService = uploadFileService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Response<UserApplicationResponse>> Handle(CreateUserApplicationDraftCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var applicationFormUrl = request.ApplicationForm != null ? await _uploadFileService.CloudinaryStorage(request.ApplicationForm) : null;
            var profilePhotoUrl = request.ProfilePhoto != null ? await _uploadFileService.CloudinaryStorage(request.ProfilePhoto) : null;
            var educationDegreeUrl = request.EducationDegree != null ? await _uploadFileService.CloudinaryStorage(request.EducationDegree) : null;
            var englishCertificateUrl = request.EnglishCertificate != null ? await _uploadFileService.CloudinaryStorage(request.EnglishCertificate) : null;
            var passportOrIdUrl = request.PassportOrID != null ? await _uploadFileService.CloudinaryStorage(request.PassportOrID) : null;

            // Lấy userId từ token
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userIdClaim != null)
                int.TryParse(userIdClaim.Value, out userId);

            var application = new UserApplication
            {
                ApplicationStatus = ApplicationStatus.Draft,
                Experience = request.Experience,
                Height = request.Height,
                Weight = request.Weight,
                EnglishDegreeNumber = request.EnglishDegreeNumber,
                EndDate = request.EndDate,
                SubmissionDate = null,
                UserId = userId
            };

            var documents = new List<Document>();
            if (applicationFormUrl != null)
                documents.Add(new Document { DocumentURL = applicationFormUrl, Type = DocumentType.ApplicationForm, IsVerified = false });
            if (profilePhotoUrl != null)
                documents.Add(new Document { DocumentURL = profilePhotoUrl, Type = DocumentType.ProfilePhoto, IsVerified = false });
            if (educationDegreeUrl != null)
                documents.Add(new Document { DocumentURL = educationDegreeUrl, Type = DocumentType.EducationDegree, IsVerified = false });
            if (englishCertificateUrl != null)
                documents.Add(new Document { DocumentURL = englishCertificateUrl, Type = DocumentType.EnglishCertificate, IsVerified = false });
            if (passportOrIdUrl != null)
                documents.Add(new Document { DocumentURL = passportOrIdUrl, Type = DocumentType.PassportOrID, IsVerified = false });

            var result = await _unitOfWork.UserApplications.CreateUserApplicationAsync(application, documents);

            var response = new UserApplicationResponse
            {
                ApplicationId = result.ApplicationId,
                UserId = userId,
                Status = application.ApplicationStatus.ToString(),
                Message = "Created draft application successfully.",
                Data = null
            };
            return new Response<UserApplicationResponse>
            {
                Code = 2, // 2 = CREATED_SUCCESS
                Message = "Created draft application successfully.",
                Data = response,
                Errors = null
            };
        }
    }
}
