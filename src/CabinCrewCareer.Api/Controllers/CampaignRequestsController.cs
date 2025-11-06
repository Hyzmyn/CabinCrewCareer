using Asp.Versioning;
using CabinCrewCareer.Application.CampaignRequests.Commands.CreateCampaignRequest;
using CabinCrewCareer.Application.CampaignRequests.Commands.UpdateCampaignRequestStatus;
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

        #region Get Campaign Requests List
        /// <summary>
        /// Get list of campaign requests with pagination and filtering
        /// </summary>
        /// <param name="searchTerm">Search by campaign name</param>
        /// <param name="sortColumn">Column to sort by (name, createdat, status)</param>
        /// <param name="sortOrder">Sort order (asc or desc)</param>
        /// <param name="status">Filter by status (1=Pending, 2=Approved, 3=Rejected)</param>
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
    }
}
