using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.FlightExperience.Commands.ImportFlightHours;
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
        ILogger<FlightExperienceController> logger,
        IUserExportService userExportService) : ApiController(mediator, logger)
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

        #region Export Flight Hours Confirmation
        /// <summary>
        /// Export Flight Hours Confirmation Excel file for a specific round
        /// </summary>
        /// <param name="roundId">Round ID of Flight Hours Confirmation round (RoundType = 3)</param>
        /// <remarks>
        /// **Export Excel file for Flight Hours Confirmation verification**
        /// 
        /// This API exports an Excel file containing flight hours and experience information 
        /// for all applicants in the specified round.
        /// 
        /// **Excel File Structure:**
        /// 
        /// **Row 1 (Title):**
        /// - Merged cells A1:I1
        /// - Content: "BẢNG XÁC NHẬN GIỜ BAY VÀ KINH NGHIỆM"
        /// - Bold, 16pt, centered
        /// 
        /// **Row 2 (Headers):**
        /// - Column A: User ID
        /// - Column B: Họ và Tên (Full Name)
        /// - Column C: Số điện thoại (Phone Number)
        /// - Column D: Ngày sinh (Date of Birth)
        /// - Column E: Giới tính (Gender)
        /// - Column F: CCCD (Citizen ID)
        /// - Column G: Kinh nghiệm (Experience)
        /// - Column H: Tổng giờ bay (Total Flight Hours)
        /// - Column I: Pass/Fail (Checkbox - unchecked)
        /// 
        /// **Row 3+ (Data):**
        /// - Each row contains applicant information
        /// - Pass/Fail column shows unchecked checkbox (☐)
        /// - Users can manually check/uncheck after download
        /// 
        /// **Requirements:**
        /// - Round must exist and have RoundType = FlightHoursConfirmation (3)
        /// - Returns Excel file (.xlsx) format
        /// - File name: "FlightHoursConfirmation_{roundId}.xlsx"
        /// 
        /// **Use Cases:**
        /// - Export applicant list for flight hours verification
        /// - Manual review and approval of flight experience
        /// - Print for physical documentation
        /// - Archive for HR records
        /// 
        /// **Example Response:**
        /// - Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
        /// - Binary Excel file download
        /// </remarks>
        /// <returns>Excel file with flight hours confirmation data</returns>
        /// <response code="200">Ok - Excel file generated successfully</response>
        /// <response code="400">Bad Request - Invalid round ID or wrong round type</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="404">Not Found - Round not found</response>
        [HttpGet("export/{roundId}")]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ExportFlightHoursConfirmationAsync(int roundId)
        {
            try
            {
                var fileBytes = await userExportService.ExportFlightHoursConfirmationAsync(roundId);
                var fileName = $"FlightHoursConfirmation_{roundId}_{DateTime.Now:yyyyMMdd}.xlsx";
                
                return File(
                    fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error exporting flight hours confirmation for round {RoundId}", roundId);
                return BadRequest(new
                {
                    errorCode = 400,
                    errorMessage = "Failed to export flight hours confirmation",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Import Flight Hours Confirmation
        /// <summary>
        /// Import Flight Hours Confirmation results from Excel file
        /// </summary>
        /// <param name="roundId">Round ID of Flight Hours Confirmation round</param>
        /// <param name="file">Excel file with Pass/Fail status filled in</param>
        /// <remarks>
        /// **Import Excel file to process flight hours confirmation results**
        /// 
        /// This API processes the Excel file that was previously exported and filled in 
        /// with Pass/Fail decisions for each applicant.
        /// 
        /// **Request:**
        /// - Content-Type: multipart/form-data
        /// - Form field "roundId": Integer (Round ID)
        /// - Form field "file": Excel file (.xlsx or .xls)
        /// 
        /// **Excel File Requirements:**
        /// - Must match the format from Export API
        /// - Row 1: Title (ignored)
        /// - Row 2: Headers (ignored)
        /// - Row 3+: Data rows with User ID in column A and Pass/Fail in column I
        /// 
        /// **Pass/Fail Column (Column I) Accepted Values:**
        /// - "Pass" or "☑" → User passes and moves to next round
        /// - "Fail" → User fails and UserHistory is created
        /// - Empty or other values → Row is skipped
        /// 
        /// **Processing Logic:**
        /// 
        /// **For PASS:**
        /// 1. Update RoundActivity status from Ongoing to Passed
        /// 2. Create new RoundActivity in Practical Test round (if exists)
        /// 3. Applicant moves to next round
        /// 
        /// **For FAIL:**
        /// 1. Update RoundActivity status from Ongoing to Failed
        /// 2. Create UserHistory record with:
        ///    - Type: NotCompleted
        ///    - CampaignRoundId: Current campaign round
        ///    - ParticipatedDate: Current date
        /// 3. Applicant is eliminated from campaign
        /// 
        /// **Validation:**
        /// - Round must exist and be FlightHoursConfirmation type (RoundType = 3)
        /// - Only processes users with Ongoing status
        /// - File must be valid Excel format
        /// - User ID must match existing RoundActivity
        /// 
        /// **Response:**
        /// Returns summary with:
        /// - Total processed count
        /// - Passed count
        /// - Failed count
        /// - Detailed results for each user
        /// 
        /// **Example Usage:**
        /// 1. Call Export API to get Excel file
        /// 2. Open Excel, review flight hours data
        /// 3. Fill in "Pass" or "Fail" in column I for each applicant
        /// 4. Save the Excel file
        /// 5. Call this Import API with the filled Excel file
        /// 6. System automatically processes all results
        /// </remarks>
        /// <returns>Processing summary with pass/fail counts</returns>
        /// <response code="200">Ok - Import processed successfully</response>
        /// <response code="400">Bad Request - Invalid file, round type, or validation errors</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        [HttpPost("import/{roundId}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> ImportFlightHoursConfirmationAsync(int roundId, IFormFile file)
        {
            try
            {
                var command = new ImportFlightHoursCommand
                {
                    RoundId = roundId,
                    File = file
                };

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
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing flight hours confirmation for round {RoundId}", roundId);
                return BadRequest(new
                {
                    errorCode = 400,
                    errorMessage = "Failed to import flight hours confirmation",
                    error = ex.Message
                });
            }
        }
        #endregion
    }
}
