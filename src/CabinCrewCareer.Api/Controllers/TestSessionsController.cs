using Asp.Versioning;
using CabinCrewCareer.Api.Models;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.TestSessions.Commands.SubmitMultipleChoiceTest;
using CabinCrewCareer.Application.TestSessions.Commands.SubmitSpeakingTest;
using CabinCrewCareer.Application.TestSessions.Commands.ScoreSpeakingTestSession;
using CabinCrewCareer.Application.TestSessions.Commands.ScoreTestAnswer;
using CabinCrewCareer.Application.TestSessions.Common;
using CabinCrewCareer.Application.TestSessions.Queries.GetAllTestSessions;
using CabinCrewCareer.Application.TestSessions.Queries.GetMyListeningTestSessions;
using CabinCrewCareer.Application.TestSessions.Queries.GetMySpeakingTestSessions;
using CabinCrewCareer.Application.TestSessions.Queries.GetMyPracticalTestSessions;
using CabinCrewCareer.Application.TestSessions.Queries.GetMyTestSessionsByType;
using CabinCrewCareer.Application.TestSessions.Queries.GetTestSessionById;
using CabinCrewCareer.Application.TestSessions.Queries.GetTestAnswersBySessionId;
using CabinCrewCareer.Application.TestSessions.Queries.GetTestSessionsByRound;
using CabinCrewCareer.Application.TestSessions.Queries.GetMyEnglishTestSessions;
using CabinCrewCareer.Application.TestSessions.Queries.GetMyPracticalTestSessionsWithStatus;
using CabinCrewCareer.Application.TestSessions.Queries.GetTestAnswersWithCriteria;
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
        /// <param name="roundId">Filter by specific round ID (optional)</param>
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
        /// - `roundId`: Optional - Filter by specific round ID (filters test sessions by tests assigned to that round)
        /// - `searchTerm`: Search in test name, user name, or email
        /// - `sortColumn`: Choose from testSessionId, testId, totalScore, startTime, endTime, createdAt
        /// - `sortOrder`: Use "asc" for ascending or "desc" for descending (default: desc by createdAt)
        /// - `page`: Page number for pagination
        /// - `pageSize`: Number of items per page
        /// 
        /// **Response includes:**
        /// - Paginated list of all test sessions for the specified test type
        /// - Each session contains test info, user info (including profile image URL), scores, and timestamps
        /// - Total records and page information
        /// 
        /// **Example:**
        /// ```
        /// GET /api/v1/test-sessions/by-type?testType=1&amp;page=1&amp;pageSize=10
        /// GET /api/v1/test-sessions/by-type?testType=2&amp;userId=5
        /// GET /api/v1/test-sessions/by-type?testType=2&amp;roundId=10
        /// GET /api/v1/test-sessions/by-type?testType=1&amp;userId=5&amp;roundId=10
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
            [FromQuery] int? roundId,
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
                    RoundId = roundId,
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

        #region Get Test Sessions By Round
        /// <summary>
        /// Get all test sessions by round with RoundActivity status
        /// </summary>
        /// <param name="roundId">Round ID to get test sessions for</param>
        /// <param name="testType">Optional test type filter: 1 (English Listening), 2 (English Speaking), 3 (Practical)</param>
        /// <param name="searchTerm">Search by user name or email</param>
        /// <param name="sortColumn">Column to sort by (testSessionId, username, totalScore, startTime, endTime, createdAt)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <remarks>
        /// **Get all test sessions for a specific round with their RoundActivity status**
        /// 
        /// **Security:**
        /// - Requires authentication
        /// - Typically used by Admin/Examiner to review candidate progress
        /// 
        /// **Query Parameters:**
        /// - `roundId`: Required - The round to get test sessions for
        /// - `testType`: Optional filter by test type (1: Listening, 2: Speaking, 3: Practical)
        /// - `searchTerm`: Search in user full name or email
        /// - `sortColumn`: Choose from testSessionId, username, totalScore, startTime, endTime, createdAt
        /// - `sortOrder`: Use "asc" or "desc" (default: desc by createdAt)
        /// 
        /// **Response includes:**
        /// - Test session details (scores, timestamps, etc.)
        /// - User information
        /// - **ActivityStatus**: "Ongoing", "Passed", or "Failed" based on RoundActivity
        /// - RoundId, RoundType, and ActivityId
        /// 
        /// **Activity Status Logic:**
        /// - "Ongoing": User is still in this round (default state)
        /// - "Passed": User has passed this round (score >= 80%)
        /// - "Failed": User has failed this round (score less than 80%)
        /// 
        /// **Example:**
        /// ```
        /// GET /api/v1/test-sessions/by-round?roundId=12&amp;testType=1&amp;page=1&amp;pageSize=20
        /// ```
        /// 
        /// **Response Example:**
        /// ```json
        /// {
        ///   "data": {
        ///     "items": [
        ///       {
        ///         "testSessionId": 501,
        ///         "userId": 301,
        ///         "userFullName": "Nguyen Van A",
        ///         "testId": 6,
        ///         "testName": "Listening Test",
        ///         "testType": "EnglishListening",
        ///         "totalScore": 110,
        ///         "maxScore": 120,
        ///         "status": true,
        ///         "activityStatus": "Passed",
        ///         "roundId": 12,
        ///         "roundType": "EnglishListeningTest",
        ///         "activityId": 201
        ///       }
        ///     ],
        ///     "page": 1,
        ///     "pageSize": 20,
        ///     "totalRecords": 1,
        ///     "totalPages": 1
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <returns>Paginated list of test sessions with RoundActivity status</returns>
        /// <response code="200">Ok - Test sessions retrieved successfully</response>
        /// <response code="400">Bad Request - Invalid round ID or test type</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("by-round")]
        [ProducesResponseType(typeof(Response<PaginatedList<TestSessionWithStatusResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetTestSessionsByRoundAsync(
            [FromQuery] int roundId,
            [FromQuery] int? testType,
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetTestSessionsByRoundQuery
                {
                    RoundId = roundId,
                    TestType = testType.HasValue ? (Domain.Common.TestType)testType.Value : null,
                    SearchTerm = searchTerm,
                    SortColumn = sortColumn,
                    SortOrder = sortOrder,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(await mediator.Send(query));
            }
            catch (ValidationException ex)
            {
                return BadRequest(new Response<string>(code: (int)HttpStatusCode.BadRequest, message: ex.Message));
            }
        }
        #endregion

        #region Get My English Test Sessions (Candidate)
        /// <summary>
        /// Get my English test sessions (Listening + Speaking) with RoundActivity status - For Candidates
        /// </summary>
        /// <param name="roundId">Optional round ID filter</param>
        /// <param name="searchTerm">Search by test name</param>
        /// <param name="sortColumn">Column to sort by (testSessionId, testtype, totalScore, startTime, endTime, createdAt)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <remarks>
        /// **Get my English test sessions (both Listening and Speaking) with pass/fail status**
        /// 
        /// **Security:**
        /// - Only accessible by authenticated users (typically Candidates)
        /// - Uses JWT token to identify the current user
        /// 
        /// **Returns:**
        /// - All English Listening test sessions
        /// - All English Speaking test sessions
        /// - RoundActivity status for each session
        /// 
        /// **Activity Status:**
        /// - "Ongoing": Test session exists but not yet evaluated
        /// - "Passed": Score greater than or equal to 80% of MaxScore
        /// - "Failed": Score less than 80% of MaxScore
        /// 
        /// **Use Case:**
        /// Candidates can view their English test results and see if they passed/failed
        /// each test round. Both Listening and Speaking tests are returned together.
        /// 
        /// **Example:**
        /// ```
        /// GET /api/v1/test-sessions/my-english?page=1&amp;pageSize=10
        /// ```
        /// 
        /// **Response Example:**
        /// ```json
        /// {
        ///   "data": {
        ///     "items": [
        ///       {
        ///         "testSessionId": 601,
        ///         "testName": "Listening Test",
        ///         "testType": "EnglishListening",
        ///         "totalScore": 110,
        ///         "maxScore": 120,
        ///         "activityStatus": "Passed",
        ///         "roundType": "EnglishListeningTest"
        ///       },
        ///       {
        ///         "testSessionId": 701,
        ///         "testName": "Speaking Test",
        ///         "testType": "EnglishSpeaking",
        ///         "totalScore": 85,
        ///         "maxScore": 100,
        ///         "activityStatus": "Passed",
        ///         "roundType": "EnglishSpeakingTest"
        ///       }
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <returns>Paginated list of my English test sessions with status</returns>
        /// <response code="200">Ok - Test sessions retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("my-english")]
        [ProducesResponseType(typeof(Response<PaginatedList<TestSessionWithStatusResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetMyEnglishTestSessionsAsync(
            [FromQuery] int? roundId,
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetMyEnglishTestSessionsQuery
            {
                RoundId = roundId,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize
            };

            return Ok(await mediator.Send(query));
        }
        #endregion

        #region Get My Practical Test Sessions (Cabin Crew)
        /// <summary>
        /// Get my Practical test sessions with RoundActivity status - For Cabin Crew
        /// </summary>
        /// <param name="roundId">Optional round ID filter</param>
        /// <param name="searchTerm">Search by test name</param>
        /// <param name="sortColumn">Column to sort by (testSessionId, totalScore, startTime, endTime, createdAt)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <remarks>
        /// **Get my Practical test sessions with pass/fail status**
        /// 
        /// **Security:**
        /// - Only accessible by authenticated users (typically Cabin Crew applicants)
        /// - Uses JWT token to identify the current user
        /// 
        /// **Returns:**
        /// - All Practical test sessions for the current user
        /// - RoundActivity status for each session
        /// 
        /// **Activity Status:**
        /// - "Ongoing": Test session exists but not yet evaluated
        /// - "Passed": Score greater than or equal to 80% of MaxScore
        /// - "Failed": Score less than 80% of MaxScore
        /// 
        /// **Use Case:**
        /// Cabin crew applicants can view their practical test results and see if 
        /// they passed or failed. Since practical tests only require one round to pass,
        /// the status directly indicates if they can move to the interview round.
        /// 
        /// **Example:**
        /// ```
        /// GET /api/v1/test-sessions/my-practical?page=1&amp;pageSize=10
        /// ```
        /// 
        /// **Response Example:**
        /// ```json
        /// {
        ///   "data": {
        ///     "items": [
        ///       {
        ///         "testSessionId": 501,
        ///         "testName": "Practical Exam",
        ///         "testType": "Practical",
        ///         "totalScore": 85,
        ///         "maxScore": 100,
        ///         "activityStatus": "Passed",
        ///         "roundType": "PracticalTest"
        ///       }
        ///     ]
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <returns>Paginated list of my practical test sessions with status</returns>
        /// <response code="200">Ok - Test sessions retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("my-practical")]
        [ProducesResponseType(typeof(Response<PaginatedList<TestSessionWithStatusResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetMyPracticalTestSessionsAsync(
            [FromQuery] int? roundId,
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetMyPracticalTestSessionsWithStatusQuery
            {
                RoundId = roundId,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize
            };

            return Ok(await mediator.Send(query));
        }
        #endregion

        #region Get My Listening Test Sessions
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

        #region Get Test Session By ID
        /// <summary>
        /// Get test session by ID
        /// </summary>
        /// <param name="id">Test session ID</param>
        /// <remarks>
        /// **Get detailed information of a specific test session**
        /// 
        /// **Security:**
        /// - Requires authentication
        /// 
        /// **Response includes:**
        /// - Complete test session information
        /// - User details (full name, email)
        /// - Test details (name, type, max score)
        /// - Session scores and timestamps
        /// - Total number of answers submitted
        /// 
        /// **Example:**
        /// ```
        /// GET /api/v1/test-sessions/5
        /// ```
        /// </remarks>
        /// <returns>Test session details</returns>
        /// <response code="200">Ok - Test session retrieved successfully</response>
        /// <response code="400">Bad Request - Test session not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<GetTestSessionResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetTestSessionByIdAsync(int id)
        {
            try
            {
                var query = new GetTestSessionByIdQuery
                {
                    TestSessionId = id
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

        #region Score Speaking Test Session
        /// <summary>
        /// Score a speaking test session by grading criteria
        /// </summary>
        /// <param name="id">Test session ID</param>
        /// <param name="request">Scoring criteria with scores</param>
        /// <remarks>
        /// **Score a speaking test session based on multiple criteria**
        /// 
        /// **Security:**
        /// - Requires authentication
        /// - Typically used by examiners/admin
        /// 
        /// **Request Body:**
        /// - `criteriaScores`: Dictionary of criteria names and their scores
        ///   - Key: Criteria name (e.g., "Pronunciation", "Fluency", "Grammar", "Vocabulary")
        ///   - Value: Score for that criteria (must be non-negative)
        /// 
        /// **Process:**
        /// 1. Validates test session exists
        /// 2. Validates all scores are non-negative
        /// 3. Calculates total score by summing all criteria scores
        /// 4. Updates the test session's total score
        /// 
        /// **Example Request:**
        /// ```json
        /// {
        ///   "criteriaScores": {
        ///     "Pronunciation": 20,
        ///     "Fluency": 18,
        ///     "Grammar": 22,
        ///     "Vocabulary": 19,
        ///     "Comprehension": 21
        ///   }
        /// }
        /// ```
        /// **Example Result:** Total Score = 100
        /// </remarks>
        /// <returns>Success message with total score</returns>
        /// <response code="200">Ok - Test session scored successfully</response>
        /// <response code="400">Bad Request - Invalid data or test session not found</response>
        [HttpPut("{id}/score")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ScoreSpeakingTestSessionAsync(int id, [FromBody] ScoreSpeakingTestSessionRequest request)
        {
            try
            {
                var command = new ScoreSpeakingTestSessionCommand
                {
                    TestSessionId = id,
                    CriteriaScores = request.CriteriaScores ?? new Dictionary<string, int>()
                };

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

        #region Get Test Answers By Session ID
        /// <summary>
        /// Get all test answers for a specific test session
        /// </summary>
        /// <param name="id">Test session ID</param>
        /// <remarks>
        /// **Get detailed answers submitted in a test session**
        /// 
        /// **Security:**
        /// - Requires authentication
        /// 
        /// **Response includes:**
        /// - List of all answers submitted in the test session
        /// - For multiple choice: selected option details and question content
        /// - For speaking: audio file URL
        /// - For essay: essay answer text
        /// - Score and correctness for each answer
        /// 
        /// **Example:**
        /// ```
        /// GET /api/v1/test-sessions/5/answers
        /// ```
        /// </remarks>
        /// <returns>List of test answers</returns>
        /// <response code="200">Ok - Test answers retrieved successfully</response>
        /// <response code="400">Bad Request - Test session not found</response>
        [HttpGet("{id}/answers")]
        [ProducesResponseType(typeof(Response<List<TestAnswerDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetTestAnswersBySessionIdAsync(int id)
        {
            try
            {
                var query = new GetTestAnswersBySessionIdQuery
                {
                    TestSessionId = id
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

        #region Score Test Answer
        /// <summary>
        /// Score multiple test answers for a test session with multiple criteria per answer
        /// </summary>
        /// <param name="testSessionId">Test session ID</param>
        /// <param name="request">Dictionary of answer IDs and their criteria scores</param>
        /// <remarks>
        /// **Score multiple test answers at once using multiple criteria and update test session total score**
        /// 
        /// **Security:**
        /// - Requires authentication
        /// - Typically used by examiners/admin
        /// 
        /// **Request Body:**
        /// - `answerScores`: Dictionary of answer IDs (key) and score information (value)
        ///   - Key: Answer ID (integer)
        ///   - Value: Object with `criteriaScores` (dictionary) and `isCorrect` (boolean)
        ///     - `criteriaScores`: Dictionary of criteria names (key) and scores (value)
        ///       - Key: Criteria name (e.g., "Pronunciation", "Grammar", "Content")
        ///       - Value: Score for that criteria (non-negative integer)
        ///     - `isCorrect`: Whether the answer is correct
        /// 
        /// **Validation:**
        /// 1. Test session must exist
        /// 2. All test answers must exist and belong to the test session
        /// 3. Test answers must not have been scored yet (score = 0)
        /// 4. All criteria scores must be non-negative
        /// 5. Sum of criteria scores cannot exceed the maximum score of each question
        /// 
        /// **Process:**
        /// 1. Validates all test answers
        /// 2. For each answer, sums all criteria scores to get the answer's total score
        /// 3. Validates total score doesn't exceed question's max score
        /// 4. Updates score and correctness for each answer
        /// 5. Calculates test session total score by summing all answer scores
        /// 6. Updates test session's total score
        /// 
        /// **Example Request:**
        /// ```json
        /// {
        ///   "answerScores": {
        ///     "121": {
        ///       "criteriaScores": {
        ///         "Pronunciation": 3,
        ///         "Fluency": 2,
        ///         "Grammar": 3
        ///       },
        ///       "isCorrect": true
        ///     },
        ///     "122": {
        ///       "criteriaScores": {
        ///         "Pronunciation": 2,
        ///         "Fluency": 3,
        ///         "Grammar": 2
        ///       },
        ///       "isCorrect": true
        ///     }
        ///   }
        /// }
        /// ```
        /// **Example Result:** 
        /// - Answer 121 Score = 8 (3+2+3)
        /// - Answer 122 Score = 7 (2+3+2)
        /// - Total Score = 15
        /// 
        /// **Use Cases:**
        /// - Scoring speaking test answers with detailed criteria (pronunciation, fluency, grammar, etc.)
        /// - Scoring essay answers with multiple rubrics (content, organization, language use, etc.)
        /// - Batch scoring multiple answers with granular feedback
        /// </remarks>
        /// <returns>Success message with total score details</returns>
        /// <response code="200">Ok - Test answers scored successfully</response>
        /// <response code="400">Bad Request - Invalid data or answers already scored</response>
        [HttpPut("{testSessionId}/answers/score")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ScoreTestAnswerAsync(int testSessionId, [FromBody] ScoreTestAnswerRequest request)
        {
            try
            {
                var command = new ScoreTestAnswerCommand
                {
                    TestSessionId = testSessionId,
                    AnswerScores = request.AnswerScores.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new Application.TestSessions.Commands.ScoreTestAnswer.AnswerScoreDto
                        {
                            CriteriaScores = kvp.Value.CriteriaScores,
                            IsCorrect = kvp.Value.IsCorrect
                        }
                    )
                };

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

        #region Get Test Answers With Criteria Scores
        /// <summary>
        /// Get test answers with criteria scores breakdown for a test session
        /// </summary>
        /// <param name="testSessionId">Test session ID</param>
        /// <remarks>
        /// **Get detailed test answers including criteria scores breakdown**
        /// 
        /// This endpoint retrieves all test answers for a specific test session and parses 
        /// the EssayAnswer field to extract:
        /// - Individual criteria scores (e.g., Pronunciation, Fluency, Grammar)
        /// - Student's original essay/answer content (if exists)
        /// - Question information
        /// 
        /// **Security:**
        /// - Requires authentication
        /// 
        /// **Use Cases:**
        /// - Review detailed scoring breakdown for speaking tests
        /// - View essay answers with rubric scores
        /// - Display criteria-based feedback to students
        /// - Analyze scoring patterns for quality assurance
        /// 
        /// **Response Structure:**
        /// ```json
        /// {
        ///   "code": 200,
        ///   "data": [
        ///     {
        ///       "answerId": 1,
        ///       "score": 24,
        ///       "isCorrect": true,
        ///       "criteriaScores": {
        ///         "Pronunciation": 8,
        ///         "Fluency": 7,
        ///         "Grammar": 9
        ///       },
        ///       "studentAnswer": "This is my essay...",
        ///       "answerAudioFileURL": "https://...",
        ///       "question": {
        ///         "questionId": 5,
        ///         "questionContent": "Describe your favorite place",
        ///         "orderNumber": 1,
        ///         "maxScore": 30
        ///       }
        ///     }
        ///   ]
        /// }
        /// ```
        /// 
        /// **Example:**
        /// ```
        /// GET /api/v1/test-sessions/123/answers-with-criteria
        /// ```
        /// </remarks>
        /// <returns>List of test answers with parsed criteria scores</returns>
        /// <response code="200">Ok - Test answers retrieved successfully</response>
        /// <response code="400">Bad Request - Test session not found</response>
        [HttpGet("{testSessionId}/answers-with-criteria")]
        [ProducesResponseType(typeof(Response<List<TestAnswerWithCriteriaResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetTestAnswersWithCriteriaAsync(int testSessionId)
        {
            try
            {
                var query = new GetTestAnswersWithCriteriaQuery
                {
                    TestSessionId = testSessionId
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
    }
}
