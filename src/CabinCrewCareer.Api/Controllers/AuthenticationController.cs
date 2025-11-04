using Asp.Versioning;
using CabinCrewCareer.Application.Authentication.Commands.Registration;
using CabinCrewCareer.Application.Authentication.Common;
using CabinCrewCareer.Application.Authentication.Queries.Login;
using CabinCrewCareer.Application.Common.Exceptions;
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
        [HttpPost("registration")]
        [ProducesResponseType(typeof(Response<AuthenticationResult>), (int)HttpStatusCode.OK)]
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
