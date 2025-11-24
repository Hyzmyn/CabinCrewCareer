using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.UserApplications.Commands.Requests;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CabinCrewCareer.Application.UserApplications.Common;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class UpdateUserApplicationCommandHandler : IRequestHandler<UpdateUserApplicationCommand, Response<UserApplicationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUploadFileService _uploadFileService;
        private readonly IFirebaseStorageService _cloudinaryStorageService;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
        public UpdateUserApplicationCommandHandler(IUnitOfWork unitOfWork, IUploadFileService uploadFileService, IFirebaseStorageService cloudinaryStorageService, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _uploadFileService = uploadFileService;
            _cloudinaryStorageService = cloudinaryStorageService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Response<UserApplicationResponse>> Handle(UpdateUserApplicationCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
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
            // Chỉ cho phép user sở hữu hồ sơ được cập nhật
            if (application.UserId != userId)
                return new Response<UserApplicationResponse>
                {
                    Code = 401,
                    Message = "Unauthorized: You do not own this application.",
                    Data = null,
                    Errors = "Unauthorized"
                };

            // Cập nhật các trường thông tin
            if (request.Experience != null) application.Experience = request.Experience;
            if (request.Height.HasValue) application.Height = request.Height.Value;
            if (request.Weight.HasValue) application.Weight = request.Weight.Value;
            if (request.EnglishDegreeNumber != null) application.EnglishDegreeNumber = request.EnglishDegreeNumber;
            if (request.EndDate.HasValue) application.EndDate = request.EndDate.Value;

            // Cập nhật từng file nếu có
            var documents = application.Documents.ToList();
            async Task UpdateDocument(DocumentType type, IFormFile? newFile)
            {
                var doc = documents.FirstOrDefault(d => d.Type == type);
                if (newFile != null)
                {
                    // Xóa file cũ trên Cloudinary nếu có
                    if (doc != null && !string.IsNullOrEmpty(doc.DocumentURL))
                        await _cloudinaryStorageService.DeleteFileAsync(doc.DocumentURL);
                    var newUrl = await _uploadFileService.CloudinaryStorage(newFile);
                    if (doc != null)
                    {
                        doc.DocumentURL = newUrl;
                        doc.UploadDate = DateOnly.FromDateTime(DateTime.UtcNow);
                    }
                    else
                    {
                        documents.Add(new Document
                        {
                            DocumentURL = newUrl,
                            Type = type,
                            UploadDate = DateOnly.FromDateTime(DateTime.UtcNow),
                            IsVerified = false,
                            ApplicationId = application.ApplicationId
                        });
                    }
                }
            }
            await UpdateDocument(DocumentType.ApplicationForm, request.ApplicationForm);
            await UpdateDocument(DocumentType.ProfilePhoto, request.ProfilePhoto);
            await UpdateDocument(DocumentType.EducationDegree, request.EducationDegree);
            await UpdateDocument(DocumentType.EnglishCertificate, request.EnglishCertificate);
            await UpdateDocument(DocumentType.PassportOrID, request.PassportOrID);

            application.Documents = documents;
            await _unitOfWork.CompleteAsync();

            // --- Tạo RoundActivity trạng thái Ongoing cho user nếu có CampaignRoundId ---
            int? campaignRoundId = request.CampaignRoundId;
            if (campaignRoundId.HasValue)
            {
                var screeningRounds = await _unitOfWork.Rounds.GetAsync(r => r.CampaignRoundId == campaignRoundId.Value && r.RoundType == CabinCrewCareer.Domain.Common.RoundType.Screening);
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
                        Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.FAIL,
                        Message = "Không tìm thấy vòng sàng lọc (Screening) cho campaignRoundId này.",
                        Data = null,
                        Errors = "ROUND_SCREENING_NOT_FOUND"
                    };
                }
            }

            var response = new UserApplicationResponse
            {
                ApplicationId = application.ApplicationId,
                UserId = userId,
                Status = application.ApplicationStatus.ToString(),
                Message = "Draft application updated successfully.",
                Data = null
            };
            return new Response<UserApplicationResponse>
            {
                Code = 4, // 4 = UPDATED_SUCCESS
                Message = "Draft application updated successfully.",
                Data = response,
                Errors = null
            };
        }
    }
}
