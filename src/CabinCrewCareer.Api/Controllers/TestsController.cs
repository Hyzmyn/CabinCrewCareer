using Asp.Versioning;
using CabinCrewCareer.Api.Models;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.Tests.Commands.CreateTest;
using CabinCrewCareer.Application.Tests.Commands.DeleteTest;
using CabinCrewCareer.Application.Tests.Commands.UpdateTest;
using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Application.Tests.Queries.GetAvailableTests;
using CabinCrewCareer.Application.Tests.Queries.GetTestById;
using CabinCrewCareer.Application.Tests.Queries.GetTests;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/tests")]
    [ApiVersion(1)]
    [Authorize]
    public class TestsController(
        IMediator mediator,
        ILogger<TestsController> logger) : ApiController(mediator, logger)
    {
        #region Create Test
        /// <summary>
        /// Examiner creates a new test with audio file upload to Cloudinary
        /// </summary>
        /// <param name="request">Test creation request with audio file</param>
        /// <remarks>
        /// **Upload audio file for test (multipart/form-data)**
        /// 
        /// TestType: 
        /// - 1 = EnglishListening (requires audio file)
        /// - 2 = EnglishSpeaking (audio file optional)
        /// - 3 = Practical (audio file optional)
        /// 
        /// Audio file requirements:
        /// - Format: .mp3 or .wav
        /// - Max size: 50 MB
        /// - Will be uploaded to Cloudinary Storage
        /// - Required only for EnglishListening tests
        /// 
        /// DurationInMinutes:
        /// - Must be between 1 and 480 minutes (8 hours)
        /// - Recommended: 30-180 minutes for most tests
        /// 
        /// Note: Only Examiner role can create tests.
        /// System will auto-generate unique 10-character join code.
        ///         
        /// </remarks> 
        /// <returns>Created test with Cloudinary Storage URL</returns>
        /// <response code="200">Ok - Test created successfully</response>
        /// <response code="400">Bad Request - Validation errors</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Not an Examiner</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Response<GetTestResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateTestAsync([FromForm] CreateTestRequest request)
        {
            try
            {
                var command = new CreateTestCommand(
                    request.TestName,
                    request.Purpose,
                    (CabinCrewCareer.Domain.Common.TestType)request.TestType,
                    request.MaxScore,
                    request.DurationInMinutes,
                    request.AudioFile);

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

        #region Get Tests List
        /// <summary>
        /// Get list of tests with pagination and filtering
        /// </summary>
        /// <param name="searchTerm">Search by test name or purpose</param>
        /// <param name="sortColumn">Column to sort by (name, testtype, maxscore, duration, createdat). Default: testid</param>
        /// <param name="sortOrder">Sort order (asc or desc). Default: desc</param>
        /// <param name="testType">Filter by test type (1=EnglishListening, 2=EnglishSpeaking, 3=Practical)</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated list of tests</returns>
        /// <response code="200">Ok - Returns paginated list of tests</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<PaginatedList<GetTestResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetTestsAsync(
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int? testType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetTestsQuery
                {
                    SearchTerm = searchTerm,
                    SortColumn = sortColumn,
                    SortOrder = sortOrder,
                    TestType = testType,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(await mediator.Send(query));
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

        #region Get Tests List for Round Selection
        /// <summary>
        /// Get list of tests for round selection
        /// </summary>
        /// <param name="testType">Filter by test type (1 = English Listening, 2 = English Speaking, 3 = Practical)</param>
        /// <returns>Paginated list of tests</returns>
        /// <response code="200">Ok - Returns paginated list of tests</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("for-rounds")]
        [ProducesResponseType(typeof(Response<PaginatedList<GetTestResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAvailableTestsAsync(
            [FromQuery] int? testType)
        {
            try
            {
                var query = new GetAvailableTestsQuery(testType);

                return Ok(await mediator.Send(query));
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

        #region Get Test By Id
        /// <summary>
        /// Get test details by ID
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <remarks>
        /// **Get detailed information of a test by ID**
        /// 
        /// Response behavior based on TestType:
        /// - **EnglishListening (1)**: Returns AudioFileURL if available
        /// - **EnglishSpeaking (2)**: AudioFileURL field is omitted from response
        /// - **Practical (3)**: AudioFileURL field is omitted from response
        /// 
        /// Note: All authenticated users can view test details.
        /// </remarks>
        /// <returns>Test details</returns>
        /// <response code="200">Ok - Returns test details</response>
        /// <response code="400">Bad Request - Invalid test ID</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="404">Not Found - Test not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<GetTestByIdResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTestByIdAsync(int id)
        {
            try
            {
                var query = new GetTestByIdQuery(id);
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
                return NotFound(new
                {
                    errorCode = e.ErrorCode,
                    error = e.Error,
                    errorMessage = e.ErrorMessage
                });
            }
        }
        #endregion

        #region Update Test
        /// <summary>
        /// Examiner updates an existing test
        /// </summary>
        /// <param name="id">Test ID to update</param>
        /// <param name="request">Test update request with audio file</param>
        /// <remarks>
        /// **Update test with optional audio file upload (multipart/form-data)**
        /// 
        /// TestType: 
        /// - 1 = EnglishListening (audio file optional for update)
        /// - 2 = EnglishSpeaking (audio file optional)
        /// - 3 = Practical (audio file optional)
        /// 
        /// Audio file handling:
        /// - If AudioFile provided: Upload new file and replace existing
        /// - If RemoveAudio = true: Remove existing audio file
        /// - If neither: Keep existing audio file
        /// 
        /// Note: Only Examiner role can update tests.
        /// </remarks> 
        /// <returns>Updated test information</returns>
        /// <response code="200">Ok - Test updated successfully</response>
        /// <response code="400">Bad Request - Validation errors</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Not an Examiner</response>
        /// <response code="404">Not Found - Test not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Response<GetTestResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateTestAsync(int id, [FromForm] UpdateTestRequest request)
        {
            try
            {
                var command = new UpdateTestCommand(
                    id,
                    request.TestName,
                    request.Purpose,
                    (CabinCrewCareer.Domain.Common.TestType)request.TestType,
                    request.MaxScore,
                    request.DurationInMinutes,
                    request.AudioFile,
                    request.RemoveAudio);

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
                if (e.ErrorCode == (int)ResponseCode.TEST_NOT_FOUND)
                {
                    return NotFound(new
                    {
                        errorCode = e.ErrorCode,
                        error = e.Error,
                        errorMessage = e.ErrorMessage
                    });
                }

                return BadRequest(new
                {
                    errorCode = e.ErrorCode,
                    error = e.Error,
                    errorMessage = e.ErrorMessage
                });
            }
        }
        #endregion

        #region Delete Test
        /// <summary>
        /// Examiner deletes a test
        /// </summary>
        /// <param name="id">Test ID to delete</param>
        /// <remarks>
        /// **Delete test and associated audio file**
        /// 
        /// - Deletes test from database
        /// - Removes audio file from Cloudinary (if exists)
        /// - Cannot delete test if students have taken it
        /// 
        /// Note: Only Examiner role can delete tests.
        /// </remarks> 
        /// <returns>Deletion success status</returns>
        /// <response code="200">Ok - Test deleted successfully</response>
        /// <response code="400">Bad Request - Cannot delete test with sessions</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Not an Examiner</response>
        /// <response code="404">Not Found - Test not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Response<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteTestAsync(int id)
        {
            try
            {
                var command = new DeleteTestCommand(id);
                return Ok(await mediator.Send(command));
            }
            catch (ApiException e)
            {
                if (e.ErrorCode == (int)ResponseCode.TEST_NOT_FOUND)
                {
                    return NotFound(new
                    {
                        errorCode = e.ErrorCode,
                        error = e.Error,
                        errorMessage = e.ErrorMessage
                    });
                }

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
