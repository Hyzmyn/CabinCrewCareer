using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.EvaluationForms.Commands.CreateInterviewEvaluationForm;
using CabinCrewCareer.Application.EvaluationForms.Common;
using CabinCrewCareer.Application.EvaluationForms.Queries.GetInterviewEvaluationFormById;
using CabinCrewCareer.Application.EvaluationForms.Queries.GetInterviewEvaluationFormsByActivityId;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/interview-results")]
    [ApiVersion(1)]
    [Authorize]
    public class InterviewResultsController(
        IMediator mediator,
        ILogger<InterviewResultsController> logger) : ApiController(mediator, logger)
    {
        #region Create Interview Result Form
        /// <summary>
        /// Examiner creates an Interview Result Form
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// Type = 1: Recruitment, 2: Promotion
        /// 
        ///     {
        ///       "activityId": 5,
        ///       "type": 1,
        ///       "choices": [
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 1
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 2
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 3
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 4
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 5
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 6
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 7
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 8
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 9
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 10
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 11
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 12
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 13
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 14
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 15
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 16
        ///         },
        ///         {
        ///           "score": 10,
        ///           "comment": "Very good",
        ///           "interviewCriteriaItemId": 17
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
        public async Task<IActionResult> LoginAsync([FromBody] CreateInterviewEvaluationFormCommand request)
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

        #region Get 1 to 3 Interview Result Forms
        /// <summary>
        /// Get 1 to 3 Interview Result Forms
        /// </summary>
        /// <param name="activityId">Id of a activity</param>
        /// <returns>List of interview result forms</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<GetInterviewEvaluationFormDetailResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetFormsAsync([FromQuery] int activityId)
        {
            try
            {
                return Ok(await mediator.Send(new GetInterviewEvaluationFormsByActivityIdQuery(activityId)));
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

        #region Get An Interview Result Form
        /// <summary>
        /// Get an interview result by Id
        /// </summary>
        /// <param name="Id">Id of a evaluation form</param>
        /// <returns>Interview result form</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(Response<GetInterviewEvaluationFormDetailResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByIdAsync(int Id)
        {
            try
            {
                return Ok(await mediator.Send(new GetInterviewEvaluationFormByIdQuery(Id)));
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
