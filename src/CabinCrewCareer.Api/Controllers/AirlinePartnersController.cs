using Asp.Versioning;
using CabinCrewCareer.Application.AirlinePartners.Commands.CreateAirlinePartner;
using CabinCrewCareer.Application.AirlinePartners.Queries.GetAirlinePartners;
using CabinCrewCareer.Application.Cities.Common;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/airline-partners")]
    [ApiVersion(1)]
    [Authorize]
    public class AirlinePartnersController(
        IMediator mediator,
        ILogger<AirlinePartnersController> logger) : ApiController(mediator, logger)
    {
        #region Get Airline Partners
        /// <summary>
        /// Get all airline partners
        /// </summary>
        /// <returns>A list of all airline partners</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<List<GetCityResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await mediator.Send(new GetAirlinePartnersQuery()));
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

        #region Create Airline Partner
        /// <summary>
        /// Create a new airline partner
        /// </summary>
        /// Sample request:
        /// 
        ///     {
        ///       "partnerName": "Vietravel Airlines",
        ///       "description": "A nice airline company",     
        ///       "address": "Your address",
        ///       "wardId": 1
        ///     }
        ///  
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync([FromBody] CreateAirlinePartnerCommand request)
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
    }
}
