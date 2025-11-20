using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.RoundActivities.Common;
using CabinCrewCareer.Application.RoundActivities.Queries;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/rounds")]
    [ApiVersion(1)]
    [Authorize]
    public class RoundsController(
        IMediator mediator,
        ILogger<RoundsController> logger) : ApiController(mediator, logger)
    {
        #region Get All Participants
        /// <summary>
        /// Get all participants of a specific round
        /// </summary>
        /// <param name="searchTerm">Search by participant's full name</param>
        /// <param name="sortColumn">Sort on a specifice column. Example: "name"</param>
        /// <param name="sortOrder">"asc" or "desc"</param>
        /// <param name="status">1: Ongoing, 2: Passed, 3: Failed</param>
        /// <param name="roundId">Id of a specific round</param>
        /// <param name="page">Current page</param>
        /// <param name="pageSize">Number of records of a specific page</param>
        /// <returns>A list of all users</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{roundId}/participants")]
        [ProducesResponseType(typeof(Response<PaginatedList<GetActivityResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync(int roundId, [FromQuery]
        string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        int? status,
        int page = 1,
        int pageSize = 5)
        {
            try
            {
                return Ok(await mediator.Send(
                    new GetRoundActivitiesQuery(
                        searchTerm,
                        sortColumn,
                        sortOrder,
                        status,
                        roundId,
                        page,
                        pageSize)));
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
                #region Update TestId for Round
                /// <summary>
                /// Update testId for a specific round
                /// </summary>
                /// <param name="request">UpdateRoundTestIdRequest</param>
                /// <returns>True if updated successfully</returns>
                [HttpPut("update-test-id")]
                [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
                [ProducesResponseType((int)HttpStatusCode.BadRequest)]
                public async Task<IActionResult> UpdateTestIdAsync([FromBody] CabinCrewCareer.Application.Rounds.Commands.UpdateRoundTestIdRequest request)
                {
                    try
                    {
                        var command = new Application.Rounds.Commands.UpdateRoundTestIdCommand(request.RoundId, request.TestId);
                        var result = await mediator.Send(command);
                        return Ok(result);
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
        #endregion
    }
}
