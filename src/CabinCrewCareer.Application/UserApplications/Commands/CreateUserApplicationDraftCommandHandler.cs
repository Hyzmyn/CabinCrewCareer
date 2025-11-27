using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.UserApplications.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class CreateUserApplicationDraftCommandHandler(
        IUnitOfWork unitOfWork,
        IUploadFileService uploadFileService,
        IHttpContextAccessor httpContextAccessor,IOcrService ocrService) : IRequestHandler<CreateUserApplicationDraftCommand, Response<UserApplicationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<Response<UserApplicationResponse>> Handle(CreateUserApplicationDraftCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            // Lấy userId từ token
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
                    Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.FAIL,
                    Message = "You already have an application. Cannot create a new one.",
                    Data = null,
                    Errors = "APPLICATION_ALREADY_EXISTS"
                };
            }

            var applicationFormUrl = request.ApplicationForm != null ? await _uploadFileService.CloudinaryStorage(request.ApplicationForm) : null;
            var profilePhotoUrl = request.ProfilePhoto != null ? await _uploadFileService.CloudinaryStorage(request.ProfilePhoto) : null;
            var educationDegreeUrl = request.EducationDegree != null ? await _uploadFileService.CloudinaryStorage(request.EducationDegree) : null;
            var englishCertificateUrl = request.EnglishCertificate != null ? await _uploadFileService.CloudinaryStorage(request.EnglishCertificate) : null;
            string citizenId = string.Empty;
                DateOnly? englishTestDate = null;
                int listeningScore = 0;
                int readingScore = 0;
                int totalScore = 0;
                DateOnly? endDate = null;
                string? englishDegreeNumber = null;
            var passportOrIdUrl = request.PassportOrID != null ? await _uploadFileService.CloudinaryStorage(request.PassportOrID) : null;

             if (request.PassportOrID != null)
                {
                    var ocrResult = await ocrService.ExtractTextFromImageAsync(request.PassportOrID);
                    Console.WriteLine($"OCR TEXT: {ocrResult}");
                    var match = System.Text.RegularExpressions.Regex.Match(ocrResult, @"\b\d{12}\b");
                    if (match.Success)
                    {
                        citizenId = match.Value;
                    }
                     else
                    {
                        return new Response<UserApplicationResponse>
                        {
                            Code = (int)ResponseCode.FAIL,
                            Message = "Không thể nhận diện số CCCD. Vui lòng kiểm tra lại ảnh (có thể số bị che hoặc ảnh bị mờ).",
                            Data = null,
                            Errors = "CCCD_FILE_INVALID"
                        };
                    }
                }

                // AI OCR cho chứng chỉ tiếng Anh
                if (request.EnglishCertificate != null)
                {
                    var ocrText = await ocrService.ExtractTextFromImageAsync(request.EnglishCertificate);
                    Console.WriteLine($"ENGLISH CERT OCR: {ocrText}");
                    // Check if OCR result contains TOEIC keyword
                    if (string.IsNullOrWhiteSpace(ocrText) || !ocrText.ToUpper().Contains("TOEIC"))
                    {
                        return new Response<UserApplicationResponse>
                        {
                            Code = (int)ResponseCode.FAIL,
                            Message = "File upload không phải là chứng chỉ tiếng Anh TOEIC hoặc ảnh quá mờ. Vui lòng upload đúng chứng chỉ TOEIC rõ nét.",
                            Data = null,
                            Errors = "ENGLISH_CERTIFICATE_INVALID"
                        };
                    }
                    // Ngày thi: yyyy/MM/dd
                    var dateMatch = System.Text.RegularExpressions.Regex.Match(ocrText, @"(\d{4}/\d{2}/\d{2})");
                    if (dateMatch.Success)
                    {
                        if (DateOnly.TryParseExact(dateMatch.Value, "yyyy/MM/dd", out var testDate))
                            englishTestDate = testDate;
                    }
                     // Valid Until: yyyy/MM/dd (date may appear before or after 'Valid Until')
                    var validUntilMatchAfter = System.Text.RegularExpressions.Regex.Match(ocrText, @"Valid Until.*?(\d{4}/\d{2}/\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    var validUntilMatchBefore = System.Text.RegularExpressions.Regex.Match(ocrText, @"(\d{4}/\d{2}/\d{2})\s*Valid Until", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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
                    // Tìm tất cả số sau 'Your score'
                    // Điểm nghe
                    var listenMatch = System.Text.RegularExpressions.Regex.Match(ocrText, @"LISTENING\s*(\d{2,4})\s*Your score", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (listenMatch.Success && int.TryParse(listenMatch.Groups[1].Value, out var listenScore))
                        listeningScore = listenScore;
                    // Điểm đọc
                    var readMatch = System.Text.RegularExpressions.Regex.Match(ocrText, @"READING\s*(\d{2,4})\s*Your score", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (readMatch.Success && int.TryParse(readMatch.Groups[1].Value, out var readScore))
                        readingScore = readScore;
                    if (listeningScore > 0 && readingScore > 0)
                        totalScore = listeningScore + readingScore;
                        // EnglishDegreeNumber = TOEIC + totalScore
                                    if (totalScore > 0)
                                        englishDegreeNumber = $"TOEIC {totalScore}";
                         // Nếu không lấy được ngày thi, ngày hết hạn, điểm nghe, điểm đọc thì báo lỗi
                    if (englishTestDate == null || endDate == null || listeningScore == 0 || readingScore == 0)
                    {
                        return new Response<UserApplicationResponse>
                        {
                            Code = (int)ResponseCode.FAIL,
                            Message = "Không thể nhận diện đủ thông tin từ ảnh chứng chỉ TOEIC. Vui lòng chụp lại ảnh rõ nét, không bị mờ hoặc thiếu thông tin.",
                            Data = null,
                            Errors = "ENGLISH_CERTIFICATE_UNREADABLE"
                        };
                    }
                }
                
            var application = new UserApplication
            {
                ApplicationStatus = ApplicationStatus.Draft,
                Experience = request.Experience,
                Height = request.Height,
                Weight = request.Weight,
                EnglishDegreeNumber = englishDegreeNumber,
                EndDate = endDate,
                SubmissionDate = null,
                UserId = userId,
                CitizenId = citizenId,
                EnglishTestDate = englishTestDate,
                ListeningScore = listeningScore,
                ReadingScore = readingScore,
                TotalScore = totalScore
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
                Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.CREATED_SUCCESS,
                Message = "Created draft application successfully.",
                Data = response,
                Errors = null
            };
        }
    }
}
