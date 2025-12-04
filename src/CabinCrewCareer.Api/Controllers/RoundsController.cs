using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.RoundActivities.Common;
using CabinCrewCareer.Application.RoundActivities.Queries;
using CabinCrewCareer.Application.Rounds.Queries;
using CabinCrewCareer.Application.Rounds.Commands.UpdateRoundDates;
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
        ILogger<RoundsController> logger,IUserExportService userExportService) : ApiController(mediator, logger)
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
        #endregion

        #region Get Test Info by RoundId
        /// <summary>
        /// Get test info by roundId
        /// </summary>
        /// <param name="roundId">Id of the round</param>
        /// <returns>Test info of the round</returns>
        [HttpGet("{roundId}/test-info")]
        [ProducesResponseType(typeof(RoundTestInfoResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetTestInfoByRoundIdAsync(int roundId)
        {
            try
            {
                var result = await mediator.Send(new GetRoundTestInfoQuery(roundId));
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
         #region Export Users By Round
                /// <summary>
                /// Export danh sách user theo roundId, type Final, round activities status Ongoing
                /// </summary>
                /// <param name="roundId">Id của round</param>
                /// <returns>File Excel danh sách user</returns>
                [HttpGet("{roundId}/export-users")]
                [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.OK)]
                public async Task<IActionResult> ExportUsersByRoundAsync( int roundId)
                {
                    // Lấy tên người login từ context (có thể lấy từ claim hoặc User.Identity.Name)
                    var representativeName = User?.Identity?.Name ?? "Ông/Bà";
                    var fileBytes = await userExportService.ExportUsersByRoundAsync(roundId, representativeName);
                    var fileName = $"FinalRound_CandidateList.xlsx";
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                #endregion
                #region Update Round Dates
        /// <summary>
        /// Update start date and end date for a specific round
        /// </summary>
        /// <param name="id">Id of the round</param>
        /// <param name="dto">StartDate, EndDate</param>
        /// <returns>True if updated successfully</returns>
        [HttpPut("{id}/dates")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateRoundDatesAsync(int id, [FromBody] UpdateRoundDatesDto dto)
        {
            try
            {
                var command = new CabinCrewCareer.Application.Rounds.Commands.UpdateRoundDates.UpdateRoundDatesCommand
                {
                    RoundId = id,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate
                };
                var result = await mediator.Send(command);
                if (result)
                    return Ok(true);
                return BadRequest(new { success = false, message = "Invalid round date update." });
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
