using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/dashboard")]
    [ApiVersion(1)]
    [Authorize]
    public class DashboardController : ApiController
    {
        public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
            : base(mediator, logger)
        {
        }

        // GET: api/v1/dashboard/campaigns-by-campaign-type?type=1
        [HttpGet("campaigns-by-campaign-type")]
        [AllowAnonymous]
           public async Task<IActionResult> GetCampaignsByCampaignType()
        {
                var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetOngoingCampaignCountByType.GetOngoingCampaignCountByTypeQuery());
            return Ok(result);
        }

        // GET: api/v1/dashboard/campaigns-by-airline-partner?airlinePartnerId=1
        [HttpGet("campaigns-by-airline-partner")]
        [AllowAnonymous]
           public async Task<IActionResult> GetCampaignsByAirlinePartner()
        {
                var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetCampaignCountByAirlinePartner.GetCampaignCountByAirlinePartnerQuery());
            return Ok(result);
        }

        // GET: api/v1/dashboard/applications-by-campaign-type?type=1
        [HttpGet("applications-by-campaign-type")]
        [AllowAnonymous]
           public async Task<IActionResult> GetApplicationsByCampaignType()
        {
               var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetApplicationCountByCampaignType.GetApplicationCountByCampaignTypeQuery());
                return Ok(result);
        }

        // GET: api/v1/dashboard/tests-by-test-type?type=1
        [HttpGet("tests-by-test-type")]
        [AllowAnonymous]
           public async Task<IActionResult> GetTestsByTestType()
        {
                var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetTestCountByTestType.GetTestCountByTestTypeQuery());
            return Ok(result);
        }

        // GET: api/v1/dashboard/campaigns-by-month?year=2025
        [HttpGet("campaigns-by-month")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCampaignsByMonth([FromQuery] int year)
        {
            var campaigns = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetCampaignsByMonth.GetCampaignsByMonthQuery(year));
            return Ok(campaigns);
        }

        // GET: api/v1/dashboard/applications-by-month?year=2025
        [HttpGet("applications-by-month")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicationsByMonth([FromQuery] int year)
        {
            var snapshots = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetApplicationsByMonth.GetApplicationsByMonthQuery(year));
            return Ok(snapshots);
        }

        // GET: api/v1/dashboard/userhistory-by-month?year=2025
        [HttpGet("userhistory-by-month")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserHistoryByMonth([FromQuery] int year)
        {
            var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetUserHistoryByMonth.GetUserHistoryByMonthQuery(year));
            return Ok(result);
        }
    }
}
