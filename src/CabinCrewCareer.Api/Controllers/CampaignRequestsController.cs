using Asp.Versioning;
using CabinCrewCareer.Application.CampaignRequests.Commands.CreateCampaignRequest;
using CabinCrewCareer.Application.CampaignRequests.Commands.UpdateCampaignRequestStatus;
using CabinCrewCareer.Application.CampaignRequests.Commands.UpdateRejectedCampaignRequest;
using CabinCrewCareer.Application.CampaignRequests.Common;
using CabinCrewCareer.Application.CampaignRequests.Queries.GetCampaignRequestDetail;
using CabinCrewCareer.Application.CampaignRequests.Queries.GetCampaignRequests;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Models;
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
        ///       "jobDescription": "",
        ///       "jobRequirement": "",
        ///       "targetQuantity": 50,
        ///       "requestType": 1
        ///     }
        /// 
        /// RequestType: 1 = Recruitment (Tuyển dụng), 2 = Promotion (Thăng bậc)
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
                    request.JobDescription,
                    request.JobRequirement,
                    request.TargetQuantity,
                    request.RequestType);

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
        /// 
        /// ⚠️ REJECTION LIMIT LOGIC:
        /// - Each rejection increments the RejectedCount field
        /// - After 3 rejections (RejectedCount >= 3), the request status automatically becomes CANCELED
        /// - Canceled requests CANNOT be resubmitted using the resubmit endpoint
        /// - ReviewedBy field records the username (not ID) of the Director who made the decision
        /// - ApprovedAt/RejectedAt fields track when the decision was made
        /// - For testing: Try rejecting the same request 3 times to see it become canceled
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

        #region Get Campaign Requests List
        /// <summary>
        /// Get list of campaign requests with pagination and filtering
        /// </summary>
        /// <param name="searchTerm">Search by campaign name</param>
        /// <param name="sortColumn">Column to sort by (name, createdat, status)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="status">Filter by status (1=Pending, 2=Approved, 3=Rejected)</param>
        /// <param name="requestType">Filter by request type (1=Recruitment, 2=Promotion)</param>
        /// <param name="partnerId">Filter by partner ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of campaign requests</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<PaginatedList<GetCampaignRequestResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetListAsync(
            [FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int? status,
            [FromQuery] int? requestType,
            [FromQuery] int? partnerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetCampaignRequestsQuery
                {
                    SearchTerm = searchTerm,
                    SortColumn = sortColumn,
                    SortOrder = sortOrder,
                    Status = status,
                    RequestType = requestType,
                    PartnerId = partnerId,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(await mediator.Send(query));
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

        #region Get Campaign Request Detail
        /// <summary>
        /// Get detail of a specific campaign request by ID
        /// </summary>
        /// <param name="id">Campaign request ID</param>
        /// <returns>Campaign request detail</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<GetCampaignRequestResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetDetailAsync(int id)
        {
            try
            {
                var query = new GetCampaignRequestDetailQuery(id);
                return Ok(await mediator.Send(query));
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

        #region Update Rejected Campaign Request
        /// <summary>
        /// Partner updates a rejected campaign request to resubmit
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="request">Request body</param>
        /// <remarks>
        /// This endpoint allows partners to update and resubmit a campaign request that was rejected.
        /// The request status will be changed back to Pending for re-approval.
        /// 
        /// ⚠️ REJECTION LIMIT RESTRICTION:
        /// - Requests that have been rejected 3 times (RejectedCount >= 3) are automatically CANCELED
        /// - CANCELED requests CANNOT be resubmitted and will return error 400
        /// - Check the RejectedCount field in GET endpoints to see how many times a request was rejected
        /// - Only requests in "Rejected" status (not "Canceled") can be resubmitted
        /// 
        /// Sample request:
        /// 
        ///     {
        ///       "campaignName": "Tuyển dụng tiếp viên hàng không 2024 - Cập nhật",
        ///       "description": "Tuyển dụng 50 tiếp viên cho các chuyến bay nội địa và quốc tế - đã chỉnh sửa theo yêu cầu",
        ///       "JobDescription": "Tuyển dụng 50 tiếp viên cho các chuyến bay nội địa và quốc tế - đã chỉnh sửa theo yêu cầu",
        ///       "jobRequirement": "Tuyển dụng 50 tiếp viên cho các chuyến bay nội địa và quốc tế - đã chỉnh sửa theo yêu cầu",
        ///       "targetQuantity": 50
        ///     }
        ///         
        /// </remarks> 
        /// <returns>Updated campaign request</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request (includes rejection limit exceeded)</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPut("{id}/resubmit")]
        [ProducesResponseType(typeof(Response<GetCampaignRequestResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateRejectedRequestAsync(int id, [FromBody] UpdateRejectedCampaignRequestDto request)
        {
            try
            {
                var command = new UpdateRejectedCampaignRequestCommand(
                    id,
                    request.CampaignName,
                    request.Description,
                    request.JobDescription,
                    request.JobRequirement,
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
    }
}
