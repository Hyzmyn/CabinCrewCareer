using Asp.Versioning;
using CabinCrewCareer.Application.Cities.Commands.CreateWard;
using CabinCrewCareer.Application.Cities.Common;
using CabinCrewCareer.Application.Cities.Queries.GetCities;
using CabinCrewCareer.Application.Cities.Queries.GetWardsByCityId;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/cities")]
    [ApiVersion(1)]
    [Authorize]
    public class CitiesController(
        IMediator mediator,
        ILogger<CitiesController> logger) : ApiController(mediator, logger)
    {
        #region Create Ward
        /// <summary>
        /// Create a new ward of a specific city
        /// </summary>
        /// <param name="id">Id of a specific city</param>
        /// <param name="wardName">Name of the ward</param>
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("{id}/wards")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync(int id, string wardName)
        {
            try
            {
                return Ok(await mediator.Send(new CreateWardCommand(wardName, id)));
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

        #region Get Cities
        /// <summary>
        /// Get all cities
        /// </summary>
        /// <returns>A list of all cities</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<List<GetCityResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await mediator.Send(new GetCitiesQuery()));
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

        #region Get Wards By City Id
        /// <summary>
        /// Get all wards of a specific city
        /// </summary>
        /// <param name="id">Id of a specific city</param>
        /// <returns>A list of all wards of a specific city</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{id}/wards")]
        [ProducesResponseType(typeof(Response<List<GetWardResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                return Ok(await mediator.Send(new GetWardsByCityIdQuery(id)));
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
