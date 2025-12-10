using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.RoundTypes.Commands.CreateRoundType;
using CabinCrewCareer.Application.RoundTypes.Commands.DeleteRoundType;
using CabinCrewCareer.Application.RoundTypes.Commands.UpdateRoundType;
using CabinCrewCareer.Application.RoundTypes.Common;
using CabinCrewCareer.Application.RoundTypes.Queries.GetRoundTypesByType;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/round-types")]
    [ApiVersion(1)]
    [Authorize]
    public class RoundTypesController(
        IMediator mediator,
        ILogger<RoundTypesController> logger) : ApiController(mediator, logger)
    {
        #region Get Round Types
        /// <summary>
        /// Get all round types by campaign type
        /// </summary>
        /// <returns>A list of all round types</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<List<GetRoundTypeResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync([FromQuery] CampaignType type)
        {
            try
            {
                return Ok(await mediator.Send(new GetRoundTypesByTypeQuery(type)));
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

        #region Create Round Type
        /// <summary>
        /// Admin create a round type
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "roundTypeName": "New round",
        ///       "campaignType": 1
        ///     }
        ///  
        /// </remarks> 
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync([FromBody] CreateRoundTypeCommand request)
        {
            try
            {
                return Ok(await mediator.Send(request));
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

        #region Update Round Type
        /// <summary>
        /// Update round type
        /// </summary>
        /// <param name="id">Id of a specific round type</param>
        /// <param name="request">Request body</param>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "roundTypeName": "Updated round"
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
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Response<GetUserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] string request)
        {
            try
            {
                return Ok(await mediator.Send(new UpdateRoundTypeCommand(id, request)));
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

        #region Delete Round Type
        /// <summary>
        /// Delete a round type
        /// </summary>
        /// <param name="id">Id of a specific round type</param>
        /// <returns>Sucessful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                return Ok(await mediator.Send(new DeleteRoundTypeCommand(id)));
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
