using Asp.Versioning;
using CabinCrewCareer.Application.CampaignAssignments.Commands.AssignRecruitersAndExaminers;
using CabinCrewCareer.Application.CampaignAssignments.Commands.UpdateTaskStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/campaign-assignments")]
    [ApiVersion(1)]
    [Authorize]
    public class CampaignAssignmentsController(
        IMediator mediator,
        ILogger<CampaignAssignmentsController> logger) : ApiController(mediator, logger)
    {
        /// <summary>
        /// Assign recruiters and examiners to a campaign (Senior only)
        /// </summary>
        /// <param name="command">Assignment details</param>
        /// <returns>Assignment result</returns>
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRecruitersAndExaminersAsync([FromBody] AssignRecruitersAndExaminersCommand command)
        {
            try
            {
                var result = await mediator.Send(command);
                if (result.Code == 200)
                {
                    return Ok(result);
                }
                else if (result.Code == 404)
                {
                    return NotFound(result);
                }
                else if (result.Code == 400)
                {
                    return BadRequest(result);
                }
                else if (result.Code == 401)
                {
                    return Unauthorized(result);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AssignRecruitersAndExaminersAsync");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update task status (Recruiter/Examiner only)
        /// </summary>
        /// <param name="assignmentId">Assignment ID</param>
        /// <param name="command">Status update details</param>
        /// <returns>Update result</returns>
        [HttpPut("tasks/{assignmentId}/status")]
        public async Task<IActionResult> UpdateTaskStatusAsync(int assignmentId, [FromBody] UpdateTaskStatusCommand command)
        {
            try
            {
                command.AssignmentId = assignmentId;
                var result = await mediator.Send(command);
                
                if (result.Code == 200)
                {
                    return Ok(result);
                }
                else if (result.Code == 404)
                {
                    return NotFound(result);
                }
                else if (result.Code == 400)
                {
                    return BadRequest(result);
                }
                else if (result.Code == 401)
                {
                    return Unauthorized(result);
                }
                else if (result.Code == 403)
                {
                    return StatusCode(403, result);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UpdateTaskStatusAsync");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}