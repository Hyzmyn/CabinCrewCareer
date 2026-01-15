using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Interfaces;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.UserApplications.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class SubmitUserApplicationCommandHandler(
        IUnitOfWork unitOfWork,
        IUploadFileService uploadFileService,
        IHttpContextAccessor httpContextAccessor,
        IDateTimeProvider dateTimeProvider,
        IGeminiService geminiService,
        ILogger<SubmitUserApplicationCommandHandler> logger) : IRequestHandler<SubmitUserApplicationCommand, Response<UserApplicationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
        private readonly IGeminiService _geminiService = geminiService;
        private readonly ILogger<SubmitUserApplicationCommandHandler> _logger = logger;

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
            var passportOrIdUrl = request.PassportOrID != null ? await _uploadFileService.CloudinaryStorage(request.PassportOrID) : null;
            var passportOrIdBackUrl = request.PassportOrIDBack != null ? await _uploadFileService.CloudinaryStorage(request.PassportOrIDBack) : null;

            string citizenId = string.Empty;
            DateOnly? englishTestDate = null;
            int listeningScore = 0;
            int readingScore = 0;
            int totalScore = 0;
            DateOnly? endDate = null;
            string englishDegreeNumber = string.Empty;

            // === QUÉT CCCD BẰNG GEMINI ===
            _logger.LogInformation("Bắt đầu quét CCCD cho userId: {UserId}", userId);
            CccdInfo cccdInfo = null;
            try
            {
                cccdInfo = await _geminiService.ExtractCccdInfoAsync(request.PassportOrID);
                _logger.LogInformation("Kết quả quét CCCD: CitizenId={CitizenId}, FullName={FullName}, DOB={DOB}", 
                    cccdInfo.CitizenId, cccdInfo.FullName, cccdInfo.DateOfBirth);

                // Validation số CCCD
                if (string.IsNullOrWhiteSpace(cccdInfo.CitizenId) || cccdInfo.CitizenId.Length != 12)
                {
                    _logger.LogWarning("Số CCCD không hợp lệ: {CitizenId}", cccdInfo.CitizenId);
                    return CreateErrorResponse("Không thể nhận diện số CCCD từ ảnh hoặc không đủ 12 số. Vui lòng chụp rõ nét và đảm bảo số CCCD không bị che.", "CCCD_INVALID");
                }

                if (string.IsNullOrWhiteSpace(cccdInfo.FullName))
                {
                    _logger.LogWarning("Không đọc được họ tên từ CCCD");
                    return CreateErrorResponse("Không thể nhận diện họ tên từ CCCD. Vui lòng chụp ảnh rõ ràng hơn.", "CCCD_NAME_UNREADABLE");
                }

                if (cccdInfo.DateOfBirth == null)
                {
                    _logger.LogWarning("Không đọc được ngày sinh từ CCCD");
                    return CreateErrorResponse("Không thể nhận diện ngày sinh từ CCCD. Vui lòng chụp ảnh rõ ràng hơn.", "CCCD_DOB_UNREADABLE");
                }

                citizenId = cccdInfo.CitizenId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi quét CCCD cho userId: {UserId}", userId);
                return CreateErrorResponse($"Lỗi khi quét CCCD: {ex.Message}", "CCCD_SCAN_FAILED");
            }

            // === QUÉT CHỨNG CHỈ TOEIC BẰNG GEMINI ===
            _logger.LogInformation("Bắt đầu quét TOEIC cho userId: {UserId}", userId);
            ToeicInfo toeicInfo = null;
            try
            {
                toeicInfo = await _geminiService.ExtractToeicInfoAsync(request.EnglishCertificate);
                _logger.LogInformation("Kết quả quét TOEIC: Type={Type}, CitizenId={CitizenId}, FullName={FullName}, DOB={DOB}, TestDate={TestDate}, ValidUntil={ValidUntil}, Listening={Listening}, Reading={Reading}, Total={Total}",
                    toeicInfo.CertificateType, toeicInfo.CitizenId, toeicInfo.FullName, toeicInfo.DateOfBirth,
                    toeicInfo.TestDate, toeicInfo.ValidUntil, toeicInfo.ListeningScore, toeicInfo.ReadingScore, toeicInfo.TotalScore);

                // Validation loại chứng chỉ
                if (string.IsNullOrWhiteSpace(toeicInfo.CertificateType) || !toeicInfo.CertificateType.ToUpper().Contains("TOEIC"))
                {
                    _logger.LogWarning("File không phải chứng chỉ TOEIC: {Type}", toeicInfo.CertificateType);
                    return CreateErrorResponse("File upload không phải là chứng chỉ TOEIC hợp lệ. Vui lòng upload đúng chứng chỉ TOEIC.", "NOT_TOEIC_CERTIFICATE");
                }

                      if (string.IsNullOrWhiteSpace(toeicInfo.CitizenId) || toeicInfo.CitizenId.Length != 12)
                {
                    _logger.LogWarning("Số CCCD không hợp lệ: {CitizenId}", toeicInfo.CitizenId);
                    return CreateErrorResponse("Không thể nhận diện số CCCD từ ảnh hoặc không đủ 12 số. Vui lòng chụp rõ nét và đảm bảo số CCCD không bị che.", "CCCD_INVALID");
                }

                if (string.IsNullOrWhiteSpace(toeicInfo.FullName))
                {
                    _logger.LogWarning("Không đọc được họ tên từ TOEIC");
                    return CreateErrorResponse("Không thể nhận diện họ tên từ TOEIC. Vui lòng chụp ảnh rõ ràng hơn.", "TOEIC_NAME_UNREADABLE");
                }

                if (toeicInfo.DateOfBirth == null)
                {
                    _logger.LogWarning("Không đọc được ngày sinh từ TOEIC");
                    return CreateErrorResponse("Không thể nhận diện ngày sinh từ TOEIC. Vui lòng chụp ảnh rõ ràng hơn.", "TOEIC_DOB_UNREADABLE");
                }

                // Validation các trường bắt buộc
                if (toeicInfo.TestDate == null || toeicInfo.ValidUntil == null)
                {
                    _logger.LogWarning("Không đọc được ngày thi hoặc ngày hết hạn từ TOEIC");
                    return CreateErrorResponse("Không đọc được ngày thi hoặc ngày hết hạn từ chứng chỉ TOEIC. Vui lòng chụp ảnh rõ nét.", "TOEIC_DATES_UNREADABLE");
                }

                if (toeicInfo.ListeningScore == 0 || toeicInfo.ReadingScore == 0)
                {
                    _logger.LogWarning("Không đọc được điểm Listening hoặc Reading");
                    return CreateErrorResponse("Không đọc được điểm Listening hoặc Reading từ chứng chỉ TOEIC. Vui lòng chụp ảnh rõ nét.", "TOEIC_SCORES_UNREADABLE");
                }

                // --- VALIDATION ĐIỂM SỐ TOEIC ---
                // Listening/Reading: 5-495, bội số của 5
                if (toeicInfo.ListeningScore < 5 || toeicInfo.ListeningScore > 495 || toeicInfo.ListeningScore % 5 != 0)
                {
                    _logger.LogWarning("Điểm Listening không hợp lệ: {ListeningScore}", toeicInfo.ListeningScore);
                    return CreateErrorResponse("Điểm Listening phải nằm trong khoảng 5-495 và là bội số của 5.", "TOEIC_LISTENING_INVALID");
                }
                if (toeicInfo.ReadingScore < 5 || toeicInfo.ReadingScore > 495 || toeicInfo.ReadingScore % 5 != 0)
                {
                    _logger.LogWarning("Điểm Reading không hợp lệ: {ReadingScore}", toeicInfo.ReadingScore);
                    return CreateErrorResponse("Điểm Reading phải nằm trong khoảng 5-495 và là bội số của 5.", "TOEIC_READING_INVALID");
                }
                // Total: 10-990
                if (toeicInfo.TotalScore < 10 || toeicInfo.TotalScore > 990)
                {
                    _logger.LogWarning("Tổng điểm TOEIC không hợp lệ: {TotalScore}", toeicInfo.TotalScore);
                    return CreateErrorResponse("Tổng điểm TOEIC phải nằm trong khoảng 10-990.", "TOEIC_TOTAL_INVALID");
                }

                // BỔ SUNG: Validation số CCCD trên TOEIC
                if (string.IsNullOrWhiteSpace(toeicInfo.CitizenId) || toeicInfo.CitizenId.Length != 12)
                {
                    _logger.LogWarning("Số CCCD không hợp lệ trên chứng chỉ TOEIC: {CitizenId}", toeicInfo.CitizenId);
                    return CreateErrorResponse("Không thể nhận diện số CCCD từ chứng chỉ TOEIC. Vui lòng chụp rõ nét và đảm bảo số CCCD không bị che.", "TOEIC_CCCD_INVALID");
                }

                // ✅ VALIDATION NGHIÊM NGẶT: Total score PHẢI BẰNG listening + reading
                int calculatedTotal = toeicInfo.ListeningScore + toeicInfo.ReadingScore;
                if (toeicInfo.TotalScore != calculatedTotal)
                {
                    _logger.LogWarning("Tổng điểm không khớp. Trên chứng chỉ: {TotalOnCert}, Tính toán: {Calculated} (Listening: {Listening} + Reading: {Reading})",
                        toeicInfo.TotalScore, calculatedTotal, toeicInfo.ListeningScore, toeicInfo.ReadingScore);
                    return CreateErrorResponse(
                        $"Tổng điểm trên chứng chỉ không khớp! Chứng chỉ hiển thị: {toeicInfo.TotalScore}, nhưng Listening ({toeicInfo.ListeningScore}) + Reading ({toeicInfo.ReadingScore}) = {calculatedTotal}. Vui lòng kiểm tra lại chứng chỉ hoặc chụp ảnh rõ hơn.",
                        "TOEIC_TOTAL_SCORE_MISMATCH");
                }

                // Parse TestDate
                if (!string.IsNullOrWhiteSpace(toeicInfo.TestDate))
                {
                    DateOnly parsedTestDate;
                    if (DateOnly.TryParseExact(toeicInfo.TestDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedTestDate)
                        || DateOnly.TryParseExact(toeicInfo.TestDate, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedTestDate))
                    {
                        englishTestDate = parsedTestDate;
                    }
                    else
                    {
                        _logger.LogWarning("Không parse được TestDate TOEIC với cả hai format: {TestDate}", toeicInfo.TestDate);
                    }
                }
                else
                {
                    _logger.LogWarning("TestDate TOEIC bị null hoặc rỗng");
                }

                // Parse ValidUntil
                if (!string.IsNullOrWhiteSpace(toeicInfo.ValidUntil))
                {
                    DateOnly parsedValidUntil;
                    if (DateOnly.TryParseExact(toeicInfo.ValidUntil, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedValidUntil)
                        || DateOnly.TryParseExact(toeicInfo.ValidUntil, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedValidUntil))
                    {
                        endDate = parsedValidUntil;
                    }
                    else
                    {
                        _logger.LogWarning("Không parse được ValidUntil TOEIC với cả hai format: {ValidUntil}", toeicInfo.ValidUntil);
                    }
                }
                else
                {
                    _logger.LogWarning("ValidUntil TOEIC bị null hoặc rỗng");
                }
                listeningScore = toeicInfo.ListeningScore;
                readingScore = toeicInfo.ReadingScore;
                totalScore = toeicInfo.TotalScore;
                // englishDegreeNumber = "TOEIC";
                englishDegreeNumber = toeicInfo.CertificateType;

                // --- VALIDATION NGÀY HẾT HẠN & NGÀY TEST TOEIC ---
                if (englishTestDate.HasValue && endDate.HasValue)
                {
                    // Ngày hết hạn phải đúng bằng ngày test + 2 năm
                    var expectedEndDate = englishTestDate.Value.AddYears(2);
                    if (endDate.Value != expectedEndDate)
                    {
                        _logger.LogWarning("Ngày hết hạn TOEIC không đúng 2 năm sau ngày test. TestDate: {TestDate}, ValidUntil: {ValidUntil}", englishTestDate, endDate);
                        return CreateErrorResponse($"Ngày hết hạn TOEIC phải đúng bằng ngày thi cộng 2 năm. Ngày thi: {englishTestDate:dd/MM/yyyy}, Ngày hết hạn: {endDate:dd/MM/yyyy}.", "TOEIC_DATE_INVALID");
                    }
                    // Ngày test không được lớn hơn ngày hiện tại
                    if (englishTestDate.Value > _dateTimeProvider.Today)
                    {
                        _logger.LogWarning("Ngày test TOEIC lớn hơn ngày hiện tại. TestDate: {TestDate}, Today: {Today}", englishTestDate, _dateTimeProvider.Today);
                        return CreateErrorResponse($"Ngày thi TOEIC không được lớn hơn ngày hiện tại. Ngày thi: {englishTestDate:dd/MM/yyyy}.", "TOEIC_TESTDATE_FUTURE");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi quét TOEIC cho userId: {UserId}", userId);
                return CreateErrorResponse($"Lỗi khi quét chứng chỉ TOEIC: {ex.Message}", "TOEIC_SCAN_FAILED");
            }

            // === SO SÁNH THÔNG TIN GIỮA CCCD, TOEIC VÀ FORM ĐĂNG KÝ ===
            _logger.LogInformation("Bắt đầu so sánh thông tin giữa các document");

            // 1. So sánh họ tên (normalize để tránh lỗi dấu/khoảng trắng)
            var normalizedFormName = NormalizeName(request.FullName);
            var normalizedCccdName = NormalizeName(cccdInfo.FullName);
            var normalizedToeicName = NormalizeName(toeicInfo.FullName);

            if (!string.IsNullOrWhiteSpace(cccdInfo.FullName) && normalizedFormName != normalizedCccdName)
            {
                _logger.LogWarning("Họ tên không khớp. Form: '{FormName}', CCCD: '{CccdName}'", request.FullName, cccdInfo.FullName);
                return CreateErrorResponse(
                    $"Họ tên trên CCCD ({cccdInfo.FullName}) không khớp với thông tin đăng ký ({request.FullName}). Vui lòng kiểm tra lại.",
                    "NAME_MISMATCH_CCCD");
            }

            if (!string.IsNullOrWhiteSpace(toeicInfo.FullName) && normalizedFormName != normalizedToeicName)
            {
                _logger.LogWarning("Họ tên không khớp. Form: '{FormName}', TOEIC: '{ToeicName}'", request.FullName, toeicInfo.FullName);
                return CreateErrorResponse(
                    $"Họ tên trên chứng chỉ TOEIC ({toeicInfo.FullName}) không khớp với thông tin đăng ký ({request.FullName}). Vui lòng kiểm tra lại.",
                    "NAME_MISMATCH_TOEIC");
            }

            // 2. So sánh ngày sinh
            // Parse string sang DateOnly để so sánh
            DateOnly? cccdDob = null;
            if (!string.IsNullOrWhiteSpace(cccdInfo.DateOfBirth))
            {
                if (DateOnly.TryParseExact(cccdInfo.DateOfBirth, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedCccdDob))
                {
                    cccdDob = parsedCccdDob;
                }
            }

            DateOnly? toeicDob = null;
            if (!string.IsNullOrWhiteSpace(toeicInfo.DateOfBirth))
            {
                if (DateOnly.TryParseExact(toeicInfo.DateOfBirth, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedToeicDob))
                {
                    toeicDob = parsedToeicDob;
                }
            }

            DateOnly? Dob = null;
                if (!string.IsNullOrWhiteSpace(request.DateOfBirth))
                {
                    if (DateOnly.TryParseExact(request.DateOfBirth, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDob))
                    {
                        Dob = parsedDob;
                    }
                }


            if (cccdDob.HasValue && cccdDob != Dob)
            {
                _logger.LogWarning("Ngày sinh không khớp. Form: {FormDOB}, CCCD: {CccdDOB}", Dob, cccdDob);
                return CreateErrorResponse(
                    $"Ngày sinh trên CCCD ({cccdDob:dd/MM/yyyy}) không khớp với thông tin đăng ký ({Dob:dd/MM/yyyy}). Vui lòng kiểm tra lại.",
                    "DOB_MISMATCH_CCCD");
            }

            if (toeicDob.HasValue && toeicDob != Dob)
            {
                _logger.LogWarning("Ngày sinh không khớp. Form: {FormDOB}, TOEIC: {ToeicDOB}", Dob, toeicDob);
                return CreateErrorResponse(
                    $"Ngày sinh trên chứng chỉ TOEIC ({toeicDob:dd/MM/yyyy}) không khớp với thông tin đăng ký ({Dob:dd/MM/yyyy}). Vui lòng kiểm tra lại.",
                    "DOB_MISMATCH_TOEIC");
            }

            // 3. So sánh số CCCD giữa 2 file
            if (!string.IsNullOrWhiteSpace(toeicInfo.CitizenId) && cccdInfo.CitizenId != toeicInfo.CitizenId)
            {
                _logger.LogWarning("Số CCCD không khớp giữa 2 file. CCCD: {CccdId}, TOEIC: {ToeicId}", cccdInfo.CitizenId, toeicInfo.CitizenId);
                return CreateErrorResponse(
                    $"Số CCCD trên 2 file không khớp nhau. CCCD: {cccdInfo.CitizenId}, TOEIC: {toeicInfo.CitizenId}. Vui lòng kiểm tra lại.",
                    "CITIZENID_MISMATCH");
            }

            _logger.LogInformation("Validation thành công! Tiếp tục lưu hồ sơ cho userId: {UserId}", userId);

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
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = Dob,
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

        private Response<UserApplicationResponse> CreateErrorResponse(string message, string errorCode)
        {
            return new Response<UserApplicationResponse>
            {
                Code = (int)ResponseCode.FAIL,
                Message = message,
                Data = null,
                Errors = errorCode
            };
        }

        private string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";

            // 1. Trim và uppercase
            var normalized = name.Trim().ToUpperInvariant();
            
            // 2. Loại bỏ tất cả dấu tiếng Việt
            normalized = normalized
                // A
                .Replace("Á", "A").Replace("À", "A").Replace("Ả", "A").Replace("Ã", "A").Replace("Ạ", "A")
                .Replace("Ă", "A").Replace("Ắ", "A").Replace("Ằ", "A").Replace("Ẳ", "A").Replace("Ẵ", "A").Replace("Ặ", "A")
                .Replace("Â", "A").Replace("Ấ", "A").Replace("Ầ", "A").Replace("Ẩ", "A").Replace("Ẫ", "A").Replace("Ậ", "A")
                // E
                .Replace("É", "E").Replace("È", "E").Replace("Ẻ", "E").Replace("Ẽ", "E").Replace("Ẹ", "E")
                .Replace("Ê", "E").Replace("Ế", "E").Replace("Ề", "E").Replace("Ể", "E").Replace("Ễ", "E").Replace("Ệ", "E")
                // I
                .Replace("Í", "I").Replace("Ì", "I").Replace("Ỉ", "I").Replace("Ĩ", "I").Replace("Ị", "I")
                // O
                .Replace("Ó", "O").Replace("Ò", "O").Replace("Ỏ", "O").Replace("Õ", "O").Replace("Ọ", "O")
                .Replace("Ô", "O").Replace("Ố", "O").Replace("Ồ", "O").Replace("Ổ", "O").Replace("Ỗ", "O").Replace("Ộ", "O")
                .Replace("Ơ", "O").Replace("Ớ", "O").Replace("Ờ", "O").Replace("Ở", "O").Replace("Ỡ", "O").Replace("Ợ", "O")
                // U
                .Replace("Ú", "U").Replace("Ù", "U").Replace("Ủ", "U").Replace("Ũ", "U").Replace("Ụ", "U")
                .Replace("Ư", "U").Replace("Ứ", "U").Replace("Ừ", "U").Replace("Ử", "U").Replace("Ữ", "U").Replace("Ự", "U")
                // Y
                .Replace("Ý", "Y").Replace("Ỳ", "Y").Replace("Ỷ", "Y").Replace("Ỹ", "Y").Replace("Ỵ", "Y")
                // D
                .Replace("Đ", "D");
            
            // 3. Loại bỏ tất cả khoảng trắng thừa (chỉ để 1 space giữa các từ)
            while (normalized.Contains("  "))
            {
                normalized = normalized.Replace("  ", " ");
            }
            
            return normalized.Trim();
        }
    }
}
