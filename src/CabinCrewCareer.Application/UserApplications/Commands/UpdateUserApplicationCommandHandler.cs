using CabinCrewCareer.Application.Common.Interfaces;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.UserApplications.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class UpdateUserApplicationCommandHandler(
        IUnitOfWork unitOfWork,
        IUploadFileService uploadFileService,
        IFirebaseStorageService cloudinaryStorageService,
        IHttpContextAccessor httpContextAccessor,
        IDateTimeProvider dateTimeProvider,
        IOcrService ocrService) : IRequestHandler<UpdateUserApplicationCommand, Response<UserApplicationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly IFirebaseStorageService _cloudinaryStorageService = cloudinaryStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        public async Task<Response<UserApplicationResponse>> Handle(UpdateUserApplicationCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var application = await _unitOfWork.UserApplicationSnapshots.GetUserApplicationSnapshotByIdAsync(command.ApplicationId);
            if (application == null || application.ApplicationStatus != ApplicationStatus.Draft)
                return new Response<UserApplicationResponse>
                {
                    Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.GET_UNSUCC,
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

            var activityQuery = await _unitOfWork.RoundActivites.GetAsync(expression: x => x.UserId == userId && x.ApplicationSnapshotId == application.ApplicationSnapshotId);

            var activity = await activityQuery.FirstOrDefaultAsync();

            // Chỉ cho phép user sở hữu hồ sơ được cập nhật
            if (activity is null)
                return new Response<UserApplicationResponse>
                {
                    Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.FAILED_AUTHENTICATION,
                    Message = "Unauthorized: You do not own this application.",
                    Data = null,
                    Errors = "Unauthorized"
                };

            // Cập nhật các trường thông tin
            if (request.Experience != null) application.Experience = request.Experience;
            if (request.Height.HasValue) application.Height = request.Height.Value;
            if (request.Weight.HasValue) application.Weight = request.Weight.Value;


            // --- Xử lý cập nhật file và AI OCR cho CCCD và chứng chỉ tiếng Anh ---
            var documents = application.DocumentSnapshots.ToList();
            // Helper lấy document hiện tại
            DocumentSnapshot FindDoc(DocumentType type) => documents.FirstOrDefault(d => d.Type == type);

            // Xử lý CCCD (PassportOrID)
            var oldCccdDoc = FindDoc(DocumentType.PassportOrID);
            if (request.PassportOrID != null)
            {
                // Upload file mới, xóa file cũ nếu có
                if (oldCccdDoc != null && !string.IsNullOrEmpty(oldCccdDoc.DocumentURL))
                    await _cloudinaryStorageService.DeleteFileAsync(oldCccdDoc.DocumentURL);
                var newUrl = await _uploadFileService.CloudinaryStorage(request.PassportOrID);
                if (oldCccdDoc != null)
                {
                    oldCccdDoc.DocumentURL = newUrl;
                    oldCccdDoc.UploadDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }
                else
                {
                    documents.Add(new DocumentSnapshot
                    {
                        DocumentURL = newUrl,
                        Type = DocumentType.PassportOrID,
                        UploadDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        IsVerified = false,
                        ApplicationSnapshotId = application.ApplicationSnapshotId
                    });
                }
                // Gọi AI OCR để lấy CitizenId
                var ocrResult = await ocrService.ExtractTextFromImageAsync(request.PassportOrID);
                var match = System.Text.RegularExpressions.Regex.Match(ocrResult, "\\b\\d{12}\\b");
                if (match.Success)
                {
                    application.CitizenId = match.Value;
                }
                else
                {
                    return new Response<UserApplicationResponse>
                    {
                        Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.FAIL,
                        Message = "Không thể nhận diện số CCCD. Vui lòng kiểm tra lại ảnh (có thể số bị che hoặc ảnh bị mờ).",
                        Data = null,
                        Errors = "CCCD_FILE_INVALID"
                    };
                }
            }
            else if (oldCccdDoc != null && request.PassportOrID == null)
            {
                // Nếu xóa file CCCD, xóa file trên cloud và reset trường CitizenId
                await _cloudinaryStorageService.DeleteFileAsync(oldCccdDoc.DocumentURL);
                documents.Remove(oldCccdDoc);
                application.CitizenId = string.Empty;
            }
            // Nếu không upload file mới, giữ nguyên CitizenId

            // Xử lý chứng chỉ tiếng Anh
            var oldEngDoc = FindDoc(DocumentType.EnglishCertificate);
            if (request.EnglishCertificate != null)
            {
                // Upload file mới, xóa file cũ nếu có
                if (oldEngDoc != null && !string.IsNullOrEmpty(oldEngDoc.DocumentURL))
                    await _cloudinaryStorageService.DeleteFileAsync(oldEngDoc.DocumentURL);
                var newUrl = await _uploadFileService.CloudinaryStorage(request.EnglishCertificate);
                if (oldEngDoc != null)
                {
                    oldEngDoc.DocumentURL = newUrl;
                    oldEngDoc.UploadDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }
                else
                {
                    documents.Add(new DocumentSnapshot
                    {
                        DocumentURL = newUrl,
                        Type = DocumentType.EnglishCertificate,
                        UploadDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        IsVerified = false,
                        ApplicationSnapshotId = application.ApplicationSnapshotId
                    });
                }
                // Gọi AI OCR để lấy thông tin chứng chỉ tiếng Anh
                var ocrText = await ocrService.ExtractTextFromImageAsync(request.EnglishCertificate);
                if (string.IsNullOrWhiteSpace(ocrText) || !ocrText.ToUpper().Contains("TOEIC"))
                {
                    return new Response<UserApplicationResponse>
                    {
                        Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.FAIL,
                        Message = "File upload không phải là chứng chỉ tiếng Anh TOEIC hoặc ảnh quá mờ. Vui lòng upload đúng chứng chỉ TOEIC rõ nét.",
                        Data = null,
                        Errors = "ENGLISH_CERTIFICATE_INVALID"
                    };
                }
                // Trích xuất các trường
                var dateMatch = System.Text.RegularExpressions.Regex.Match(ocrText, "(\\d{4}/\\d{2}/\\d{2})");
                DateOnly? englishTestDate = null;
                if (dateMatch.Success)
                {
                    if (DateOnly.TryParseExact(dateMatch.Value, "yyyy/MM/dd", out var testDate))
                        englishTestDate = testDate;
                }
                var validUntilMatchAfter = System.Text.RegularExpressions.Regex.Match(ocrText, "Valid Until.*?(\\d{4}/\\d{2}/\\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var validUntilMatchBefore = System.Text.RegularExpressions.Regex.Match(ocrText, "(\\d{4}/\\d{2}/\\d{2})\\s*Valid Until", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                DateOnly? endDate = null;
                if (validUntilMatchAfter.Success)
                {
                    if (DateOnly.TryParseExact(validUntilMatchAfter.Groups[1].Value, "yyyy/MM/dd", out var validUntilDate))
                        endDate = validUntilDate;
                }
                else if (validUntilMatchBefore.Success)
                {
                    if (DateOnly.TryParseExact(validUntilMatchBefore.Groups[1].Value, "yyyy/MM/dd", out var validUntilDate))
                        endDate = validUntilDate;
                }
                var listenMatch = System.Text.RegularExpressions.Regex.Match(ocrText, "LISTENING\\s*(\\d{2,4})\\s*Your score", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                int listeningScore = 0;
                if (listenMatch.Success && int.TryParse(listenMatch.Groups[1].Value, out var listenScore))
                    listeningScore = listenScore;
                var readMatch = System.Text.RegularExpressions.Regex.Match(ocrText, "READING\\s*(\\d{2,4})\\s*Your score", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                int readingScore = 0;
                if (readMatch.Success && int.TryParse(readMatch.Groups[1].Value, out var readScore))
                    readingScore = readScore;
                int totalScore = 0;
                if (listeningScore > 0 && readingScore > 0)
                    totalScore = listeningScore + readingScore;
                string englishDegreeNumber = totalScore > 0 ? $"TOEIC {totalScore}" : string.Empty;
                if (englishTestDate == null || endDate == null || listeningScore == 0 || readingScore == 0)
                {
                    return new Response<UserApplicationResponse>
                    {
                        Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.FAIL,
                        Message = "Không thể nhận diện đủ thông tin từ ảnh chứng chỉ TOEIC. Vui lòng chụp lại ảnh rõ nét, không bị mờ hoặc thiếu thông tin.",
                        Data = null,
                        Errors = "ENGLISH_CERTIFICATE_UNREADABLE"
                    };
                }
                // Gán vào application
                application.EnglishDegreeNumber = englishDegreeNumber;
                application.EnglishTestDate = englishTestDate;
                application.ListeningScore = listeningScore;
                application.ReadingScore = readingScore;
                application.TotalScore = totalScore;
                application.EndDate = endDate;
            }
            else if (oldEngDoc != null && request.EnglishCertificate == null)
            {
                // Nếu xóa file chứng chỉ tiếng Anh, xóa file trên cloud và reset các trường liên quan
                await _cloudinaryStorageService.DeleteFileAsync(oldEngDoc.DocumentURL);
                documents.Remove(oldEngDoc);
                application.EnglishDegreeNumber = string.Empty;
                application.EnglishTestDate = null;
                application.ListeningScore = 0;
                application.ReadingScore = 0;
                application.TotalScore = 0;
                application.EndDate = null;
            }
            // Nếu không upload file mới, giữ nguyên các trường liên quan

            // Các file khác giữ logic cũ
            async Task UpdateDocument(DocumentType type, IFormFile? newFile)
            {
                var doc = documents.FirstOrDefault(d => d.Type == type);
                if (newFile != null)
                {
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
                        documents.Add(new DocumentSnapshot
                        {
                            DocumentURL = newUrl,
                            Type = type,
                            UploadDate = DateOnly.FromDateTime(DateTime.UtcNow),
                            IsVerified = false,
                            ApplicationSnapshotId = application.ApplicationSnapshotId
                        });
                    }
                }
            }
            await UpdateDocument(DocumentType.ApplicationForm, request.ApplicationForm);
            await UpdateDocument(DocumentType.ProfilePhoto, request.ProfilePhoto);
            await UpdateDocument(DocumentType.EducationDegree, request.EducationDegree);
            await UpdateDocument(DocumentType.PassportOrIDBack, request.PassportOrIDBack);

            application.DocumentSnapshots = documents;
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
                        Status = ActivityStatus.Ongoing,
                        ApplicationSnapshotId = application.ApplicationSnapshotId
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
                ApplicationId = application.ApplicationSnapshotId,
                UserId = userId,
                Status = application.ApplicationStatus.ToString(),
                Message = "Draft application updated successfully.",
                Data = null
            };
            return new Response<UserApplicationResponse>
            {
                Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.UPDATED_SUCCESS,
                Message = "Draft application updated successfully.",
                Data = response,
                Errors = null
            };
        }
    }
}
