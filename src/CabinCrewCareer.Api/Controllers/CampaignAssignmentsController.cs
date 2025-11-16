using Asp.Versioning;
using CabinCrewCareer.Application.CampaignAssignments.Commands.AssignRecruitersAndExaminers;
using CabinCrewCareer.Application.CampaignAssignments.Commands.CancelAssignment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/campaign-assignments")]
    [ApiVersion(1)]
    [Authorize]
    public class CampaignAssignmentsController(
        IMediator mediator,
        ILogger<CampaignAssignmentsController> logger) : ApiController(mediator, logger)
    {
        /// <summary>
        /// Assign recruiters and examiners to a campaign (Senior Recruiter only)
        /// </summary>
        /// <remarks>
        /// **Chức năng:**
        /// - Giao việc cho recruiters và examiners trong một campaign
        /// - Tự động gửi notification cho tất cả người được giao việc (realtime nếu có SignalR)
        /// - Chuyển campaign status sang Published
        /// - Assignment status ban đầu: Assigned
        /// - Tự động chuyển sang InProgress khi campaign Ongoing
        /// - Tự động chuyển sang Completed khi campaign Ended
        /// 
        /// **Business Rules:**
        /// - Campaign phải ở trạng thái Approved
        /// - Mỗi user chỉ được 1 campaign ongoing tại một thời điểm
        /// - Không được overlap thời gian với campaigns khác
        /// - Không thể từ chối task (tự động assigned)
        /// 
        /// **Sample request:**
        /// 
        ///     POST /api/v1/campaign-assignments/assign
        ///     {
        ///       "campaignId": 1,
        ///       "assignments": [
        ///         {
        ///           "userId": 5,
        ///           "taskType": 1,
        ///           "taskDescription": "Tuyển dụng và sàng lọc 50 ứng viên"
        ///         },
        ///         {
        ///           "userId": 6,
        ///           "taskType": 2,
        ///           "taskDescription": "Kiểm tra và đánh giá ứng viên vòng phỏng vấn"
        ///         }
        ///       ]
        ///     }
        /// 
        /// **TaskType values:**
        /// - 1 = Recruitment (Tuyển dụng)
        /// - 2 = Examination (Kiểm tra)
        /// 
        /// **Response Format:**
        /// ```json
        /// {
        ///   "code": 2,
        ///   "message": "Created successfully",
        ///   "data": true
        /// }
        /// ```
        /// 
        /// **Response Code (field 'code' in response body):**
        /// - **2** (CREATED_SUCCESS): Giao việc thành công
        /// - **24** (CAMPAIGN_NOT_FOUND): Campaign không tồn tại
        /// - **26** (CAMPAIGN_NOT_APPROVED): Campaign không ở trạng thái Approved
        /// - **10** (FAILED_AUTHENTICATION): Không xác định được người giao việc
        /// - **14** (USER_NOT_FOUND): User không tồn tại
        /// - **27** (CAMPAIGN_ALREADY_ASSIGNED): Campaign đã được giao việc rồi
        /// - **28** (USER_HAS_ONGOING_CAMPAIGN): User đang có campaign ongoing
        /// - **29** (USER_HAS_OVERLAPPING_CAMPAIGN): User có campaign overlap thời gian
        /// </remarks>
        /// <param name="command">Assignment details</param>
        /// <returns>Assignment result</returns>
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRecruitersAndExaminersAsync([FromBody] AssignRecruitersAndExaminersCommand command)
        {
            try
            {
                var result = await mediator.Send(command);
                if (!result.Data)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AssignRecruitersAndExaminersAsync");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel assignment for a user (Senior Recruiter/Manager only)
        /// </summary>
        /// <remarks>
        /// **Chức năng:**
        /// - Hủy assignment đã giao cho recruiter/examiner
        /// - Tự động gửi notification cho người bị hủy assignment (realtime nếu có SignalR)
        /// - Lưu lý do hủy và người thực hiện hủy
        /// - Chuyển assignment status sang Cancelled
        /// 
        /// **Business Rules:**
        /// - Chỉ người có quyền (Senior/Manager) mới được cancel
        /// - Không thể cancel assignment đã Completed
        /// - Không thể cancel assignment đã bị Cancelled trước đó
        /// - Phải có lý do cancel (cancelReason)
        /// 
        /// **Use Cases:**
        /// - Nhân viên xin nghỉ phép/nghỉ việc
        /// - Giao việc cho người khác phù hợp hơn
        /// - Thay đổi kế hoạch campaign
        /// - Nhân viên không đủ năng lực
        /// 
        /// **Sample request:**
        /// 
        ///     PUT /api/v1/campaign-assignments/5/cancel
        ///     {
        ///       "cancelReason": "Nhân viên xin nghỉ phép đột xuất 1 tháng"
        ///     }
        /// 
        /// 
        /// **Response Format:**
        /// ```json
        /// {
        ///   "code": 4,
        ///   "message": "Updated successfully",
        ///   "data": true
        /// }
        /// ```
        /// 
        /// **Response Code (field 'code' in response body):**
        /// - **4** (UPDATED_SUCCESS): Hủy assignment thành công
        /// - **10** (FAILED_AUTHENTICATION): Không xác định được người dùng
        /// - **25** (ASSIGNMENT_NOT_FOUND): Assignment không tồn tại
        /// - **30** (CANNOT_CANCEL_COMPLETED_ASSIGNMENT): Không thể hủy assignment đã Completed
        /// - **31** (ASSIGNMENT_ALREADY_CANCELLED): Assignment đã bị hủy rồi
        /// - **24** (CAMPAIGN_NOT_FOUND): Campaign không tồn tại
        /// 
        /// **Notes:**
        /// - Sau khi cancel, có thể giao việc lại cho người khác bằng API assign
        /// - Notification sẽ được gửi realtime (nếu có SignalR) hoặc qua polling
        /// </remarks>
        /// <param name="assignmentId">Assignment ID to cancel</param>
        /// <param name="request">Cancel request body (chỉ chứa cancelReason)</param>
        /// <returns>Cancel result</returns>
        [HttpPut("{assignmentId}/cancel")]
        public async Task<IActionResult> CancelAssignmentAsync(int assignmentId, [FromBody] CancelAssignmentCommand request)
        {
            try
            {
                request.AssignmentId = assignmentId;
                var result = await mediator.Send(request);
                if (!result.Data)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in CancelAssignmentAsync for assignmentId {assignmentId}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}