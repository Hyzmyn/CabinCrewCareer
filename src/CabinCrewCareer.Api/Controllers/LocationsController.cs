using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Locations.Commands.EditLocation;
using CabinCrewCareer.Application.Locations.Common;
using CabinCrewCareer.Application.Locations.Queries.GetLocations;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/locations")]
    [ApiVersion(1)]
    [Authorize]
    public class LocationsController(
        IMediator mediator,
        ILogger<LocationsController> logger) : ApiController(mediator, logger)
    {
        #region Get Locations
        /// <summary>
        /// Get all locations
        /// </summary>
        /// <returns>A list of all locations</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<List<GetLocationResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await mediator.Send(new GetLocationsQuery()));
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

        #region Edit location
        /// <summary>
        /// Admin edit a location
        /// <param name="id">Id of a specific location</param>
        /// <param name="request">Request body</param>
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "address": "Happy New Year",
        ///       "wardId": 1
        ///     }
        ///
        /// </remarks> 
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPut("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<GetLocationResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync(int id, [FromBody] EditLocationRequest request)
        {
            try
            {
                return Ok(await mediator.Send(new EditLocationCommand(id, request)));
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
    }
}
