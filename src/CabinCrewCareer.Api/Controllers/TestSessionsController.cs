using Asp.Versioning;
using CabinCrewCareer.Api.Models;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.TestSessions.Commands.SubmitMultipleChoiceTest;
using CabinCrewCareer.Application.TestSessions.Commands.SubmitSpeakingTest;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/test-sessions")]
    [ApiVersion(1)]
    [Authorize]
    public class TestSessionsController(
        IMediator mediator,
        ILogger<TestSessionsController> logger) : ApiController(mediator, logger)
    {
        #region Submit Multiple Choice Test (Listening/Practical)
        /// <summary>
        /// Submit multiple choice test - For English Listening and Practical tests only
        /// </summary>
        /// <param name="request">Test submission request with selected options</param>
        /// <remarks>
        /// **Submit answers for English Listening and Practical tests (Multiple Choice)**
        /// 
        /// **Security:**
        /// - Only accessible by CabinCrew and Candidate roles
        /// - Uses JWT token authentication to identify user
        /// - Validates test type is Listening or Practical
        /// 
        /// **Auto-grading:**
        /// - System automatically compares selected options with correct answers
        /// - Calculates total score by summing points for correct answers
        /// - Returns immediate results with score and correct/incorrect flags
        /// - Each correct answer earns the question's assigned points
        /// 
        /// **Request Format:**
        /// ```json
        /// {
        ///   "testId": 1,
        ///   "startTime": "2025-11-22T10:00:00Z",
        ///   "endTime": "2025-11-22T10:30:00Z",
        ///   "answers": [
        ///     {
        ///       "questionId": 1,
        ///       "selectedOptionId": 3
        ///     }
        ///   ]
        /// }
        /// ```
        /// 
        /// **Response includes:**
        /// - TestSessionId: Generated session ID
        /// - TotalScore: Auto-calculated score
        /// - MaxScore: Maximum possible score
        /// - CorrectAnswers: Count of correct answers
        /// - SubmittedAnswers: Details of each answer with correctness
        /// 
        /// Note: AudioFileURL is automatically set to null for these test types.
        /// </remarks>
        /// <returns>Test session result with immediate score</returns>
        /// <response code="200">Ok - Test submitted and graded successfully</response>
        /// <response code="400">Bad Request - Invalid test type or validation errors</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found - Test not found</response>
        [HttpPost("submit-multiple-choice")]
        [ProducesResponseType(typeof(Response<SubmitTestSessionResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SubmitMultipleChoiceTestAsync([FromBody] SubmitMultipleChoiceTestRequest request)
        {
            try
            {
                var command = new SubmitMultipleChoiceTestCommand(
                    request.TestId,
                    request.StartTime,
                    request.EndTime,
                    request.Answers);

                return Ok(await mediator.Send(command));
            }
            catch (ValidationException e)
            {
                return BadRequest(new
                {
                    errorCode = e.ErrorCode,
                    errorMessage = e.ErrorMessage,
                    errors = e.Errors
                });
            }
            catch (ApiException e)
            {
                return BadRequest(new
                {
                    errorCode = e.ErrorCode,
                    error = e.Error,
                    errorMessage = e.ErrorMessage
                });
            }
        }
        #endregion

        #region Submit Speaking Test
        /// <summary>
        /// Submit speaking test with audio files - For English Speaking tests only
        /// </summary>
        /// <param name="request">Test submission with answers and audio files</param>
        /// <remarks>
        /// **Submit answers for English Speaking tests with audio file uploads**
        /// 
        /// **Security:**
        /// - Only accessible by CabinCrew and Candidate roles
        /// - Uses JWT token authentication to identify user
        /// - Validates test type is Speaking
        /// 
        /// **Request Format (multipart/form-data):**
        /// ```
        /// testId: 75
        /// startTime: 2025-11-22T10:00:00Z
        /// endTime: 2025-11-22T10:30:00Z
        /// questionIds: 137,138,139
        /// audioFiles: [file1.mp3]
        /// audioFiles: [file2.mp3]
        /// audioFiles: [file3.mp3]
        /// ```
        /// 
        /// **Field Descriptions:**
        /// - `testId`: Test ID (integer)
        /// - `startTime`: When test started (ISO 8601 format)
        /// - `endTime`: When test ended (ISO 8601 format)
        /// - `questionIds`: Comma-separated question IDs in order (e.g., "137,138,139")
        /// - `audioFiles`: Upload multiple audio files (one per question, in same order)
        /// 
        /// **Audio File Requirements:**
        /// - Format: .mp3 or .wav only
        /// - Max size: 50 MB per file
        /// - Number of files must match number of question IDs
        /// - Files will be uploaded to Firebase Storage
        /// 
        /// **Manual Grading:**
        /// - Does NOT auto-grade (score remains 0)
        /// - Audio files uploaded to Firebase Storage
        /// - URLs stored in TestAnswer records
        /// - Requires manual grading by examiner later
        /// 
        /// **Response includes:**
        /// - TestSessionId: Generated session ID
        /// - TotalScore: 0 (pending manual grading)
        /// - SubmittedAnswers: Details with audio URLs
        /// - Message: Confirmation of submission
        /// 
        /// **Example cURL:**
        /// ```bash
        /// curl -X POST 'http://localhost:5268/api/v1/test-sessions/submit-speaking' \
        ///   -H 'Authorization: Bearer YOUR_TOKEN' \
        ///   -F 'testId=75' \
        ///   -F 'startTime=2025-11-22T10:00:00Z' \
        ///   -F 'endTime=2025-11-22T10:30:00Z' \
        ///   -F 'questionIds=137,138' \
        ///   -F 'audioFiles=@file1.mp3' \
        ///   -F 'audioFiles=@file2.mp3'
        /// ```
        /// </remarks>
        /// <returns>Test session confirmation with audio URLs</returns>
        /// <response code="200">Ok - Speaking test submitted successfully</response>
        /// <response code="400">Bad Request - Invalid test type, file format, or validation errors</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found - Test not found</response>
        [HttpPost("submit-speaking")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Response<SubmitTestSessionResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SubmitSpeakingTestAsync([FromForm] SubmitSpeakingTestRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.QuestionIds))
                {
                    return BadRequest(new Response<string>(
                        code: 400,
                        message: "questionIds is required (comma-separated list of question IDs)"));
                }

                if (request.AudioFiles == null || request.AudioFiles.Count == 0)
                {
                    return BadRequest(new Response<string>(
                        code: 400,
                        message: "At least one audio file is required"));
                }

                // Parse question IDs
                var questionIdStrings = request.QuestionIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var questionIds = new List<int>();

                foreach (var idStr in questionIdStrings)
                {
                    if (!int.TryParse(idStr.Trim(), out int questionId))
                    {
                        return BadRequest(new Response<string>(
                            code: 400,
                            message: $"Invalid question ID: '{idStr}'. Must be comma-separated integers (e.g., '137,138,139')"));
                    }
                    questionIds.Add(questionId);
                }

                // Validate matching counts
                if (questionIds.Count != request.AudioFiles.Count)
                {
                    return BadRequest(new Response<string>(
                        code: 400,
                        message: $"Number of question IDs ({questionIds.Count}) must match number of audio files ({request.AudioFiles.Count})"));
                }

                // Build answers list by pairing question IDs with audio files
                var speakingAnswers = new List<SpeakingAnswerDto>();
                for (int i = 0; i < questionIds.Count; i++)
                {
                    speakingAnswers.Add(new SpeakingAnswerDto
                    {
                        QuestionId = questionIds[i],
                        AudioFile = request.AudioFiles[i]
                    });
                }

                var command = new SubmitSpeakingTestCommand(
                    request.TestId,
                    request.StartTime,
                    request.EndTime,
                    speakingAnswers);

                return Ok(await mediator.Send(command));
            }
            catch (ValidationException e)
            {
                return BadRequest(new
                {
                    errorCode = e.ErrorCode,
                    errorMessage = e.ErrorMessage,
                    errors = e.Errors
                });
            }
            catch (ApiException e)
            {
                return BadRequest(new
                {
                    errorCode = e.ErrorCode,
                    error = e.Error,
                    errorMessage = e.ErrorMessage
                });
            }
        }
        #endregion
    }
}
