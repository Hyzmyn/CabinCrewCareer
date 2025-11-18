using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.ScoringCriterias.Common;
using CabinCrewCareer.Application.ScoringCriterias.Queries.GetScoringCriterias;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/scoring-criterias")]
    [ApiVersion(1)]
    [Authorize]
    public class ScoringCriteriasController(
        IMediator mediator,
        ILogger<ScoringCriteriasController> logger) : ApiController(mediator, logger)
    {
        #region Get Scoring Criterias
        /// <summary>
        /// Get scoring criterias for appearance checking
        /// </summary>
        /// <returns>A list of all scoring criterias</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<List<GetScoringCriteriaResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await mediator.Send(new GetScoringCriteriasQuery()));
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
    }
}
