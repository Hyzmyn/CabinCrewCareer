using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Roles.Common;
using CabinCrewCareer.Application.Roles.Queries.GetUsersByRoleId;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/roles")]
    [ApiVersion(1)]
    [Authorize]
    public class RolesController(
        IMediator mediator,
        ILogger<RolesController> logger) : ApiController(mediator, logger)
    {
        #region Get Users By Role Id
        /// <summary>
        /// Get all users of a specific role
        /// </summary>
        /// <param name="id">Id of a specific role</param>
        /// <returns>A list of all users of a specific role</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{id}/users")]
        [ProducesResponseType(typeof(Response<List<GetRoleResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int? id)
        {
            try
            {
                return Ok(await mediator.Send(new GetUsersByRoleIdQuery(id)));
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
