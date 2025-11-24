using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CabinCrewCareer.Application.TestSessions.Commands.SubmitSpeakingTest
{
    public class SubmitSpeakingTestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFirebaseStorageService firebaseStorageService) : IRequestHandler<SubmitSpeakingTestCommand, Response<SubmitTestSessionResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IFirebaseStorageService _firebaseStorageService = firebaseStorageService;

        public async Task<Response<SubmitTestSessionResponse>> Handle(SubmitSpeakingTestCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new SubmitSpeakingTestCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errorMessages.Add(error.ErrorMessage);
                }
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Get current user from token
            var currentUserId = _currentUserService.CurrentPrincipal;
            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new ApiException(ResponseCode.FAILED_AUTHENTICATION);
            }

            var userId = int.Parse(currentUserId);

            // Get user and check role
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ApiException(ResponseCode.USER_NOT_FOUND);
            }

            if (user.RoleId != (int)AvailableRole.CabinCrew && user.RoleId != (int)AvailableRole.Candidate)
            {
                errorMessages.Add("Only Cabin Crew and Candidate can submit test answers");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Get test with questions
            var tests = await _unitOfWork.Tests.GetAllAsync(
                expression: t => t.TestId == request.TestId,
                includeFunc: query => query.Include(t => t.TestQuestions)
            );

            var testEntity = tests.FirstOrDefault();
            if (testEntity == null)
            {
                throw new ApiException(ResponseCode.TEST_NOT_FOUND);
            }

            // Validate test type - must be Speaking
            if (testEntity.TestType != TestType.EnglishSpeaking)
            {
                errorMessages.Add("This endpoint is only for English Speaking tests. Use the Multiple Choice endpoint for Listening/Practical tests.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Check if user already has a test session for this test
            var existingTestSessions = await _unitOfWork.TestSessions.GetAllAsync(
                expression: ts => ts.UserId == userId && ts.TestId == request.TestId
            );

            if (existingTestSessions.Any())
            {
                errorMessages.Add($"You have already submitted this test. Multiple submissions are not allowed.");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Check if all question IDs exist in the test
            var testQuestionIds = testEntity.TestQuestions.Select(q => q.QuestionId).ToHashSet();
            var answerQuestionIds = request.Answers.Select(a => a.QuestionId).ToHashSet();
            
            var invalidQuestionIds = answerQuestionIds.Except(testQuestionIds).ToList();
            if (invalidQuestionIds.Count != 0)
            {
                errorMessages.Add($"Invalid question IDs: {string.Join(", ", invalidQuestionIds)}");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Create test session
            var testSession = new TestSession
            {
                UserId = userId,
                TestId = request.TestId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = true,
                TotalScore = 0, // Speaking tests require manual grading
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.TestSessions.AddAsync(testSession);
            await _unitOfWork.CompleteAsync();

            // Process answers - upload audio files
            List<SubmittedAnswerDto> submittedAnswers = [];

            foreach (var answerDto in request.Answers)
            {
                var question = testEntity.TestQuestions.FirstOrDefault(q => q.QuestionId == answerDto.QuestionId);
                if (question == null) continue;

                // Upload audio file to Firebase
                string audioUrl;
                try
                {
                    using var stream = answerDto.AudioFile.OpenReadStream();
                    var fileName = $"speaking_test_{testSession.TestSessionId}_q{answerDto.QuestionId}_{Guid.NewGuid()}.mp3";
                    audioUrl = await _firebaseStorageService.UploadAudioFileAsync(stream, fileName, answerDto.AudioFile.ContentType);
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Failed to upload audio file for question {answerDto.QuestionId}: {ex.Message}");
                    throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
                }

                var testAnswer = new TestAnswer
                {
                    TestSessionId = testSession.TestSessionId,
                    SelectedOptionId = null,
                    EssayAnswer = string.Empty,
                    AnswerAudioFileURL = audioUrl,
                    Score = 0, // Requires manual grading
                    IsCorrect = false // Will be updated after manual grading
                };

                await _unitOfWork.TestAnswers.AddAsync(testAnswer);

                // Build submitted answer DTO
                var submittedAnswer = new SubmittedAnswerDto
                {
                    AnswerId = testAnswer.AnswerId,
                    QuestionId = question.QuestionId,
                    QuestionContent = question.QuestionContent,
                    QuestionScore = question.Score,
                    SelectedOptionId = null,
                    SelectedOptionContent = null,
                    IsCorrect = false,
                    Score = 0,
                    EssayAnswer = null,
                    AnswerAudioFileURL = audioUrl
                };

                submittedAnswers.Add(submittedAnswer);
            }

            await _unitOfWork.CompleteAsync();

            // Prepare response
            string message = "Speaking test submitted successfully. Audio files uploaded. Awaiting manual grading by examiner.";

            var response = new SubmitTestSessionResponse
            {
                TestSessionId = testSession.TestSessionId,
                TestId = testEntity.TestId,
                TestName = testEntity.TestName,
                TestType = testEntity.TestType.ToString(),
                StartTime = testSession.StartTime,
                EndTime = testSession.EndTime,
                TotalScore = 0,
                MaxScore = testEntity.MaxScore,
                Status = testSession.Status,
                TotalQuestions = request.Answers.Count,
                CorrectAnswers = 0, // Will be updated after manual grading
                SubmittedAnswers = submittedAnswers,
                Message = message
            };

            return new Response<SubmitTestSessionResponse>(
                data: response,
                code: (int)ResponseCode.CREATED_SUCCESS,
                message: message);
        }
    }
}
