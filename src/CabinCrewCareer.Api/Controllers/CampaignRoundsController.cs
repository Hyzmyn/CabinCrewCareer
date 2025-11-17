using Asp.Versioning;
using CabinCrewCareer.Application.CampaignRounds.Common;
using CabinCrewCareer.Application.CampaignRounds.Queries.GetCampaignRoundById;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/campaign-rounds")]
    [ApiVersion(1)]
    [Authorize]
    public class CampaignRoundsController(
        IMediator mediator,
        ILogger<CampaignRoundsController> logger) : ApiController(mediator, logger)
    {
        #region Get campaign round
        /// <summary>
        /// Get campaign round detail
        /// </summary>
        /// <param name="id">Id of a specific campaign round</param>
        /// <returns>Campaign Round detail</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<GetCampaignRoundResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                return Ok(await mediator.Send(new GetCampaignRoundByIdQuery(id)));
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

        //#region Get rounds of a campaign round
        ///// <summary>
        ///// Get all rounds of a specific campaign rounds
        ///// </summary>
        ///// <param name="id">Id of a specific campaign round</param>
        ///// <returns>List of rounds</returns>
        ///// <response code="200">Ok</response>
        ///// <response code="400">Bad Request</response>
        ///// <response code="401">Unauthorized</response>
        ///// <response code="403">Forbidden</response>
        ///// <response code="404">Not Found</response>
        ///// <response code="500">Internal Server Error</response>
        //[HttpGet("{id}/rounds")]
        //[ProducesResponseType(typeof(Response<List<GetRoundResponse>>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //public async Task<IActionResult> GetRoundsByIdAsync(int id)
        //{
        //    try
        //    {
        //        return Ok(await mediator.Send(new GetRoundsByCampaignRoundIdQuery(id)));
        //    }
        //    catch (ApiException e)
        //    {
        //        return NotFound(new
        //        {
        //            errorCode = e.ErrorCode,
        //            error = e.Error,
        //            errorMessage = e.ErrorMessage
        //        });
        //    }
        //}
        //#endregion
    }
}
