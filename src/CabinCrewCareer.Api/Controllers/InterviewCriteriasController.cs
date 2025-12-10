using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.InterviewCriterias.Commands.CreateInterviewCriteria;
using CabinCrewCareer.Application.InterviewCriterias.Commands.DeleteInterviewCriteria;
using CabinCrewCareer.Application.InterviewCriterias.Commands.UpdateInterviewCriteria;
using CabinCrewCareer.Application.InterviewCriterias.Common;
using CabinCrewCareer.Application.InterviewCriterias.Queries.GetInterviewCriterias;
using CabinCrewCareer.Domain.Common;
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
        #region Get Interview Criterias For Recruitment
        /// <summary>
        /// Get interview criterias for recruitment
        /// </summary>
        /// <returns>A list of all interview criterias</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("recruitment")]
        [ProducesResponseType(typeof(Response<List<GetInterviewCriteriaResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                return Ok(await mediator.Send(new GetInterviewCriteriasQuery(CampaignType.Recruitment)));
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

        #region Get Interview Criterias For Promotion
        /// <summary>
        /// Get interview criterias for promotion
        /// </summary>
        /// <returns>A list of all interview criterias</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("promotion")]
        [ProducesResponseType(typeof(Response<List<GetInterviewCriteriaResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCriteriasAsync()
        {
            try
            {
                return Ok(await mediator.Send(new GetInterviewCriteriasQuery(CampaignType.Promotion)));
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
        /// <param name="id">Id of a specific criteria</param>
        /// <param name="criteria">Request body</param>
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "criteria": "Appearance"
        ///     }
        ///  
        /// </remarks> 
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("{id}/interview-criteria-items")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync(int id, [FromBody] string criteria)
        {
            try
            {
                return Ok(await mediator.Send(new CreateInterviewCriteriaCommand(criteria, id)));
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

        #region Update Interview Criteria
        /// <summary>
        /// Update requirement
        /// </summary>
        /// <param name="id">Id of a specific criteria</param>
        /// <param name="itemId">Id of a specific criteria item</param>
        /// <param name="criteria">Request body</param>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "criteria": "Eyes"
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
        [HttpPut("{id}/interview-criteria-items/{itemId}")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateAsync(int id, int itemId, [FromBody] string criteria)
        {
            try
            {
                return Ok(await mediator.Send(new UpdateInterviewCriteriaCommand(id, itemId, criteria)));
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

        #region Delete Interview Criteria
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
        [HttpDelete("{id}/interview-criteria-items/{itemId}")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteAsync(int id, int itemId)
        {
            try
            {
                return Ok(await mediator.Send(new DeleteInterviewCriteriaCommand(id, itemId)));
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
