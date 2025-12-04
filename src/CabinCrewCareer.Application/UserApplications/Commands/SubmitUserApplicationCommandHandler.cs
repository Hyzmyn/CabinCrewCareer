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
    public class SubmitUserApplicationCommandHandler(
        IUnitOfWork unitOfWork,
        IUploadFileService uploadFileService,
        IHttpContextAccessor httpContextAccessor,
        IDateTimeProvider dateTimeProvider,
        IOcrService ocrService) : IRequestHandler<SubmitUserApplicationCommand, Response<UserApplicationResponse>>
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
            //var existedApplication = await _unitOfWork.UserApplications.GetUserApplicationByUserIdAsync(userId);
            //if (existedApplication != null)
            //{
            //    return new Response<UserApplicationResponse>
            //    {
            //        Code = (int)ResponseCode.FAIL,
            //        Message = "You already have an application. Cannot create a new one.",
            //        Data = null,
            //        Errors = "APPLICATION_ALREADY_EXISTS"
            //    };
            //}

            // Check for all 5 required files
            if (request.ApplicationForm == null || request.ProfilePhoto == null || request.EducationDegree == null || request.EnglishCertificate == null || request.PassportOrID == null)
                return new Response<UserApplicationResponse>
                {
                    Code = (int)ResponseCode.FAIL,
                    Message = "You must submit all 6 required documents to submit your application.",
                    Data = null,
                    Errors = "Missing required documents"
                };

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
                // Check if OCR result contains TOEIC keyword
                if (string.IsNullOrWhiteSpace(ocrText) || !ocrText.ToUpper().Contains("TOEIC"))
                {
                    return new Response<UserApplicationResponse>
                    {
                        Code = (int)ResponseCode.FAIL,
                        Message = "File upload không phải là chứng chỉ tiếng Anh TOEIC . Vui lòng upload đúng chứng chỉ TOEIC rõ nét.",
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
                        Message = "Không thể nhận diện đủ thông tin từ ảnh chứng chỉ TOEIC hoặc ảnh có thể bị mờ. Vui lòng chụp lại ảnh rõ nét, không bị mờ hoặc thiếu thông tin.",
                        Data = null,
                        Errors = "ENGLISH_CERTIFICATE_UNREADABLE"
                    };
                }
            }

            // var application = new UserApplication
            // {
            //     ApplicationStatus = ApplicationStatus.Submitted,
            //     Experience = request.Experience,
            //     Height = request.Height,
            //     Weight = request.Weight,
            //     EnglishDegreeNumber = englishDegreeNumber,
            //     EndDate = endDate,
            //     SubmissionDate = _dateTimeProvider.UtcNow,
            //     UserId = userId,
            //     CitizenId = citizenId,
            //     EnglishTestDate = englishTestDate,
            //     ListeningScore = listeningScore,
            //     ReadingScore = readingScore,
            //     TotalScore = totalScore
            // };

            // var documents = new List<Document>();
            // if (applicationFormUrl != null)
            //     documents.Add(new Document { DocumentURL = applicationFormUrl, Type = DocumentType.ApplicationForm, IsVerified = true });
            // if (profilePhotoUrl != null)
            //     documents.Add(new Document { DocumentURL = profilePhotoUrl, Type = DocumentType.ProfilePhoto, IsVerified = true });
            // if (educationDegreeUrl != null)
            //     documents.Add(new Document { DocumentURL = educationDegreeUrl, Type = DocumentType.EducationDegree, IsVerified = true });
            // if (englishCertificateUrl != null)
            //     documents.Add(new Document { DocumentURL = englishCertificateUrl, Type = DocumentType.EnglishCertificate, IsVerified = true });
            // if (passportOrIdUrl != null)
            //     documents.Add(new Document { DocumentURL = passportOrIdUrl, Type = DocumentType.PassportOrID, IsVerified = true });
            // if (passportOrIdBackUrl != null)
            //     documents.Add(new Document { DocumentURL = passportOrIdBackUrl, Type = DocumentType.PassportOrIDBack, IsVerified = true });


            // Tạo snapshot cho hồ sơ ứng viên
            var userAppSnapshot = new UserApplicationSnapshot
            {
                ApplicationStatus = ApplicationStatus.Submitted,
                SubmissionDate = _dateTimeProvider.UtcNow,
                Experience = request.Experience,
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

            // var result = await _unitOfWork.UserApplications.CreateUserApplicationAsync(application, documents);

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
                        Status = ActivityStatus.Ongoing,
                        ApplicationSnapshotId = snapshotResult.ApplicationSnapshotId // Gắn snapshot vào activity
                    };

                    UserHistory history = new()
                    {
                        UserId = userId,
                        CampaignRoundId = campaignRoundId.Value,
                        Type = HistoryType.Ongoing,
                        ParticipatedDate = _dateTimeProvider.Today
                    };

                    await _unitOfWork.RoundActivites.AddAsync(roundActivity);
                    await _unitOfWork.UserHistories.AddAsync(history);
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
                ApplicationId = snapshotResult.ApplicationSnapshotId,
                UserId = userId,
                Status = snapshotResult.ApplicationStatus.ToString(),
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
