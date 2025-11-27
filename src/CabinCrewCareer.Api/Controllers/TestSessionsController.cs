using Asp.Versioning;
using CabinCrewCareer.Api.Models;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.TestSessions.Commands.SubmitMultipleChoiceTest;
using CabinCrewCareer.Application.TestSessions.Commands.SubmitSpeakingTest;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Application.TestSessions.Queries.GetAllTestSessions;
using CabinCrewCareer.Application.TestSessions.Queries.GetMyListeningTestSessions;
using CabinCrewCareer.Application.TestSessions.Queries.GetMySpeakingTestSessions;
using CabinCrewCareer.Application.TestSessions.Queries.GetMyPracticalTestSessions;
using CabinCrewCareer.Application.TestSessions.Queries.GetMyTestSessionsByType;
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
        #region Get All Test Sessions
        /// <summary>
        /// Get all test sessions with pagination and filters
        /// </summary>
        /// <param name="searchTerm">Search by user name, email, or test name</param>
        /// <param name="sortColumn">Column to sort by (testSessionId, userId, testId, totalScore, startTime, endTime, createdAt)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="testId">Filter by test ID</param>
        /// <param name="userId">Filter by user ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <remarks>
        /// **Get all test sessions with flexible filtering and sorting**
        /// 
        /// **Security:**
        /// - Requires authentication
        /// 
        /// **Query Parameters:**
        /// - `searchTerm`: Search in user full name, email, or test name
        /// - `sortColumn`: Choose from testSessionId, userId, testId, totalScore, startTime, endTime, createdAt
        /// - `sortOrder`: Use "asc" for ascending or "desc" for descending (default: desc by createdAt)
        /// - `testId`: Filter sessions by specific test
        /// - `userId`: Filter sessions by specific user
        /// - `page`: Page number for pagination
        /// - `pageSize`: Number of items per page
        /// 
        /// **Response includes:**
        /// - Paginated list of test sessions
        /// - Each session contains user info, test info, scores, and timestamps
        /// - Total records and page information
        /// 
        /// **Example:**
        /// ```
        /// GET /api/v1/test-sessions?testId=75&amp;page=1&amp;pageSize=20&amp;sortColumn=createdAt&amp;sortOrder=desc
        /// ```
        /// </remarks>
        /// <returns>Paginated list of test sessions</returns>
        /// <response code="200">Ok - Test sessions retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<PaginatedList<GetTestSessionResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetAllTestSessionsAsync(
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int? testId,
            [FromQuery] int? userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAllTestSessionsQuery
            {
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                TestId = testId,
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            return Ok(await mediator.Send(query));
        }
        #endregion

        #region Get My Test Sessions By Type
        /// <summary>
        /// Get all test sessions by test type
        /// </summary>
        /// <param name="testType">Test type: 1 (English Listening), 2 (English Speaking), 3 (Practical)</param>
        /// <param name="userId">Filter by specific user ID (optional)</param>
        /// <param name="searchTerm">Search by test name, user name, or email</param>
        /// <param name="sortColumn">Column to sort by (testSessionId, testId, totalScore, startTime, endTime, createdAt)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <remarks>
        /// **Get all test sessions filtered by test type**
        /// 
        /// **Test Type Values:**
        /// - `1`: English Listening
        /// - `2`: English Speaking  
        /// - `3`: Practical
        /// 
        /// **Query Parameters:**
        /// - `testType`: Required - Filter by test type (1, 2, or 3)
        /// - `userId`: Optional - Filter by specific user ID
        /// - `searchTerm`: Search in test name, user name, or email
        /// - `sortColumn`: Choose from testSessionId, testId, totalScore, startTime, endTime, createdAt
        /// - `sortOrder`: Use "asc" for ascending or "desc" for descending (default: desc by createdAt)
        /// - `page`: Page number for pagination
        /// - `pageSize`: Number of items per page
        /// 
        /// **Response includes:**
        /// - Paginated list of all test sessions for the specified test type
        /// - Each session contains test info, user info, scores, and timestamps
        /// - Total records and page information
        /// 
        /// **Example:**
        /// ```
        /// GET /api/v1/test-sessions/by-type?testType=1&amp;page=1&amp;pageSize=10
        /// GET /api/v1/test-sessions/by-type?testType=2&amp;userId=5
        /// ```
        /// </remarks>
        /// <returns>Paginated list of all test sessions</returns>
        /// <response code="200">Ok - Test sessions retrieved successfully</response>
        /// <response code="400">Bad Request - Invalid test type</response>
        [HttpGet("by-type")]
        [ProducesResponseType(typeof(Response<PaginatedList<GetTestSessionResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetMyTestSessionsByTypeAsync(
            [FromQuery] int testType,
            [FromQuery] int? userId,
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetMyTestSessionsByTypeQuery
                {
                    TestType = testType,
                    UserId = userId,
                    SearchTerm = searchTerm,
                    SortColumn = sortColumn,
                    SortOrder = sortOrder,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(await mediator.Send(query));
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

        #region Get My Listening Test Sessions
        /// <summary>
        /// Get my listening test sessions - Returns only the logged-in user's listening test sessions
        /// </summary>
        /// <param name="searchTerm">Search by test name</param>
        /// <param name="sortColumn">Column to sort by (testSessionId, testId, totalScore, startTime, endTime, createdAt)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <remarks>
        /// **Get listening test sessions for the currently logged-in user**
        /// 
        /// **Security:**
        /// - Requires authentication (JWT token)
        /// - Returns only sessions belonging to the authenticated user
        /// - Filters by English Listening test type
        /// 
        /// **Query Parameters:**
        /// - `searchTerm`: Search in test name
        /// - `sortColumn`: Choose from testSessionId, testId, totalScore, startTime, endTime, createdAt
        /// - `sortOrder`: Use "asc" for ascending or "desc" for descending (default: desc by createdAt)
        /// - `page`: Page number for pagination
        /// - `pageSize`: Number of items per page
        /// 
        /// **Response includes:**
        /// - Paginated list of listening test sessions
        /// - Each session contains test info, scores, and timestamps
        /// - Total records and page information
        /// </remarks>
        /// <returns>Paginated list of listening test sessions</returns>
        /// <response code="200">Ok - Listening test sessions retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("my-listening-sessions")]
        [ProducesResponseType(typeof(Response<PaginatedList<GetTestSessionResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetMyListeningTestSessionsAsync(
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetMyListeningTestSessionsQuery
            {
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize
            };

            return Ok(await mediator.Send(query));
        }
        #endregion

        #region Get My Speaking Test Sessions
        /// <summary>
        /// Get my speaking test sessions - Returns only the logged-in user's speaking test sessions
        /// </summary>
        /// <param name="searchTerm">Search by test name</param>
        /// <param name="sortColumn">Column to sort by (testSessionId, testId, totalScore, startTime, endTime, createdAt)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <remarks>
        /// **Get speaking test sessions for the currently logged-in user**
        /// 
        /// **Security:**
        /// - Requires authentication (JWT token)
        /// - Returns only sessions belonging to the authenticated user
        /// - Filters by English Speaking test type
        /// 
        /// **Query Parameters:**
        /// - `searchTerm`: Search in test name
        /// - `sortColumn`: Choose from testSessionId, testId, totalScore, startTime, endTime, createdAt
        /// - `sortOrder`: Use "asc" for ascending or "desc" for descending (default: desc by createdAt)
        /// - `page`: Page number for pagination
        /// - `pageSize`: Number of items per page
        /// 
        /// **Response includes:**
        /// - Paginated list of speaking test sessions
        /// - Each session contains test info, scores, audio URLs, and timestamps
        /// - Total records and page information
        /// - Note: Speaking tests require manual grading, so scores may be 0 until graded
        /// </remarks>
        /// <returns>Paginated list of speaking test sessions</returns>
        /// <response code="200">Ok - Speaking test sessions retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("my-speaking-sessions")]
        [ProducesResponseType(typeof(Response<PaginatedList<GetTestSessionResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetMySpeakingTestSessionsAsync(
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetMySpeakingTestSessionsQuery
            {
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize
            };

            return Ok(await mediator.Send(query));
        }
        #endregion

        #region Get My Practical Test Sessions
        /// <summary>
        /// Get my practical test sessions - Returns only the logged-in user's practical test sessions
        /// </summary>
        /// <param name="searchTerm">Search by test name</param>
        /// <param name="sortColumn">Column to sort by (testSessionId, testId, totalScore, startTime, endTime, createdAt)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <remarks>
        /// **Get practical test sessions for the currently logged-in user**
        /// 
        /// **Security:**
        /// - Requires authentication (JWT token)
        /// - Returns only sessions belonging to the authenticated user
        /// - Filters by Practical test type
        /// 
        /// **Query Parameters:**
        /// - `searchTerm`: Search in test name
        /// - `sortColumn`: Choose from testSessionId, testId, totalScore, startTime, endTime, createdAt
        /// - `sortOrder`: Use "asc" for ascending or "desc" for descending (default: desc by createdAt)
        /// - `page`: Page number for pagination
        /// - `pageSize`: Number of items per page
        /// 
        /// **Response includes:**
        /// - Paginated list of practical test sessions
        /// - Each session contains test info, scores, and timestamps
        /// - Total records and page information
        /// </remarks>
        /// <returns>Paginated list of practical test sessions</returns>
        /// <response code="200">Ok - Practical test sessions retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("my-practical-sessions")]
        [ProducesResponseType(typeof(Response<PaginatedList<GetTestSessionResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetMyPracticalTestSessionsAsync(
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetMyPracticalTestSessionsQuery
            {
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize
            };

            return Ok(await mediator.Send(query));
        }
        #endregion

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
