using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.InterviewCriterias.Common;
using CabinCrewCareer.Application.InterviewCriterias.Queries.GetInterviewCriterias;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/interview-criterias")]
    [ApiVersion(1)]
    [Authorize]
    public class InterviewCriteriasController(
        IMediator mediator,
        ILogger<InterviewCriteriasController> logger) : ApiController(mediator, logger)
    {
        #region Get Interview Criterias
        /// <summary>
        /// Get interview criterias for appearance checking
        /// </summary>
        /// <returns>A list of all interview criterias</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<List<GetInterviewCriteriaResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await mediator.Send(new GetInterviewCriteriasQuery()));
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
