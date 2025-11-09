using Asp.Versioning;
using CabinCrewCareer.Application.Campaigns.Commands.UpdateCampaignFields;
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
    public class CampaignController(
        IMediator mediator,
        ILogger<CampaignController> logger) : ApiController(mediator, logger)
    {
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

        [HttpPut("{id}/CampaignRound")]
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
                    return BadRequest(result.Message);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in UpdateAndCreateRoundAsync for id {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
