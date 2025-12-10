using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.ScoringCriterias.Commands.CreateScoringCriteria;
using CabinCrewCareer.Application.ScoringCriterias.Commands.DeleteScoringCriteria;
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

        #region Create Scoring Criteria
        /// <summary>
        /// Admin create a requirement
        /// </summary>
        /// <param name="id">Id of a specific criteria</param>
        /// <param name="request">Request body</param>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "itemType": "Appearance",
        ///       "text": "Hello",
        ///       "englishText": "Hello",
        ///       "details": [
        ///         {
        ///           "detailText": "Nice"
        ///         }
        ///       ]
        ///     }
        ///  
        /// </remarks> 
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("{id}/scoring-criteria-items")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync(int id, [FromBody] CreateScoringCriteriaRequest request)
        {
            try
            {
                return Ok(await mediator.Send(new CreateScoringCriteriaCommand(id, request)));
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

        //#region Update Interview Criteria
        ///// <summary>
        ///// Update requirement
        ///// </summary>
        ///// <param name="id">Id of a specific criteria</param>
        ///// <param name="itemId">Id of a specific criteria item</param>
        ///// <param name="criteria">Request body</param>
        ///// <remarks>
        ///// Sample request:
        ///// 
        /////     {
        /////       "criteria": "Eyes"
        /////     }
        /////         
        ///// </remarks> 
        ///// <returns>Successful code</returns>
        ///// <response code="200">Ok</response>
        ///// <response code="400">Bad Request</response>
        ///// <response code="401">Authenticated</response>
        ///// <response code="403">Forbidden</response>
        ///// <response code="404">Not Found</response>
        ///// <response code="500">Internal Server Error</response>
        //[HttpPut("{id}/scoring-criteria-items/{itemId}")]
        //[ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //public async Task<IActionResult> UpdateAsync(int id, int itemId, [FromBody] string criteria)
        //{
        //    try
        //    {
        //        return Ok(await mediator.Send(new UpdateInterviewCriteriaCommand(id, itemId, criteria)));
        //    }
        //    catch (ValidationException e)
        //    {
        //        return NotFound(new
        //        {
        //            errorCode = e.ErrorCode,
        //            errorMessage = e.ErrorMessage,
        //            errors = e.Errors
        //        });
        //    }
        //}
        //#endregion

        #region Delete Scoring Criteria
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
        [HttpDelete("{id}/scoring-criteria-items/{itemId}")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteAsync(int id, int itemId)
        {
            try
            {
                return Ok(await mediator.Send(new DeleteScoringCriteriaCommand(id, itemId)));
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
