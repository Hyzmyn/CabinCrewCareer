using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.TestQuestions.Commands.CreateTestQuestion;
using CabinCrewCareer.Application.TestQuestions.Commands.CreateBulkTestQuestions;
using CabinCrewCareer.Application.TestQuestions.Common;
using CabinCrewCareer.Domain.Wrappers;
using CabinCrewCareer.Api.Models;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/test-questions")]
    [ApiVersion(1)]
    [Authorize]
    public class TestQuestionsController(
        IMediator mediator,
        ILogger<TestQuestionsController> logger) : ApiController(mediator, logger)
    {
        #region Create Test Question
        /// <summary>
        /// Examiner creates a new question for a test
        /// </summary>
        /// <param name="request">Question creation request</param>
        /// <remarks>
        /// **Create a question for a test**
        /// 
        /// TestType requirements:
        /// - **EnglishListening (1)**: MUST provide Options with exactly one correct answer
        /// - **EnglishSpeaking (2)**: Should NOT provide Options (open-ended question)
        /// - **Practical (3)**: MUST provide Options with exactly one correct answer
        /// 
        /// Options validation:
        /// - Minimum 2 options, maximum 6 options
        /// - Exactly one option must be marked as IsCorrect = true
        /// - Each option content is required (max 500 characters)
        /// 
        /// Score:
        /// - Must be between 1 and 100
        /// - Represents points awarded for correct answer
        /// 
        /// OrderNumber:
        /// - Determines the display order of questions in the test
        /// - Must be greater than 0
        /// 
        /// Note: Only Examiner role can create test questions.
        /// </remarks>
        /// <returns>Created test question with options</returns>
        /// <response code="200">Ok - Question created successfully</response>
        /// <response code="400">Bad Request - Validation errors or test not found</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - Not an Examiner</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [ProducesResponseType(typeof(Response<CreateTestQuestionResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateTestQuestionAsync([FromBody] CreateTestQuestionRequest request)
        {
            try
            {
                var command = new CreateTestQuestionCommand(
                    request.TestId,
                    request.QuestionContent,
                    request.Score,
                    request.OrderNumber,
                    request.Options);

                return Ok(await mediator.Send(command));
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

        #region Create Bulk Test Questions
        /// <summary>
        /// Examiner creates multiple questions for a test at once
        /// </summary>
        /// <param name="request">Bulk questions creation request</param>
        /// <remarks>
        /// **Create multiple questions for a test in one request**
        /// 
        /// TestType requirements:
        /// - **EnglishListening (1)**: Each question MUST have 4-6 options with exactly one correct answer
        /// - **EnglishSpeaking (2)**: Questions should NOT have options (open-ended questions)
        /// - **Practical (3)**: Each question MUST have 4-6 options with exactly one correct answer
        /// 
        /// Options validation (for Listening/Practical):
        /// - **Minimum 4 options, maximum 6 options** (changed from 2-6)
        /// - Exactly one option must be marked as IsCorrect = true
        /// - Each option content is required (max 500 characters)
        /// 
        /// Limits:
        /// - Maximum 50 questions can be created at once
        /// - Each question score: 1-100 points
        /// - OrderNumber must be greater than 0
        /// 
        /// **OrderNumber explanation:**
        /// - Determines the display order of questions (Question 1, 2, 3...)
        /// - Used for sorting questions when displaying to test takers
        /// - Example: orderNumber 1 = First question, orderNumber 2 = Second question
        /// 
        /// Note: Only Examiner role can create test questions.
        /// </remarks>
        /// <returns>List of created questions with their options</returns>
        /// <response code="200">Ok - Questions created successfully</response>
        /// <response code="400">Bad Request - Validation errors or test not found</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - Not an Examiner</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("bulk")]
        [ProducesResponseType(typeof(Response<CreateBulkTestQuestionsResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateBulkTestQuestionsAsync([FromBody] CreateBulkTestQuestionsRequest request)
        {
            try
            {
                var command = new CreateBulkTestQuestionsCommand(
                    request.TestId,
                    request.Questions);

                return Ok(await mediator.Send(command));
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
    }
}
