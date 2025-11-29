using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.RoundActivities.Commands.MoveToInterviewRound;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/round-activities")]
    [ApiVersion(1)]
    [Authorize]
    public class RoundActivitiesController(
        IMediator mediator,
        ILogger<RoundActivitiesController> logger) : ApiController(mediator, logger)
    {
        #region Move To Interview Round
        /// <summary>
        /// Move all candidates from test round to interview round after validating test results
        /// </summary>
        /// <param name="request">Request containing RoundId</param>
        /// <remarks>
        /// **Batch move all candidates from test round to interview round**
        /// 
        /// **Security:**
        /// - Requires authentication
        /// 
        /// **Process:**
        /// 1. Validates the round is a test round (Listening/Speaking/Practical)
        /// 2. Gets all RoundActivities with status Ongoing in this round
        /// 3. For each candidate (RoundActivity):
        ///    - Gets all their test sessions for this test
        ///    - Validates all sessions have Status = true (finalized/scored)
        ///    - Checks if each session's TotalScore >= 80% of Test MaxScore
        /// 
        /// **Test-specific Rules:**
        /// 
        /// **Practical Test:**
        /// - If pass (greater than or equal to 80%): Immediately moves to Interview round
        /// - If fail (less than 80%): Status = Failed
        /// 
        /// **English Test (Listening or Speaking):**
        /// - If pass (greater than or equal to 80%): Marks as Passed, but waits for the OTHER English test
        /// - Only creates Interview RoundActivity when BOTH Listening AND Speaking are Passed
        /// - If either test fails: Status = Failed, no interview round
        /// 
        /// **Use Case:**
        /// - Call this API for Listening Test Round → Users marked Passed, waiting for Speaking
        /// - Call this API for Speaking Test Round → Users who also passed Listening move to Interview
        /// - Call this API for Practical Test Round → Users immediately move to Interview
        /// 
        /// **Example Scenarios:**
        /// 
        /// **Scenario 1: Practical Test**
        /// ```
        /// Input: RoundId = 6 (Practical)
        /// Result: User passes 85/100 → Immediately creates Interview RoundActivity
        /// ```
        /// 
        /// **Scenario 2: Listening Test (First)**
        /// ```
        /// Input: RoundId = 4 (Listening)
        /// Result: User passes 90/100 → Marked as Passed, waiting for Speaking test
        /// ```
        /// 
        /// **Scenario 3: Speaking Test (Second, after Listening passed)**
        /// ```
        /// Input: RoundId = 5 (Speaking)
        /// User has: Listening = Passed, Speaking = 85/100
        /// Result: BOTH passed → Creates Interview RoundActivity
        /// ```
        /// 
        /// **Scenario 4: Speaking Test (Listening failed)**
        /// ```
        /// Input: RoundId = 5 (Speaking)
        /// User has: Listening = Failed, Speaking = 90/100
        /// Result: Speaking marked Passed, but NO interview round (Listening failed)
        /// ```
        /// 
        /// **Example Request:**
        /// ```json
        /// POST /api/v1/round-activities/move-to-interview
        /// {
        ///   "roundId": 5
        /// }
        /// ```
        /// 
        /// **Example Response (English Test):**
        /// ```json
        /// {
        ///   "code": 200,
        ///   "data": "Passed: 10, Failed: 5",
        ///   "message": "Processed 15 round activities in English Test Round (Both Listening and Speaking must pass):\n- Passed: 10\n- Failed: 5\n\nDetails:\nUser 10: PASSED (90/100) - Both English tests passed, moved to Interview\nUser 12: PASSED (85/100) - Waiting for other English test\nUser 15: FAILED (70/100) - Below 80% threshold"
        /// }
        /// ```
        /// </remarks>
        /// <returns>Summary of processed candidates</returns>
        /// <response code="200">Ok - Round activities processed successfully</response>
        /// <response code="400">Bad Request - Validation failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("move-to-interview")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> MoveToInterviewRoundAsync([FromBody] MoveToInterviewRoundCommand request)
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
    }
}
