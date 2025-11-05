using Asp.Versioning;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Common.Models;
using CabinCrewCareer.Application.Users.Commands.CreateUser;
using CabinCrewCareer.Application.Users.Commands.DisableUser;
using CabinCrewCareer.Application.Users.Commands.UpdateUser;
using CabinCrewCareer.Application.Users.Commands.UploadProfileImg;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Application.Users.Queries.GetUserById;
using CabinCrewCareer.Application.Users.Queries.GetUsers;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/users")]
    [ApiVersion(1)]
    [Authorize]
    public class UsersController(
        IMediator mediator,
        ILogger<UsersController> logger) : ApiController(mediator, logger)
    {
        #region Create User
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<GetUserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync([FromBody] CreateUserCommand request)
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
                    errors = e.Errors,
                    errorMessage = e.ErrorMessage
                });
            }
        }
        #endregion

        #region Get Users
        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <param name="query.SearchTerm">User active or not</param>
        /// <param name="query.SortColumn">Current page the user is on</param>
        /// <param name="pageSize">Number of entities you want to show</param>
        /// <param name="searchTerm">Search query</param>
        /// <param name="sortColumn">Column you want to sort</param>
        /// <param name="sortOrder">Sort column by ascending or descening</param>
        /// <returns>A list of all users</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<PaginatedList<GetUserResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllAsync([FromQuery] GetUsersQuery query)
        {
            try
            {
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

        #region Get User
        /// <summary>
        /// Get account profile
        /// </summary>
        /// <param name="id">Id of a specific account</param>
        /// <returns>Account profile</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Response<GetUserDetailResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                return Ok(await mediator.Send(new GetUserByIdQuery(id)));
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

        #region Update User
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Response<GetUserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                return Ok(await mediator.Send(new UpdateUserCommand(id, request)));
            }
            catch (ValidationException e)
            {
                return NotFound(new
                {
                    errorCode = e.ErrorCode,
                    errors = e.Errors,
                    errorMessage = e.ErrorMessage
                });
            }
        }
        #endregion

        #region Disable User
        /// <summary>
        /// Disable an account
        /// </summary>
        /// <param name="id">Id of a specific account</param>
        /// <returns>Sucessful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Response<GetUserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                return Ok(await mediator.Send(new DisableUserCommand(id)));
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

        #region Update Profile Img
        [HttpPost("profile-img")]
        [ProducesResponseType(typeof(Response<GetUserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UploadAsync(IFormFile file)
        {
            try
            {
                return Ok(await mediator.Send(new UploadProfileImgCommand(file)));
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
