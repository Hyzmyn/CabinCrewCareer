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
        IHttpContextAccessor httpContextAccessor, IOcrService ocrService,
        IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateUserApplicationDraftCommand, Response<UserApplicationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IOcrService _ocrService = ocrService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

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
            //var existedApplication = await _unitOfWork.UserApplications.GetUserApplicationByUserIdAsync(userId);
            //if (existedApplication != null)
            //{
            //    return new Response<UserApplicationResponse>
            //    {
            //        Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.FAIL,
            //        Message = "You already have an application. Cannot create a new one.",
            //        Data = null,
            //        Errors = "APPLICATION_ALREADY_EXISTS"
            //    };
            //}

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
            string englishDegreeNumber = string.Empty;
            var passportOrIdUrl = request.PassportOrID != null ? await _uploadFileService.CloudinaryStorage(request.PassportOrID) : null;
            var passportOrIdBackUrl = request.PassportOrIDBack != null ? await _uploadFileService.CloudinaryStorage(request.PassportOrIDBack) : null;
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
                // Check if OCR result contains TOEIC keyword (handle OCR misreads like TOE/C, TOEJC, etc.)
                var normalizedText = ocrText?.ToUpper().Replace("/", "I").Replace("J", "I") ?? "";
                var isToeic = normalizedText.Contains("TOEIC") || 
                              ocrText?.ToUpper().Contains("TOE") == true && 
                              (ocrText.ToUpper().Contains("LISTENING") || ocrText.ToUpper().Contains("READING") || 
                               ocrText.ToUpper().Contains("OFFICIAL SCORE"));
                
                if (string.IsNullOrWhiteSpace(ocrText) || !isToeic)
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
                // Handle OCR typos: Uniil, Unlil, Untll, etc.
                var validUntilMatchAfter = System.Text.RegularExpressions.Regex.Match(ocrText, @"Valid\s*U[nml][itl][itl][il]l?.*?(\d{4}/\d{2}/\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var validUntilMatchBefore = System.Text.RegularExpressions.Regex.Match(ocrText, @"(\d{4}/\d{2}/\d{2})\s*Valid\s*U[nml][itl][itl][il]l?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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
                // Điểm nghe - handle OCR errors like LISTENINGI, LISTENNG, etc.
                var listenMatch = System.Text.RegularExpressions.Regex.Match(ocrText, @"LISTEN[ING]+[\s\S]{0,20}?(\d{2,4})[\s\S]{0,20}?Your score", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (listenMatch.Success && int.TryParse(listenMatch.Groups[1].Value, out var listenScore))
                    listeningScore = listenScore;
                // Điểm đọc - handle OCR errors like READNG, READINC, etc.
                var readMatch = System.Text.RegularExpressions.Regex.Match(ocrText, @"READ[ING]+[\s\S]{0,20}?(\d{2,4})[\s\S]{0,20}?Your score", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (readMatch.Success && int.TryParse(readMatch.Groups[1].Value, out var readScore))
                    readingScore = readScore;
                // Tổng điểm
                var totalScoreMatch = System.Text.RegularExpressions.Regex.Match(ocrText, @"TOTAL\s*.*?(\d{2,4})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (totalScoreMatch.Success && int.TryParse(totalScoreMatch.Groups[1].Value, out var tScore))
                    totalScore = tScore;
                // EnglishDegreeNumber = TOEIC + totalScore
                 // Chỉ lấy loại chứng chỉ từ text OCR
                if (!string.IsNullOrWhiteSpace(ocrText))
                {
                    var certType = "";
                    var upperText = ocrText.ToUpper();
                    if (upperText.Contains("TOEIC")) certType = "TOEIC";
                    else if (upperText.Contains("IELTS")) certType = "IELTS";
                    else if (upperText.Contains("TOEFL")) certType = "TOEFL";
                    else if (upperText.Contains("CEFR")) certType = "CEFR";
                    // Có thể bổ sung thêm các loại khác nếu cần
                    englishDegreeNumber = certType;
                }
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

            //var application = new UserApplication
            //{
            //    ApplicationStatus = ApplicationStatus.Draft,
            //    Experience = request.Experience,
            //    Height = request.Height,
            //    Weight = request.Weight,
            //    EnglishDegreeNumber = englishDegreeNumber,
            //    EndDate = endDate,
            //    SubmissionDate = null,
            //    UserId = userId,
            //    CitizenId = citizenId,
            //    EnglishTestDate = englishTestDate,
            //    ListeningScore = listeningScore,
            //    ReadingScore = readingScore,
            //    TotalScore = totalScore
            //};

            //var documents = new List<Document>();
            //if (applicationFormUrl != null)
            //    documents.Add(new Document { DocumentURL = applicationFormUrl, Type = DocumentType.ApplicationForm, IsVerified = false });
            //if (profilePhotoUrl != null)
            //    documents.Add(new Document { DocumentURL = profilePhotoUrl, Type = DocumentType.ProfilePhoto, IsVerified = false });
            //if (educationDegreeUrl != null)
            //    documents.Add(new Document { DocumentURL = educationDegreeUrl, Type = DocumentType.EducationDegree, IsVerified = false });
            //if (englishCertificateUrl != null)
            //    documents.Add(new Document { DocumentURL = englishCertificateUrl, Type = DocumentType.EnglishCertificate, IsVerified = false });
            //if (passportOrIdUrl != null)
            //    documents.Add(new Document { DocumentURL = passportOrIdUrl, Type = DocumentType.PassportOrID, IsVerified = false });

            //var result = await _unitOfWork.UserApplications.CreateUserApplicationAsync(application, documents);

            var userAppSnapshot = new UserApplicationSnapshot
            {
                ApplicationStatus = ApplicationStatus.Submitted,
                SubmissionDate = _dateTimeProvider.UtcNow,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Height = request.Height,
                Weight = request.Weight,
                EnglishDegreeNumber = englishDegreeNumber,
                EndDate = endDate,
                CitizenId = citizenId,
                EnglishTestDate = englishTestDate,
                ListeningScore = listeningScore,
                ReadingScore = readingScore,
                TotalScore = totalScore
            };
            var documents = new List<DocumentSnapshot>();
            if (applicationFormUrl != null)
                documents.Add(new DocumentSnapshot { DocumentURL = applicationFormUrl, Type = DocumentType.ApplicationForm, IsVerified = true });
            if (profilePhotoUrl != null)
                documents.Add(new DocumentSnapshot { DocumentURL = profilePhotoUrl, Type = DocumentType.ProfilePhoto, IsVerified = true });
            if (educationDegreeUrl != null)
                documents.Add(new DocumentSnapshot { DocumentURL = educationDegreeUrl, Type = DocumentType.EducationDegree, IsVerified = true });
            if (englishCertificateUrl != null)
                documents.Add(new DocumentSnapshot { DocumentURL = englishCertificateUrl, Type = DocumentType.EnglishCertificate, IsVerified = true });
            if (passportOrIdUrl != null)
                documents.Add(new DocumentSnapshot { DocumentURL = passportOrIdUrl, Type = DocumentType.PassportOrID, IsVerified = true });
            if (passportOrIdBackUrl != null)
                documents.Add(new DocumentSnapshot { DocumentURL = passportOrIdBackUrl, Type = DocumentType.PassportOrIDBack, IsVerified = true });

            var snapshotResult = await _unitOfWork.UserApplicationSnapshots.AddAsync(userAppSnapshot);
            await _unitOfWork.CompleteAsync();

            // Tạo các DocumentSnapshot
            var documentSnapshots = documents.Select(doc => new DocumentSnapshot
            {
                DocumentURL = doc.DocumentURL,
                Type = doc.Type,
                UploadDate = _dateTimeProvider.Today,
                IsVerified = doc.IsVerified,
                ApplicationSnapshotId = snapshotResult.ApplicationSnapshotId
            }).ToList();
            if (documentSnapshots.Count > 0)
            {
                await _unitOfWork.DocumentSnapshots.AddRangeAsync(documentSnapshots);
                await _unitOfWork.CompleteAsync();
            }

            var response = new UserApplicationResponse
            {
                ApplicationId = userAppSnapshot.ApplicationSnapshotId,
                UserId = userId,
                Status = userAppSnapshot.ApplicationStatus.ToString(),
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
