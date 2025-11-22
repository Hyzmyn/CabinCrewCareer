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

namespace CabinCrewCareer.Application.TestSessions.Commands.SubmitTestSession
{
    public class SubmitTestSessionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<SubmitTestSessionCommand, Response<SubmitTestSessionResponse>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<Response<SubmitTestSessionResponse>> Handle(SubmitTestSessionCommand request, CancellationToken cancellationToken)
        {
            List<string> errorMessages = [];

            // Validate request
            var validator = new SubmitTestSessionCommandValidator();
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

            // Get user and check role (must be CabinCrew or Candidate)
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ApiException(ResponseCode.USER_NOT_FOUND);
            }

            // Only CabinCrew and Candidate can submit test
            if (user.RoleId != (int)AvailableRole.CabinCrew && user.RoleId != (int)AvailableRole.Candidate)
            {
                errorMessages.Add("Only Cabin Crew and Candidate can submit test answers");
                throw new ValidationException(ResponseCode.CREATED_UNSUCC, errorMessages);
            }

            // Get test with questions and options
            var tests = await _unitOfWork.Tests.GetAllAsync(
                expression: t => t.TestId == request.TestId,
                includeFunc: query => query
                    .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.TestOptions)
            );

            var testEntity = tests.FirstOrDefault();
            if (testEntity == null)
            {
                throw new ApiException(ResponseCode.TEST_NOT_FOUND);
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
                TotalScore = 0, // Will be calculated
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.TestSessions.AddAsync(testSession);
            await _unitOfWork.CompleteAsync();

            // Process answers based on test type
            int totalScore = 0;
            int correctAnswersCount = 0;
            List<SubmittedAnswerDto> submittedAnswers = [];

            foreach (var answerDto in request.Answers)
            {
                var question = testEntity.TestQuestions.FirstOrDefault(q => q.QuestionId == answerDto.QuestionId);
                if (question == null) continue;

                var testAnswer = new TestAnswer
                {
                    TestSessionId = testSession.TestSessionId,
                    SelectedOptionId = answerDto.SelectedOptionId,
                    EssayAnswer = answerDto.EssayAnswer ?? string.Empty,
                    AnswerAudioFileURL = answerDto.AnswerAudioFileURL ?? string.Empty,
                    Score = 0,
                    IsCorrect = false
                };

                // Logic based on TestType
                if (testEntity.TestType == TestType.EnglishListening || testEntity.TestType == TestType.Practical)
                {
                    // For Listening and Practical: Auto-grade by comparing with correct option
                    if (answerDto.SelectedOptionId.HasValue)
                    {
                        var selectedOption = question.TestOptions.FirstOrDefault(o => o.OptionId == answerDto.SelectedOptionId.Value);
                        if (selectedOption != null && selectedOption.IsCorrect)
                        {
                            testAnswer.IsCorrect = true;
                            testAnswer.Score = question.Score;
                            totalScore += question.Score;
                            correctAnswersCount++;
                        }
                    }
                }
                else if (testEntity.TestType == TestType.EnglishSpeaking)
                {
                    // For Speaking: Don't auto-grade, leave score as 0 for manual grading later
                    // Just store the essay answer or audio URL
                    testAnswer.Score = 0;
                    testAnswer.IsCorrect = false; // Will be graded manually
                }

                await _unitOfWork.TestAnswers.AddAsync(testAnswer);

                // Build submitted answer DTO
                var submittedAnswer = new SubmittedAnswerDto
                {
                    AnswerId = testAnswer.AnswerId,
                    QuestionId = question.QuestionId,
                    QuestionContent = question.QuestionContent,
                    QuestionScore = question.Score,
                    SelectedOptionId = answerDto.SelectedOptionId,
                    IsCorrect = testAnswer.IsCorrect,
                    Score = testAnswer.Score,
                    EssayAnswer = answerDto.EssayAnswer,
                    AnswerAudioFileURL = answerDto.AnswerAudioFileURL
                };

                if (answerDto.SelectedOptionId.HasValue)
                {
                    var selectedOption = question.TestOptions.FirstOrDefault(o => o.OptionId == answerDto.SelectedOptionId.Value);
                    submittedAnswer.SelectedOptionContent = selectedOption?.OptionContent;
                }

                submittedAnswers.Add(submittedAnswer);
            }

            // Update test session total score
            testSession.TotalScore = totalScore;
            await _unitOfWork.TestSessions.UpdateAsync(testSession);
            await _unitOfWork.CompleteAsync();

            // Prepare response
            string message = testEntity.TestType == TestType.EnglishSpeaking
                ? "Test submitted successfully. Your answers will be graded manually by an examiner."
                : $"Test submitted successfully. You scored {totalScore} out of {testEntity.MaxScore} points.";

            var response = new SubmitTestSessionResponse
            {
                TestSessionId = testSession.TestSessionId,
                TestId = testEntity.TestId,
                TestName = testEntity.TestName,
                TestType = testEntity.TestType.ToString(),
                StartTime = testSession.StartTime,
                EndTime = testSession.EndTime,
                TotalScore = totalScore,
                MaxScore = testEntity.MaxScore,
                Status = testSession.Status,
                TotalQuestions = request.Answers.Count,
                CorrectAnswers = correctAnswersCount,
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
