using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.EnquiryRequests.Commands.CreateEnquiryRequest
{
    public class CreateEnquiryRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<CreateEnquiryRequestCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<int>> Handle(CreateEnquiryRequestCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Get current user from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            var userId = int.Parse(currentUserId);

            // 1. Validate TestSession exists and belongs to current user
            var testSessions = await _unitOfWork.TestSessions.GetAllAsync(
                expression: ts => ts.TestSessionId == request.TestSessionId,
                includeFunc: query => query
                    .Include(ts => ts.User)
                    .Include(ts => ts.Test)
                        .ThenInclude(t => t.Rounds)
                            .ThenInclude(r => r.CampaignRound)
                                .ThenInclude(cr => cr.Campaign)
                                    .ThenInclude(c => c.Assignments)
                    .Include(ts => ts.EnquiryRequest)
            );

            var testSession = testSessions.FirstOrDefault();
            if (testSession == null)
            {
                errorMessages.Add("Test session not found.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            if (testSession.UserId != userId)
            {
                errorMessages.Add("You can only create enquiry request for your own test session.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // 2. Check if enquiry request already exists for this test session
            if (testSession.EnquiryRequest != null)
            {
                errorMessages.Add("An enquiry request already exists for this test session.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // 3. Create EnquiryRequest
            var enquiryRequest = new EnquiryRequest
            {
                TestSessionId = request.TestSessionId,
                UserId = userId,
                Reason = request.Reason,
                Status = request.Status,
                NewScore = 0, // Will be updated by examiner later
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EnquiryRequests.AddAsync(enquiryRequest);

            // 4. Update TestSession Status to false (needs review)
            testSession.Status = false;
            await _unitOfWork.TestSessions.UpdateAsync(testSession);

            // 5. Find examiner(s) from CampaignAssignment with Task = TestCreation
            if (testSession.Test?.Rounds != null && testSession.Test.Rounds.Any())
            {
                // Get all campaigns from rounds
                var campaigns = testSession.Test.Rounds
                    .Where(r => r.CampaignRound?.Campaign != null)
                    .Select(r => r.CampaignRound!.Campaign)
                    .Distinct()
                    .ToList();

                // Get all examiners with TestCreation task from campaign assignments
                var examinerIds = new List<int>();
                foreach (var campaign in campaigns)
                {
                    if (campaign.Assignments != null && campaign.Assignments.Any())
                    {
                        var campaignExaminers = campaign.Assignments
                            .Where(a => a.Task == TaskType.TestCreation && a.UserId.HasValue)
                            .Select(a => a.UserId!.Value)
                            .ToList();
                        
                        examinerIds.AddRange(campaignExaminers);
                    }
                }

                // Remove duplicates
                examinerIds = examinerIds.Distinct().ToList();

                // Create notifications for all examiners
                foreach (var examinerId in examinerIds)
                {
                    var notification = new Notification
                    {
                        Title = "Yêu cầu phúc khảo",
                        Body = request.Reason,
                        Time = DateTime.UtcNow,
                        IsRead = false,
                        UserId = examinerId,
                        NotificationTypeId = null // You can set this based on your system
                    };

                    await _unitOfWork.Notifications.AddAsync(notification);
                }
            }

            // 6. Save all changes
            await _unitOfWork.CompleteAsync();

            return new Response<int>(
                code: (int)ResponseCode.SUCCESS,
                data: enquiryRequest.EnquiryId,
                message: "Enquiry request created successfully. Examiners have been notified.");
        }
    }
}
