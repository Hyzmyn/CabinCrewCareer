using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.EvaluationForms.Commands.CreateAppearanceEvaluationForm;
using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Application.EvaluationForms.Queries.GetAppearanceEvaluationFormByActivityId;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/appearance-results")]
    [ApiVersion(1)]
    [Authorize]
    public class AppearanceResultsController(
        IMediator mediator,
        ILogger<AppearanceResultsController> logger) : ApiController(mediator, logger)
    {
        #region Create Appearance Result Form
        /// <summary>
        /// Examiner creates an Appearance Result Form
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "activityId": 2,
        ///       "choices": [
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 1
        ///         },
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 2
        ///         },
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 3
        ///         },
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 4
        ///         },
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 5
        ///         },
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 6
        ///         },
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 7
        ///         },
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 8
        ///         },
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 9
        ///         },
        ///         {
        ///           "isPassed": true,
        ///           "scoringCriteriaItemId": 10
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
        [HttpPost]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync([FromBody] CreateAppearanceEvaluationFormCommand request)
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

        #region Get An Appearance Result Form
        /// <summary>
        /// Get an appearance result by round activity Id
        /// </summary>
        /// <param name="activityId">Id of a round activity</param>
        /// <returns>Appearance result form</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{activityId}")]
        [ProducesResponseType(typeof(Response<GetAppearanceEvaluationFormResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByIdAsync(int activityId)
        {
            try
            {
                return Ok(await mediator.Send(new GetAppearanceEvaluationFormByActivityIdQuery(activityId)));
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
