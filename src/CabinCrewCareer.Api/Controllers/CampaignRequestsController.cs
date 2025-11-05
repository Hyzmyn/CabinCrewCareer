using Asp.Versioning;
using CabinCrewCareer.Application.CampaignRequests.Commands.CreateCampaignRequest;
using CabinCrewCareer.Application.CampaignRequests.Commands.UpdateCampaignRequestStatus;
using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/campaign-requests")]
    [ApiVersion(1)]
    [Authorize]
    public class CampaignRequestsController(
        IMediator mediator,
        ILogger<CampaignRequestsController> logger) : ApiController(mediator, logger)
    {
        #region Create Campaign Request
        /// <summary>
        /// Partner create a campaign request
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "campaignName": "Tuyển dụng tiếp viên hàng không 2024",
        ///       "description": "Tuyển dụng 50 tiếp viên cho các chuyến bay nội địa và quốc tế",
        ///       "targetQuantity": 50
        ///     }
        ///         
        /// </remarks> 
        /// <returns>Created campaign request</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [ProducesResponseType(typeof(Response<GetCampaignRequestResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateCampaignRequestDto request)
        {
            try
            {
                var command = new CreateCampaignRequestCommand(
                    request.CampaignName,
                    request.Description,
                    request.TargetQuantity);
                    
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

        #region Update Campaign Request Status
        /// <summary>
        /// Director approve or reject a campaign request
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="request">Request body</param>
        /// <remarks>
        /// Sample request for Approve:
        /// 
        ///     {
        ///       "status": 2
        ///     }
        ///     
        /// Sample request for Reject:
        /// 
        ///     {
        ///       "status": 3,
        ///       "rejectReason": "Không đủ nguồn lực để thực hiện chiến dịch này"
        ///     }
        ///         
        /// Status: 2 = Approved, 3 = Rejected
        /// </remarks> 
        /// <returns>Updated campaign request</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Response<GetCampaignRequestResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateStatusAsync(int id, [FromBody] UpdateCampaignRequestStatusDto request)
        {
            try
            {
                var command = new UpdateCampaignRequestStatusCommand(
                    id,
                    request.Status,
                    request.RejectReason);
                    
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
