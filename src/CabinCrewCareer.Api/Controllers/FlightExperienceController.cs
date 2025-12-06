using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.UserApplications.Commands;
using CabinCrewCareer.Application.UserApplications.Commands.Requests;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/flight-experience")]
    [ApiVersion(1)]
    [Authorize]
    public class FlightExperienceController(
        IMediator mediator,
        ILogger<FlightExperienceController> logger) : ApiController(mediator, logger)
    {
        #region Update Flight Experience
        /// <summary>
        /// Update flight hours and experience for cabin crew application snapshot
        /// </summary>
        /// <param name="applicationSnapshotId">Application Snapshot ID</param>
        /// <param name="request">Request body with TotalFlightHours and Experience</param>
        /// <remarks>
        /// **Update flight experience information for a cabin crew application**
        /// 
        /// This API is used in the flight experience verification flow to record:
        /// - Total flight hours accumulated by the cabin crew applicant
        /// - Previous work experience description
        /// 
        /// **Request Body:**
        /// ```json
        /// {
        ///   "totalFlightHours": 1500,
        ///   "experience": "5 years of experience with domestic and international flights. Previously worked with Vietnam Airlines on Boeing 787 and Airbus A350 aircraft."
        /// }
        /// ```
        /// 
        /// **Validation Rules:**
        /// - `applicationSnapshotId`: Must be greater than 0
        /// - `totalFlightHours`: Must be greater than or equal to 0
        /// - `experience`: Optional, maximum 500 characters
        /// 
        /// **Use Cases:**
        /// - Record flight hours for experienced cabin crew applicants
        /// - Document previous airline work experience
        /// - Track total flight time for qualification assessment
        /// - Support promotion requests for existing cabin crew
        /// 
        /// **Example Scenarios:**
        /// 
        /// 1. **New Applicant (No Experience):**
        /// ```json
        /// {
        ///   "totalFlightHours": 0,
        ///   "experience": null
        /// }
        /// ```
        /// 
        /// 2. **Experienced Applicant:**
        /// ```json
        /// {
        ///   "totalFlightHours": 3200,
        ///   "experience": "8 years with Emirates Airlines, specialized in long-haul international flights"
        /// }
        /// ```
        /// 
        /// 3. **Promotion Request:**
        /// ```json
        /// {
        ///   "totalFlightHours": 5000,
        ///   "experience": "10 years with Vietnam Airlines, served as Senior Flight Attendant on international routes"
        /// }
        /// ```
        /// </remarks>
        /// <returns>Success message with updated application snapshot ID</returns>
        /// <response code="200">Ok - Flight experience updated successfully</response>
        /// <response code="400">Bad Request - Validation errors or application snapshot not found</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - Insufficient permissions</response>
        [HttpPut("{applicationSnapshotId}")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> UpdateFlightExperienceAsync(int applicationSnapshotId, [FromBody] UpdateFlightExperienceRequest request)
        {
            try
            {
                var command = new UpdateFlightExperienceCommand(
                    applicationSnapshotId,
                    request.TotalFlightHours,
                    request.Experience);

                return Ok(await mediator.Send(command));
            }
            catch (CabinCrewCareer.Application.Common.Exceptions.ValidationException e)
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
