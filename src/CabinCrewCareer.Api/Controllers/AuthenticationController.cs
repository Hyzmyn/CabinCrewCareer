using Asp.Versioning;
using CabinCrewCareer.Application.Authentication.Commands.Registration;
using CabinCrewCareer.Application.Authentication.Common;
using CabinCrewCareer.Application.Authentication.Queries.Login;
using CabinCrewCareer.Application.Common.Exceptions;
using CabinCrewCareer.Application.Users.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/auth")]
    [ApiVersion(1)]
    public class AuthenticationController(
        IMediator mediator,
        ILogger<AuthenticationController> logger) : ApiController(mediator, logger)
    {

        #region Login
        /// <summary>
        /// Log into system using username and password
        /// </summary>    
        /// <remarks>
        /// Sample request:  
        /// 
        ///     {
        ///       "username": "admin",
        ///       "password": "admin"
        ///     }   
        ///         
        /// </remarks>
        /// <returns>Access Token and Refresh Token</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(Response<AuthenticationResult>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginQuery query)
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

        #region Registration
        /// <summary>
        /// Register an account
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///       "username": "howsyourday",
        ///       "password": "callmebaby",
        ///       "confirmPassword": "callmebaby",
        ///       "fullName": "Nguyễn Phi Hùng",
        ///       "gender": 1,
        ///       "dateOfBirth": "2003-01-01",
        ///       "phoneNumber": "0123456789",
        ///       "email": "hungphn@gmail.com",
        ///     }
        ///         
        /// </remarks> 
        /// <returns>Successful code</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("registration")]
        [ProducesResponseType(typeof(Response<GetUserResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SignUpAsync([FromBody] RegistrationCommand command)
        {
            try
            {
                return Ok(await mediator.Send(command));
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
