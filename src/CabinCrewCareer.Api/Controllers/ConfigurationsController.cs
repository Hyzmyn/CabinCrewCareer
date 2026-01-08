using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Configurations.Commands.CreateConfiguration;
using CabinCrewCareer.Application.Configurations.Common;
using CabinCrewCareer.Application.Configurations.Queries.GetConfigurations;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/configurations")]
    [ApiVersion(1)]
    [Authorize]
    public class ConfigurationsController(
        IMediator mediator,
        ILogger<ConfigurationsController> logger) : ApiController(mediator, logger)
    {
        #region Get Configurations
        /// <summary>
        /// Get all configurations
        /// </summary>
        /// <param name="sortColumn">Sort on a specifice column. Example: "date"</param>
        /// <param name="sortOrder">"asc" or "desc"</param>
        /// <param name="campaignType">1: Recruitment, 2: Promotion</param>
        /// <param name="roundTypeId">Round type Id</param>
        /// <param name="page">Current page</param>
        /// <param name="pageSize">Number of records of a specific page</param>
        /// <returns>A list of all configurations</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<List<GetConfigurationResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync(
            string? sortColumn,
            string? sortOrder,
            int? campaignType,
            int? roundTypeId,
            int page = 1,
            int pageSize = 5)
        {
            try
            {
                return Ok(await mediator.Send(new GetConfigurationsQuery(sortColumn, sortOrder, campaignType, roundTypeId, page, pageSize)));
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

        //#region Get Configuration
        ///// <summary>
        ///// Get configuration
        ///// </summary>
        ///// <param name="id">Id of a specific configuration</param>
        ///// <returns>configuration</returns>
        ///// <response code="200">Ok</response>
        ///// <response code="400">Bad Request</response>
        ///// <response code="401">Unauthorized</response>
        ///// <response code="403">Forbidden</response>
        ///// <response code="404">Not Found</response>
        ///// <response code="500">Internal Server Error</response>
        //[HttpGet("{id}")]
        //[ProducesResponseType(typeof(Response<GetConfigurationResponse>), (int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //public async Task<IActionResult> GetByIdAsync(int id)
        //{
        //    try
        //    {
        //        return Ok(await mediator.Send(new GetConfigurationByIdQuery(id)));
        //    }
        //    catch (ApiException e)
        //    {
        //        return NotFound(new
        //        {
        //            errorCode = e.ErrorCode,
        //            error = e.Error,
        //            errorMessage = e.ErrorMessage
        //        });
        //    }
        //}
        //#endregion

        #region Create configuration
        /// <summary>
        /// Admin create a configuration
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "roundTypeId": 1,     
        ///       "benchmark": 80
        ///     }
        ///  
        /// </remarks> 
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<GetUserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync([FromBody] CreateConfigurationCommand request)
        {
            try
            {
                return Ok(await mediator.Send(request));
            }
            catch (ApiException e)
            {
                return BadRequest(new
                {
                    errorCode = e.ErrorCode,
                    errorMessage = e.ErrorMessage,
                    error = e.Error
                });
            }
        }
        #endregion
    }
}
