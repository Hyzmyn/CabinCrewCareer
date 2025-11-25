using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.UserApplications.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class SubmitUserApplicationCommandHandler(
        IUnitOfWork unitOfWork,
        IUploadFileService uploadFileService,
        IHttpContextAccessor httpContextAccessor,
        IDateTimeProvider dateTimeProvider) : IRequestHandler<SubmitUserApplicationCommand, Response<UserApplicationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<Response<UserApplicationResponse>> Handle(SubmitUserApplicationCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Get userId from token
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userIdClaim != null)
                int.TryParse(userIdClaim.Value, out userId);

            // Kiểm tra hồ sơ đã tồn tại chưa
            var existedApplication = await _unitOfWork.UserApplications.GetUserApplicationByUserIdAsync(userId);
            if (existedApplication != null)
            {
                return new Response<UserApplicationResponse>
                {
                    Code = (int)ResponseCode.FAIL,
                    Message = "You already have an application. Cannot create a new one.",
                    Data = null,
                    Errors = "APPLICATION_ALREADY_EXISTS"
                };
            }

            // Check for all 5 required files
            if (request.ApplicationForm == null || request.ProfilePhoto == null || request.EducationDegree == null || request.EnglishCertificate == null || request.PassportOrID == null)
                return new Response<UserApplicationResponse>
                {
                    Code = (int)ResponseCode.FAIL,
                    Message = "You must submit all 5 required documents to submit your application.",
                    Data = null,
                    Errors = "Missing required documents"
                };

            var applicationFormUrl = request.ApplicationForm != null ? await _uploadFileService.CloudinaryStorage(request.ApplicationForm) : null;
            var profilePhotoUrl = request.ProfilePhoto != null ? await _uploadFileService.CloudinaryStorage(request.ProfilePhoto) : null;
            var educationDegreeUrl = request.EducationDegree != null ? await _uploadFileService.CloudinaryStorage(request.EducationDegree) : null;
            var englishCertificateUrl = request.EnglishCertificate != null ? await _uploadFileService.CloudinaryStorage(request.EnglishCertificate) : null;
            var passportOrIdUrl = request.PassportOrID != null ? await _uploadFileService.CloudinaryStorage(request.PassportOrID) : null;

            var application = new UserApplication
            {
                ApplicationStatus = ApplicationStatus.Submitted,
                Experience = request.Experience,
                Height = request.Height,
                Weight = request.Weight,
                EnglishDegreeNumber = request.EnglishDegreeNumber,
                EndDate = request.EndDate,
                SubmissionDate = _dateTimeProvider.UtcNow,
                UserId = userId
            };

            var documents = new List<Document>();
            if (applicationFormUrl != null)
                documents.Add(new Document { DocumentURL = applicationFormUrl, Type = DocumentType.ApplicationForm, IsVerified = true });
            if (profilePhotoUrl != null)
                documents.Add(new Document { DocumentURL = profilePhotoUrl, Type = DocumentType.ProfilePhoto, IsVerified = true });
            if (educationDegreeUrl != null)
                documents.Add(new Document { DocumentURL = educationDegreeUrl, Type = DocumentType.EducationDegree, IsVerified = true });
            if (englishCertificateUrl != null)
                documents.Add(new Document { DocumentURL = englishCertificateUrl, Type = DocumentType.EnglishCertificate, IsVerified = true });
            if (passportOrIdUrl != null)
                documents.Add(new Document { DocumentURL = passportOrIdUrl, Type = DocumentType.PassportOrID, IsVerified = true });

            var result = await _unitOfWork.UserApplications.CreateUserApplicationAsync(application, documents);

            // --- Tạo RoundActivity trạng thái Ongoing cho user ---
            // Nếu request có campaignRoundId, thực hiện logic tự động
            int? campaignRoundId = null;
            var campaignRoundIdProp = request.GetType().GetProperty("CampaignRoundId");
            if (campaignRoundIdProp != null)
            {
                var value = campaignRoundIdProp.GetValue(request);
                if (value != null && int.TryParse(value.ToString(), out int crId))
                    campaignRoundId = crId;
            }
            if (campaignRoundId.HasValue)
            {
                // Tìm round sàng lọc (Screening) thuộc campaignRoundId
                var screeningRounds = await _unitOfWork.Rounds.GetAsync(r => r.CampaignRoundId == campaignRoundId.Value && r.RoundType == RoundType.Screening);
                var screeningRound = screeningRounds?.FirstOrDefault();
                if (screeningRound != null)
                {
                    var roundActivity = new RoundActivity
                    {
                        RoundId = screeningRound.RoundId,
                        UserId = userId,
                        Status = ActivityStatus.Ongoing
                    };
                    await _unitOfWork.RoundActivites.AddAsync(roundActivity);
                    await _unitOfWork.CompleteAsync();
                }
                else
                {
                    return new Response<UserApplicationResponse>
                    {
                        Code = (int)ResponseCode.FAIL,
                        Message = "Không tìm thấy vòng sàng lọc (Screening) cho campaignRoundId này.",
                        Data = null,
                        Errors = "ROUND_SCREENING_NOT_FOUND"
                    };
                }
            }

            var response = new UserApplicationResponse
            {
                ApplicationId = result.ApplicationId,
                UserId = userId,
                Status = application.ApplicationStatus.ToString(),
                Message = "Submitted application successfully.",
                Data = null
            };
            return new Response<UserApplicationResponse>
            {
                Code = (int)ResponseCode.CREATED_SUCCESS,
                Message = "Application submitted successfully.",
                Data = response,
                Errors = null
            };
        }
    }
}
