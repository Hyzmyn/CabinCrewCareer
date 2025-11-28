using Asp.Versioning;
using CabinCrewCareer.Api.Models;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.EnquiryRequests.Commands.CreateEnquiryRequest;
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
    }
}
