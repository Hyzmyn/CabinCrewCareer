using Microsoft.AspNetCore.Mvc;
using CabinCrewCareer.Application.UserApplications.Commands.Requests;
using CabinCrewCareer.Application.UserApplications.Commands;
using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Application.Common.Interfaces.Persistence.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

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
		public async Task<IActionResult> VerifyApplication(int id)
		{
			var command = new VerifyUserApplicationCommand(id);
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
	}
}
