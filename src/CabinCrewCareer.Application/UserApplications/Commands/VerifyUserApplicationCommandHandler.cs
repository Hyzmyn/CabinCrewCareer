using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Domain.Common;
using MediatR;
using CabinCrewCareer.Application.UserApplications.Common;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class VerifyUserApplicationCommandHandler : IRequestHandler<VerifyUserApplicationCommand, Response<UserApplicationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
        public VerifyUserApplicationCommandHandler(IUnitOfWork unitOfWork, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Response<UserApplicationResponse>> Handle(VerifyUserApplicationCommand command, CancellationToken cancellationToken)
        {
            var application = await _unitOfWork.UserApplications.GetUserApplicationByIdAsync(command.ApplicationId);
            if (application == null || application.ApplicationStatus != ApplicationStatus.Draft)
                return new Response<UserApplicationResponse>
                {
                    Code = 404,
                    Message = "Application not found or not in draft status.",
                    Data = null,
                    Errors = "Not found or invalid status"
                };

            // Lấy userId từ token
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userIdClaim != null)
                int.TryParse(userIdClaim.Value, out userId);
            // Chỉ cho phép user sở hữu hồ sơ xác thực
            if (application.UserId != userId)
                return new Response<UserApplicationResponse>
                {
                    Code = 401,
                    Message = "Unauthorized: You do not own this application.",
                    Data = null,
                    Errors = "Unauthorized"
                };

            // Validate đủ 5 file document
            var requiredTypes = new[] {
                DocumentType.ApplicationForm,
                DocumentType.ProfilePhoto,
                DocumentType.EducationDegree,
                DocumentType.EnglishCertificate,
                DocumentType.PassportOrID
            };
            var hasAllDocuments = requiredTypes.All(type => application.Documents.Any(d => d.Type == type && !string.IsNullOrEmpty(d.DocumentURL)));
            if (!hasAllDocuments)
                return new Response<UserApplicationResponse>
                {
                    Code = 400,
                    Message = "Application does not have all required documents.",
                    Data = null,
                    Errors = "Missing documents"
                };

            var updated = await _unitOfWork.UserApplications.UpdateApplicationStatusAsync(command.ApplicationId, ApplicationStatus.Submitted);
            if (!updated)
                return new Response<UserApplicationResponse>
                {
                    Code = 500,
                    Message = "Failed to update application status.",
                    Data = null,
                    Errors = "Update failed"
                };

            var response = new UserApplicationResponse
            {
                ApplicationId = application.ApplicationId,
                UserId = userId,
                Status = ApplicationStatus.Submitted.ToString(),
                Message = "Application submitted successfully.",
                Data = null
            };
            return new Response<UserApplicationResponse>
            {
                Code = 2, // 2 = CREATED_SUCCESS
                Message = "Application submitted successfully.",
                Data = response,
                Errors = null
            };
        }
    }
}
