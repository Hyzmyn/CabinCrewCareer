using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.UserApplications.Commands;
using CabinCrewCareer.Application.UserApplications.Commands.Requests;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static CabinCrewCareer.Application.UserApplications.Commands.ApproveScreeningProfileCommand;
namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v1/applications")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ApplicationsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("draft")]
        public async Task<IActionResult> CreateDraft([FromForm] CreateUserApplicationRequest request)
        {
            var command = new CreateUserApplicationDraftCommand(request);
            var result = await _mediator.Send(command);
            if (result.Code == 2)
                return Ok(result);
            return BadRequest(result);
        }


        [HttpPost("submit")]
        public async Task<IActionResult> SubmitApplication([FromForm] CreateUserApplicationRequest request)
        {
            var command = new SubmitUserApplicationCommand(request);
            var result = await _mediator.Send(command);
            if (result.Code == 2)
                return Ok(result);
            return BadRequest(result);
        }


        [HttpPut("{id}/submit")]
        public async Task<IActionResult> VerifyApplication(int id, [FromQuery] int? campaignRoundId = null)
        {
            var command = new VerifyUserApplicationCommand(id, campaignRoundId ?? 0);
            var result = await _mediator.Send(command);
            if (result.Code == 2)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDraft(int id, [FromForm] UpdateUserApplicationRequest request)
        {
            var command = new UpdateUserApplicationCommand(id, request);
            var result = await _mediator.Send(command);
            if (result.Code == 4)
                return Ok(result);
            return BadRequest(result);
        }
        /// <summary>
        /// Duyệt hồ sơ vòng sàng lọc (Screening Approval)
        /// </summary>
        /// <param name="activityId">Id của RoundActivity cần duyệt (truyền qua URL)</param>
        /// <param name="dto">Body truyền vào dạng object: { "status": 2 }<br/>Status: 1 = Ongoing, 2 = Passed, 3 = Failed</param>
        /// <returns>Response với mã kết quả và dữ liệu. Trả về 200 nếu thành công, 400 nếu lỗi.</returns>
        [HttpPut("screening-approve/{activityId}")]
        [Authorize]
        public async Task<IActionResult> ApproveScreeningProfile(int activityId, [FromBody] ApproveScreeningProfileDto dto)
        {
            var command = new ApproveScreeningProfileCommand(activityId, dto.Status);
            var result = await _mediator.Send(command);

            if (result == null || result.Code != 4)
                return BadRequest(result);
            return Ok(result);
        }

        #region Get User Application
        /// <summary>
        /// Get user's application with documents
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<CabinCrewCareer.Application.UserApplications.Common.UserApplicationDetailResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUserApplicationWithDocumentsAsync(int id)
        {
            try
            {
                return Ok(await _mediator.Send(new CabinCrewCareer.Application.UserApplications.Queries.GetUserApplicationByUserId.GetUserApplicationByUserIdQuery(id)));
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
