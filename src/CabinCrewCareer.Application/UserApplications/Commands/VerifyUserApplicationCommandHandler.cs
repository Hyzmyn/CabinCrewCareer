using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.UserApplications.Common;
using CabinCrewCareer.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using CabinCrewCareer.Domain.Entities;

namespace CabinCrewCareer.Application.UserApplications.Commands
{
    public class VerifyUserApplicationCommandHandler(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<VerifyUserApplicationCommand, Response<UserApplicationResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<Response<UserApplicationResponse>> Handle(VerifyUserApplicationCommand command, CancellationToken cancellationToken)
        {
            var application = await _unitOfWork.UserApplications.GetUserApplicationByIdAsync(command.ApplicationId);
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
            // Chỉ cho phép user sở hữu hồ sơ xác thực
            if (application.UserId != userId)
                return new Response<UserApplicationResponse>
                {
                    Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.FAILED_AUTHENTICATION,
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
                DocumentType.PassportOrID,
                DocumentType.PassportOrIDBack,
            };
            var hasAllDocuments = requiredTypes.All(type => application.Documents.Any(d => d.Type == type && !string.IsNullOrEmpty(d.DocumentURL)));
            if (!hasAllDocuments)
                return new Response<UserApplicationResponse>
                {
                    Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.FAIL,
                    Message = "Application does not have all required documents.",
                    Data = null,
                    Errors = "Missing documents"
                };

            var updated = await _unitOfWork.UserApplications.UpdateApplicationStatusAsync(command.ApplicationId, ApplicationStatus.Submitted);
            if (!updated)
                return new Response<UserApplicationResponse>
                {
                    Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.UPDATED_UNSUCC,
                    Message = "Failed to update application status.",
                    Data = null,
                    Errors = "Update failed"
                };

            // --- Tạo RoundActivity trạng thái Ongoing cho user nếu có CampaignRoundId ---
            int? campaignRoundId = command.CampaignRoundId;
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
                        Message = "Screening round not found for this campaignRoundId.",
                        Data = null,
                        Errors = "ROUND_SCREENING_NOT_FOUND"
                    };
                }
            }

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
                Code = (int)CabinCrewCareer.Application.Common.Enums.ResponseCode.CREATED_SUCCESS,
                Message = "Application submitted successfully.",
                Data = response,
                Errors = null
            };
        }
    }
}
