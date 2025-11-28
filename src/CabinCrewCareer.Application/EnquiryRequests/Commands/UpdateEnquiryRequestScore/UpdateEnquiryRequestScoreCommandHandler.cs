using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.EnquiryRequests.Commands.UpdateEnquiryRequestScore
{
    public class UpdateEnquiryRequestScoreCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<UpdateEnquiryRequestScoreCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<string>> Handle(UpdateEnquiryRequestScoreCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Get current user from token (examiner)
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            // 1. Validate EnquiryRequest exists and get related data
            var enquiryRequests = await _unitOfWork.EnquiryRequests.GetAllAsync(
                expression: er => er.EnquiryId == request.EnquiryId,
                includeFunc: query => query
                    .Include(er => er.TestSession)
                        .ThenInclude(ts => ts.TestAnswers)
                    .Include(er => er.User)
            );

            var enquiryRequest = enquiryRequests.FirstOrDefault();
            if (enquiryRequest == null)
            {
                errorMessages.Add("Enquiry request not found.");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            if (enquiryRequest.TestSession == null)
            {
                errorMessages.Add("Test session not found for this enquiry request.");
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // 2. Validate and update TestAnswer scores
            int totalNewScore = 0;
            var oldTotalScore = enquiryRequest.TestSession.TotalScore;

            foreach (var answerScore in request.AnswerScores)
            {
                var answerId = answerScore.Key;
                var scoreInfo = answerScore.Value;

                // Find the answer
                var testAnswer = enquiryRequest.TestSession.TestAnswers.FirstOrDefault(ta => ta.AnswerId == answerId);
                if (testAnswer == null)
                {
                    errorMessages.Add($"Answer with ID {answerId} not found in this test session.");
                    continue;
                }

                // Calculate total score for this answer from criteria scores
                int answerScore_total = scoreInfo.CriteriaScores.Values.Sum();

                // Update the test answer
                testAnswer.Score = answerScore_total;
                testAnswer.IsCorrect = scoreInfo.IsCorrect;
                await _unitOfWork.TestAnswers.UpdateAsync(testAnswer);

                totalNewScore += answerScore_total;
            }

            if (errorMessages.Any())
            {
                throw new ValidationException(ResponseCode.UPDATED_UNSUCC, errorMessages);
            }

            // 3. Update EnquiryRequest
            enquiryRequest.NewScore = totalNewScore;
            enquiryRequest.Status = true; // Mark as reviewed/completed
            
            if (!string.IsNullOrEmpty(request.NewReason))
            {
                enquiryRequest.Reason = request.NewReason;
            }

            await _unitOfWork.EnquiryRequests.UpdateAsync(enquiryRequest);

            // 4. Update TestSession
            enquiryRequest.TestSession.TotalScore = totalNewScore;
            enquiryRequest.TestSession.Status = true; // Mark as completed
            await _unitOfWork.TestSessions.UpdateAsync(enquiryRequest.TestSession);

            // 5. Create notification for candidate (TestSession.UserId)
            if (enquiryRequest.TestSession.UserId.HasValue)
            {
                string notificationTitle = "Thông báo cập nhật điểm phúc khảo";
                string notificationBody = BuildNotificationBody(oldTotalScore, totalNewScore, request.NewReason);

                var notification = new Notification
                {
                    Title = notificationTitle,
                    Body = notificationBody,
                    Time = DateTime.UtcNow,
                    IsRead = false,
                    UserId = enquiryRequest.TestSession.UserId.Value,
                    NotificationTypeId = null
                };

                await _unitOfWork.Notifications.AddAsync(notification);
            }

            // 6. Save all changes
            await _unitOfWork.CompleteAsync();

            return new Response<string>(
                code: (int)ResponseCode.SUCCESS,
                data: "Score updated successfully",
                message: $"Enquiry request scored successfully. New total score: {totalNewScore}");
        }

        private string BuildNotificationBody(int oldScore, int newScore, string? newReason)
        {
            string comparison;
            int scoreDifference = newScore - oldScore;

            if (scoreDifference > 0)
            {
                comparison = $"Điểm của bạn đã được tăng từ {oldScore} lên {newScore} (tăng {scoreDifference} điểm).";
            }
            else if (scoreDifference < 0)
            {
                comparison = $"Điểm của bạn đã được điều chỉnh từ {oldScore} xuống {newScore} (giảm {Math.Abs(scoreDifference)} điểm).";
            }
            else
            {
                comparison = $"Điểm của bạn vẫn giữ nguyên là {newScore} điểm sau khi phúc khảo.";
            }

            string body = comparison;

            if (!string.IsNullOrEmpty(newReason))
            {
                body += $"\n\nNhận xét từ giám khảo: {newReason}";
            }

            return body;
        }
    }
}
