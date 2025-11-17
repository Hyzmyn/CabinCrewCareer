using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.TestQuestions.Commands.CreateTestQuestion;
using CabinCrewCareer.Application.TestQuestions.Commands.CreateBulkTestQuestions;
using CabinCrewCareer.Application.TestQuestions.Commands.ImportQuestionsFromExcel;
using CabinCrewCareer.Application.TestQuestions.Queries.GetTestQuestions;
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

        #region Import Questions From Excel
        /// <summary>
        /// Examiner imports questions from Excel file
        /// </summary>
        /// <remarks>
        /// **Import multiple questions from Excel file**
        /// 
        /// Excel file format (starting from row 2, row 1 is header):
        /// - **Column A (1)**: OrderNumber - Question order number
        /// - **Column B (2)**: QuestionContent - Question text content
        /// - **Column C (3)**: Score - Points for this question (1-100)
        /// - **Column D-I (4-9)**: Options - Answer options (leave empty if not needed)
        /// - **Column J (10)**: CorrectAnswer - Text of correct answer (fallback if bold detection fails)
        /// 
        /// **Option detection:**
        /// 1. **BOLD text** in columns D-I will be marked as correct answer (IsCorrect = true)
        /// 2. If no bold option found, use Column J to match correct answer text
        /// 3. Empty option columns will be skipped (supports 3-6 options)
        /// 
        /// **TestType requirements:**
        /// - **EnglishListening/Practical**: 4-6 options required, exactly 1 must be correct
        /// - **EnglishSpeaking**: No options allowed
        /// 
        /// **File constraints:**
        /// - Format: .xlsx or .xls only
        /// - Max size: 10 MB
        /// - Processes all rows until empty OrderNumber found
        /// 
        /// **Example Excel structure:**
        /// | OrderNumber | QuestionContent | Score | Option1 | Option2 | Option3 | Option4 | Option5 | Option6 | CorrectAnswer |
        /// |-------------|-----------------|-------|---------|---------|---------|---------|---------|---------|---------------|
        /// | 1           | What is 2+2?    | 10    | **4**   | 3       | 5       | 6       |         |         | 4             |
        /// | 2           | Capital of VN?  | 10    | HCMC    | **Hanoi**| Danang  | Hue     |         |         | Hanoi         |
        /// 
        /// Note: Only Examiner role can import questions.
        /// </remarks>
        /// <param name="testId">Test ID to import questions for</param>
        /// <param name="excelFile">Excel file to import (.xlsx or .xls)</param>
        /// <response code="200">Ok - Questions imported successfully</response>
        /// <response code="400">Bad Request - Validation errors or invalid file format</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Not an Examiner</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("import-excel")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Response<CreateBulkTestQuestionsResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ImportQuestionsFromExcelAsync(int testId, IFormFile excelFile)
        {
            try
            {
                var command = new ImportQuestionsFromExcelCommand(testId, excelFile);
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

        #region Get Test Questions
        /// <summary>
        /// Get all questions for a specific test
        /// </summary>
        /// <param name="testId">Test ID to get questions for</param>
        /// <remarks>
        /// **Retrieve all questions and options for a test**
        /// 
        /// **Logic based on TestType:**
        /// 
        /// 1. **EnglishListening (1)** and **Practical (3)**:
        ///    - Returns questions WITH options
        ///    - Each question includes list of answer options
        ///    - Options contain OptionId, OptionContent, and IsCorrect flag
        /// 
        /// 2. **EnglishSpeaking (2)**:
        ///    - Returns questions WITHOUT options (Options = null)
        ///    - These are open-ended questions that don't require multiple choice answers
        /// 
        /// **Response includes:**
        /// - Test information (ID, Name, Type)
        /// - Total number of questions
        /// - List of questions ordered by OrderNumber
        /// - For each question: QuestionId, QuestionContent, Score, OrderNumber
        /// - Options (only for Listening/Practical tests)
        /// 
        /// **Use cases:**
        /// - Display test questions to test takers
        /// - Review test content for examiners
        /// - Test preview before starting
        /// 
        /// Note: This endpoint can be accessed by all authenticated users.
        /// </remarks>
        /// <returns>List of questions with or without options based on test type</returns>
        /// <response code="200">Ok - Questions retrieved successfully</response>
        /// <response code="400">Bad Request - Invalid test ID</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="404">Not Found - Test not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("test/{testId}")]
        [ProducesResponseType(typeof(Response<GetTestQuestionsResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTestQuestionsAsync(int testId)
        {
            try
            {
                var query = new GetTestQuestionsQuery(testId);
                return Ok(await mediator.Send(query));
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
