using Asp.Versioning;
using CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/campaigns")]
    [ApiVersion(1)]
    [Authorize]
    public class CampaignsController(
        IMediator mediator,
        ILogger<CampaignsController> logger) : ApiController(mediator, logger)
    {

        #region Get Campaigns
        /// <summary>
        /// Get all campaigns
        /// </summary>
        /// <param name="searchTerm">Search by campaign's name</param>
        /// <param name="sortColumn">Sort on a specifice column. Example: "name"</param>
        /// <param name="sortOrder">"asc" or "desc"</param>
        /// <param name="campaignType">1: Recruitment, 2: Promotion</param>
        /// <param name="campaignStatus">0: Draft, 1: Pending, 2: Approved, 3: Rejected, 4: Cancelled, 5: Ongoing, 6: Upcoming, 7: Ended</param>
        /// <param name="partnerId">1: VietJet Air, 3: Vietname Airlines, 4: Bamboo Airways, 5: SunPhuQuoc Airways</param>
        /// <param name="page">Current page</param>
        /// <param name="pageSize">Number of records of a specific page</param>
        /// <returns>A list of all campaigns</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<PaginatedList<GetCampaignListDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync([FromQuery]
        string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        int? campaignType,
        int? campaignStatus,
        int? partnerId,
        int page = 1,
        int pageSize = 5)
        {
            try
            {
                return Ok(await mediator.Send(
                    new GetCampaignListQuery(
                        searchTerm,
                        sortColumn,
                        sortOrder,
                        campaignType,
                        campaignStatus,
                        partnerId,
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

        /// <summary>
        /// Get the detail of a campaign by id.
        /// </summary>
        /// <param name="id">Campaign id</param>
        /// <returns>Campaign detail.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetailAsync(int id)
        {
            try
            {
                var result = await mediator.Send(new CabinCrewCareer.Application.Campaigns.Queries.GetCampaignDetail.GetCampaignDetailQuery(id));
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in GetDetailAsync for id {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update campaign information and create rounds for the campaign.
        /// </summary>
        /// <param name="id">Campaign id</param>
        /// <param name="dto">Update and create round data</param>
        /// <returns>Response with campaign detail or error information.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/v1/campaigns/1/campaign-rounds
        ///     {
        ///         "startDate": "2025-12-06",
        ///         "endDate": "2026-01-30",
        ///         "rounds": [
        ///             {
        ///                 "roundName": "Round 1",
        ///                 "description": "First recruitment round",
        ///             "targetQuantity": 21,
        ///             "roundStartDate": "2025-12-06",
        ///            "roundEndDate": "2025-12-25",
        ///             "roundDates": [
        ///                 { "startDate": "2025-12-07", "endDate": "2025-12-09" },
        ///                 { "startDate": "2025-12-10", "endDate": "2025-12-12" },
        ///                 { "startDate": "2025-12-13", "endDate": "2025-12-15" },
        ///                 { "startDate": "2025-12-16", "endDate": "2025-12-18" },
        ///                 { "startDate": "2025-12-19", "endDate": "2025-12-22" },
        ///                 { "startDate": "2025-12-23", "endDate": "2025-12-25" }
        ///             ]
        ///         },
        ///         {
        ///             "roundName": "Round 2", 
        ///             "description": "Second recruitment round",
        ///             "targetQuantity": 9,
        ///             "roundStartDate": "2025-12-27",
        ///             "roundEndDate": "2026-01-30",
        ///             "roundDates": [
        ///                 { "startDate": "2025-12-27", "endDate": "2025-12-29" },
        ///                 { "startDate": "2025-12-30", "endDate": "2026-01-02" },
        ///                 { "startDate": "2026-01-05", "endDate": "2026-01-08" },
        ///                 { "startDate": "2026-01-10", "endDate": "2026-01-13" },
        ///                 { "startDate": "2026-01-15", "endDate": "2026-01-18" },
        ///                 { "startDate": "2026-01-19", "endDate": "2026-01-22" }
        ///             ]
        ///         }
        ///     ]
        /// }
        ///
        /// Notes:
        /// - Campaign must be in Draft or Rejected status
        /// - Round 1: targetQuantity must be 60-70% of campaign target
        /// - Middle rounds: targetQuantity must be 60-70% of remaining
        /// - Last round: targetQuantity must be ≥80% of remaining
        /// - After success, campaign status changes to Pending
        /// </remarks>
        [HttpPut("{id}/campaign-rounds")]
        [ProducesResponseType(typeof(Response<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateAndCreateRoundAsync(int id, [FromBody] CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignAndCreateRound.UpdateCampaignAndCreateRoundDto dto)
        {
            try
            {
                var command = new CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignAndCreateRound.UpdateCampaignAndCreateRoundCommand(
                    id, dto.StartDate, dto.EndDate, dto.Rounds, dto.RoundDates);
                var result = await mediator.Send(command);
                if (!result.Data)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in UpdateAndCreateRoundAsync for id {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update rejected campaign and resubmit for approval (Senior only).
        /// </summary>
        /// <param name="id">Campaign id</param>
        /// <param name="dto">Updated campaign data</param>
        /// <returns>Response with update status.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/v1/campaigns/1/resubmit
        ///     {
        ///         "startDate": "2025-01-01T00:00:00Z",
        ///         "endDate": "2025-12-31T23:59:59Z",
        ///         "rounds": [
        ///             {
        ///                 "roundName": "Round 1 - Updated",
        ///                 "description": "First recruitment round (revised)",
        ///                 "targetQuantity": 65,
        ///                 "roundStartDate": "2025-01-01T00:00:00Z",
        ///                 "roundEndDate": "2025-06-30T23:59:59Z"
        ///             },
        ///             {
        ///                 "roundName": "Round 2 - Updated", 
        ///                 "description": "Second recruitment round (revised)",
        ///                 "targetQuantity": 35,
        ///                 "roundStartDate": "2025-07-01T00:00:00Z",
        ///                 "roundEndDate": "2025-12-31T23:59:59Z"
        ///             }
        ///         ]
        ///     }
        ///
        /// Notes:
        /// - Campaign must be in Rejected status
        /// - ⚠️ REJECTION LIMIT: Campaigns with 3+ rejections are automatically CANCELED and cannot be resubmitted
        /// - All old rounds will be deleted and replaced with new ones
        /// - Same validation rules apply for targetQuantity
        /// - After success, campaign status changes to Pending for re-approval
        /// </remarks>
        [HttpPut("{id}/resubmit")]
        [ProducesResponseType(typeof(Response<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateRejectedCampaignAsync(int id, [FromBody] CabinCrewCareer.Application.Campaigns.Commands.UpdateRejectedCampaign.UpdateRejectedCampaignDto dto)
        {
            try
            {
                var command = new CabinCrewCareer.Application.Campaigns.Commands.UpdateRejectedCampaign.UpdateRejectedCampaignCommand(
                    id, dto.StartDate, dto.EndDate, dto.Rounds);
                var result = await mediator.Send(command);
                if (!result.Data)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in UpdateRejectedCampaignAsync for id {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update campaign status - approve or reject (Director only).
        /// </summary>
        /// <param name="id">Campaign id</param>
        /// <param name="dto">Status and reject reason</param>
        /// <returns>Response with status update result.</returns>
        /// <remarks>
        /// Sample request for APPROVE:
        ///
        ///     PUT /api/v1/campaigns/1/status
        ///     {
        ///         "status": 2
        ///     }
        ///
        /// Sample request for REJECT:
        ///
        ///     PUT /api/v1/campaigns/1/status
        ///     {
        ///         "status": 3,
        ///         "rejectReason": "Please adjust targetQuantity for the rounds"
        ///     }
        ///
        /// Status values:
        /// - 2 = Approved
        /// - 3 = Rejected
        ///
        /// Notes:
        /// - Campaign must be in Pending status
        /// - rejectReason is required when status = 3
        /// - After approve: campaign status = Approved
        /// - After reject: campaign status = Rejected (Senior can resubmit)
        /// - ⚠️ REJECTION LIMIT: After 3 rejections, campaign becomes CANCELED automatically and cannot be resubmitted
        /// - ReviewedBy field will record the username (not ID) of the Director who approved/rejected
        /// - RejectedCount tracks the number of times this campaign has been rejected
        /// </remarks>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(Response<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateCampaignStatusAsync(int id, [FromBody] CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignStatus.UpdateCampaignStatusDto dto)
        {
            try
            {
                var command = new CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignStatus.UpdateCampaignStatusCommand(id, dto.Status, dto.RejectReason);
                var result = await mediator.Send(command);
                if (!result.Data)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in UpdateCampaignStatusAsync for id {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
