using CabinCrewCareer.Application.Tests.Commands.CreateTest;
using CabinCrewCareer.Application.Tests.Queries.GetTests;
using CabinCrewCareer.Application.Tests.Common;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Wrappers;
using CabinCrewCareer.Api.Models;
using Asp.Versioning;
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
        /// Examiner creates a new test with audio file upload to Firebase
        /// </summary>
        /// <param name="request">Test creation request with audio file</param>
        /// <remarks>
        /// **Upload audio file for test (multipart/form-data)**
        /// 
        /// TestType: 
        /// - 1 = English (requires audio file)
        /// - 2 = Practical (audio file optional)
        /// 
        /// Audio file requirements:
        /// - Format: .mp3 or .wav
        /// - Max size: 50 MB
        /// - Will be uploaded to Firebase Storage
        /// 
        /// Note: Only Examiner role can create tests.
        /// System will auto-generate unique 10-character join code.
        ///         
        /// </remarks> 
        /// <returns>Created test with Firebase Storage URL</returns>
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
        /// <param name="sortColumn">Column to sort by (name, testtype, maxscore, createdat). Default: testid</param>
        /// <param name="sortOrder">Sort order (asc or desc). Default: desc</param>
        /// <param name="testType">Filter by test type (1=English, 2=Practical)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
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
    }
}
