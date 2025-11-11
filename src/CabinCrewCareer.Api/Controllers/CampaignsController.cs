using Asp.Versioning;
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
    /// <summary>
    /// Get the list of campaigns.
    /// </summary>
    /// <returns>List of campaigns.</returns>
    [HttpGet]
    public async Task<IActionResult> GetListAsync()
        {
            try
            {
                var result = await mediator.Send(new CabinCrewCareer.Application.Campaigns.Queries.GetCampaignList.GetCampaignListQuery());
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetListAsync");
                return StatusCode(500, new { error = ex.Message });
            }
        }

    /// <summary>
    /// Get the detail of a campaign by id.
    /// </summary>
    /// <param name="id">Campaign id</param>
    /// <returns>Campaign detail.</returns>
    [HttpGet("{id}")]
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
    ///         "startDate": "2025-01-01T00:00:00Z",
    ///         "endDate": "2025-12-31T23:59:59Z",
    ///         "rounds": [
    ///             {
    ///                 "roundName": "Round 1",
    ///                 "description": "First recruitment round",
    ///                 "targetQuantity": 60,
    ///                 "roundStartDate": "2025-01-01T00:00:00Z",
    ///                 "roundEndDate": "2025-06-30T23:59:59Z"
    ///             },
    ///             {
    ///                 "roundName": "Round 2", 
    ///                 "description": "Second recruitment round",
    ///                 "targetQuantity": 40,
    ///                 "roundStartDate": "2025-07-01T00:00:00Z",
    ///                 "roundEndDate": "2025-12-31T23:59:59Z"
    ///             }
    ///         ]
    ///     }
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
                    id, dto.StartDate, dto.EndDate, dto.Rounds);
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
