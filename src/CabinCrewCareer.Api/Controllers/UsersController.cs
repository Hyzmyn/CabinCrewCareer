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
        /// <summary>
        /// Admin create an account for Recruiter, Examiner and Airline Partner
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "username": "howsyourday",
        ///       "fullName": "Nguyễn Phi Hùng",     
        ///       "phoneNumber": "0123456789",
        ///       "email": "hungphn@gmail.com",
        ///       "dateOfBirth": "2003-01-01",
        ///       "roleId": 4
        ///     }
        ///  
        /// Recruiter: 4, Examiner: 5 and Airline Partner: 8
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
                    errorMessage = e.ErrorMessage,
                    errors = e.Errors
                });
            }
        }
        #endregion

        #region Get Users
        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <param name="searchTerm">Search by account's full name</param>
        /// <param name="sortColumn">Sort on a specifice column. Example: "name"</param>
        /// <param name="sortOrder">"asc" or "desc"</param>
        /// <param name="isActive">1: Active, 0: Inactive</param>
        /// <param name="roleId">4: Recruiter, 5: Examiner, 6: Cabin Crew, 7: Candidate, 8: Airline Partner</param>
        /// <param name="partnerId">1: VietJet Air, 3: Vietnam Airlines, 4: Bamboo Airways, 5: Sun PhuQuoc Airways</param>
        /// <param name="page">Current page</param>
        /// <param name="pageSize">Number of records of a specific page</param>
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
        public async Task<IActionResult> GetAllAsync([FromQuery]
        string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        int? isActive,
        int? roleId,
        int? partnerId,
        int page,
        int pageSize)
        {
            try
            {
                return Ok(await mediator.Send(
                    new GetUsersQuery(
                        searchTerm,
                        sortColumn,
                        sortOrder,
                        isActive,
                        roleId,
                        partnerId,
                        page,
                        pageSize)));
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
        /// <summary>
        /// Update account's profile
        /// </summary>
        /// <param name="id">Id of a specific account</param>
        /// <param name="request">Request body</param>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "fullname": "Nguyễn Phi Hùng",
        ///       "email": "hungphn@gmail.com",
        ///       "phoneNumber": "0123456789",
        ///       "dateOfBirth": "2003-01-01",
        ///       "gender": 1,
        ///       "address": "1 Lê Duẩn",
        ///       "wardId": 1
        ///     }
        ///         
        /// </remarks> 
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Authenticated</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
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
                    errorMessage = e.ErrorMessage,
                    errors = e.Errors
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
        /// <summary>
        /// Update account's profile image
        /// </summary>
        /// <param name="file">Picture file</param>
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPut("profile-img")]
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
