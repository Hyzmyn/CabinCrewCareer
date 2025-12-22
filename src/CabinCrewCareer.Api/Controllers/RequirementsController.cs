using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Requirements.Commands.CreateRequirement;
using CabinCrewCareer.Application.Requirements.Commands.DeleteRequirement;
using CabinCrewCareer.Application.Requirements.Commands.UpdateRequirement;
using CabinCrewCareer.Application.Requirements.Common;
using CabinCrewCareer.Application.Requirements.Queries.GetRequirementsByType;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{

    [Route("api/v{version:apiVersion}/requirements")]
    [ApiVersion(1)]
    [Authorize]
    public class RequirementsController(
        IMediator mediator,
        ILogger<RequirementsController> logger) : ApiController(mediator, logger)
    {
        #region Get Requirements
        /// <summary>
        /// Get all requirements by campaign type
        /// </summary>
        /// <returns>A list of all requirements</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{id}/requirement-items")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<List<GetRequirementResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync(int id)
        {
            try
            {
                return Ok(await mediator.Send(new GetRequirementsByTypeQuery((CampaignType)id)));
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

        #region Create Requirement
        /// <summary>
        /// Admin create a requirement
        /// <param name="id">Id of a specific round type</param>
        /// <param name="request">Request body</param>
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "title": "Appearance",
        ///       "description": "Taller than me"
        ///     }
        ///  
        /// </remarks> 
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("{id}/requirement-items")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync(int id, [FromBody] CreateRequirementRequest request)
        {
            try
            {
                return Ok(await mediator.Send(new CreateRequirementCommand(request.Title, request.Description, id)));
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
        }
        #endregion

        #region Update Requirement
        /// <summary>
        /// Update requirement
        /// </summary>
        /// <param name="id">Id of a specific requirement</param>
        /// <param name="itemId">Id of a specific requirement item</param>
        /// <param name="request">Request body</param>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "Title": "Eyes",
        ///       "description": "New requirement"
        ///     }
        ///         
        /// </remarks> 
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Authenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPut("{id}/requirement-items/{itemId}")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateAsync(int id, int itemId, [FromBody] CreateRequirementRequest request)
        {
            try
            {
                return Ok(await mediator.Send(new UpdateRequirementCommand(id, itemId, request.Title, request.Description)));
            }
            catch (ValidationException e)
            {
                return NotFound(new
                {
                    errorCode = e.ErrorCode,
                    errorMessage = e.ErrorMessage,
                    errors = e.Errors
                });
            }
        }
        #endregion

        #region Delete Requirement
        /// <summary>
        /// Delete a round type
        /// </summary>
        /// <param name="id">Id of a specific requirement</param>
        /// <param name="itemId">Id of a specific requirement item</param>
        /// <returns>Sucessful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("{id}/requirement-items/{itemId}")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteAsync(int id, int itemId)
        {
            try
            {
                return Ok(await mediator.Send(new DeleteRequirementCommand(id, itemId)));
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
    }
}
