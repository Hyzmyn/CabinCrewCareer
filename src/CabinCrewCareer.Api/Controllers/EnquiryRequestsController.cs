using Asp.Versioning;
using CabinCrewCareer.Api.Models;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.EnquiryRequests.Commands.CreateEnquiryRequest;
using CabinCrewCareer.Application.EnquiryRequests.Commands.UpdateEnquiryRequestScore;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/enquiry-requests")]
    [ApiVersion(1)]
    [Authorize]
    public class EnquiryRequestsController(
        IMediator mediator,
        ILogger<EnquiryRequestsController> logger) : ApiController(mediator, logger)
    {
        #region Create Enquiry Request
        /// <summary>
        /// Create a new enquiry request (phúc khảo) for a test session
        /// </summary>
        /// <param name="request">Enquiry request details</param>
        /// <remarks>
        /// **Create an enquiry request for a test session**
        /// 
        /// **Security:**
        /// - Requires authentication
        /// - User can only create enquiry request for their own test session
        /// 
        /// **Request Body:**
        /// - `testSessionId`: The test session ID to request review for
        /// - `reason`: Detailed reason why you want a review (max 250 characters)
        /// - `status`: Status of the request (typically false for pending)
        /// 
        /// **Validation:**
        /// 1. Test session must exist
        /// 2. Test session must belong to the current user
        /// 3. Cannot create duplicate enquiry request for the same test session
        /// 
        /// **Process:**
        /// 1. Creates the enquiry request
        /// 2. Sets TestSession.Status = false (needs review)
        /// 3. Finds all examiners from the round activities
        /// 4. Sends notifications to all examiners with title "Yêu cầu phúc khảo" and the reason as body
        /// 
        /// **Example Request:**
        /// ```json
        /// {
        ///   "testSessionId": 109,
        ///   "reason": "Tôi nghĩ điểm phần speaking của tôi cao hơn. Xin xem xét lại phần phát âm.",
        ///   "status": false
        /// }
        /// ```
        /// 
        /// **Response:**
        /// - Returns the EnquiryId of the created request
        /// - Examiners receive notification immediately
        /// </remarks>
        /// <returns>Enquiry request ID</returns>
        /// <response code="200">Ok - Enquiry request created successfully</response>
        /// <response code="400">Bad Request - Validation failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [ProducesResponseType(typeof(Response<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateEnquiryRequestAsync([FromBody] CreateEnquiryRequestRequest request)
        {
            try
            {
                var command = new CreateEnquiryRequestCommand
                {
                    TestSessionId = request.TestSessionId,
                    Reason = request.Reason,
                    Status = request.Status
                };

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

        #region Update Enquiry Request Score
        /// <summary>
        /// Update scores for an enquiry request (examiner reviews and re-scores)
        /// </summary>
        /// <param name="testSessionId">Test session ID</param>
        /// <param name="request">Answer scores and optional new reason</param>
        /// <remarks>
        /// **Update scores after reviewing enquiry request**
        /// 
        /// **Security:**
        /// - Requires authentication
        /// - Only examiners can access this endpoint
        /// 
        /// **URL Parameter:**
        /// - `testSessionId`: The test session ID (not enquiry request ID)
        /// 
        /// **Request Body:**
        /// - `answerScores`: Dictionary of answer IDs and their new scores
        ///   - Key: Answer ID (int)
        ///   - Value: Object containing:
        ///     - `criteriaScores`: Dictionary of criteria name and score
        ///     - `isCorrect`: Boolean indicating correctness
        /// - `newReason`: Optional comment/feedback from examiner
        /// 
        /// **Process:**
        /// 1. Finds enquiry request by test session ID
        /// 2. Updates each answer's score based on criteria scores
        /// 3. Calculates total new score
        /// 4. Updates EnquiryRequest (NewScore, Status = true, Reason if provided)
        /// 5. Updates TestSession (TotalScore = NewScore, Status = true)
        /// 6. Sends notification to candidate with score comparison:
        ///    - If score increased: "Điểm tăng từ X lên Y"
        ///    - If score decreased: "Điểm giảm từ X xuống Y"
        ///    - If score unchanged: "Điểm giữ nguyên"
        ///    - Includes examiner's new reason if provided
        /// 
        /// **Example URL:**
        /// ```
        /// PUT /api/v1/enquiry-requests/109/score
        /// ```
        /// (where 109 is the test session ID)
        /// 
        /// **Example Request:**
        /// ```json
        /// {
        ///   "answerScores": {
        ///     "1": {
        ///       "criteriaScores": {
        ///         "Pronunciation": 8,
        ///         "Fluency": 9,
        ///         "Grammar": 7
        ///       },
        ///       "isCorrect": true
        ///     },
        ///     "2": {
        ///       "criteriaScores": {
        ///         "Pronunciation": 6,
        ///         "Fluency": 7,
        ///         "Grammar": 8
        ///       },
        ///       "isCorrect": true
        ///     }
        ///   },
        ///   "newReason": "Sau khi xem xét lại, phần phát âm của bạn thực sự tốt hơn đánh giá ban đầu."
        /// }
        /// ```
        /// 
        /// **Response:**
        /// - Success message with new total score
        /// - Candidate receives notification immediately
        /// </remarks>
        /// <returns>Success message</returns>
        /// <response code="200">Ok - Score updated successfully</response>
        /// <response code="400">Bad Request - Validation failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPut("{testSessionId}/score")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> UpdateEnquiryRequestScoreAsync(int testSessionId, [FromBody] UpdateEnquiryRequestScoreRequest request)
        {
            try
            {
                var command = new UpdateEnquiryRequestScoreCommand
                {
                    TestSessionId = testSessionId,
                    AnswerScores = request.AnswerScores.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new Application.EnquiryRequests.Commands.UpdateEnquiryRequestScore.AnswerScoreInfo
                        {
                            CriteriaScores = kvp.Value.CriteriaScores,
                            IsCorrect = kvp.Value.IsCorrect
                        }
                    ),
                    NewReason = request.NewReason
                };

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
