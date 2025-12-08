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

        /// <summary>
        /// Lấy số lượng campaign đang diễn ra theo từng loại CampaignType
        /// </summary>
        // GET: api/v1/dashboard/campaigns-by-campaign-type
        [HttpGet("campaigns-by-campaign-type")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCampaignsByCampaignType()
        {
            var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetOngoingCampaignCountByType.GetOngoingCampaignCountByTypeQuery());
            return Ok(result);
        }

        /// <summary>
        /// Lấy số lượng campaign theo từng hãng máy bay
        /// </summary>
        // GET: api/v1/dashboard/campaigns-by-airline-partner
        [HttpGet("campaigns-by-airline-partner")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCampaignsByAirlinePartner()
        {
            var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetCampaignCountByAirlinePartner.GetCampaignCountByAirlinePartnerQuery());
            return Ok(result);
        }

        /// <summary>
        /// Lấy số lượng ứng viên nộp hồ sơ vào từng loại chiến dịch
        /// </summary>
        // GET: api/v1/dashboard/applications-by-campaign-type
        [HttpGet("applications-by-campaign-type")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicationsByCampaignType()
        {
            var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetApplicationCountByCampaignType.GetApplicationCountByCampaignTypeQuery());
            return Ok(result);
        }

        /// <summary>
        /// Lấy số lượng bài test theo từng loại TestType
        /// </summary>
        // GET: api/v1/dashboard/tests-by-test-type
        [HttpGet("tests-by-test-type")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTestsByTestType()
        {
            var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetTestCountByTestType.GetTestCountByTestTypeQuery());
            return Ok(result);
        }

        /// <summary>
        /// Lấy số lượng campaign theo từng tháng của năm (dựa vào StartDate)
        /// </summary>
        // GET: api/v1/dashboard/campaigns-by-month?year=2025
        [HttpGet("campaigns-by-month")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCampaignsByMonth([FromQuery] int year)
        {
            var campaigns = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetCampaignsByMonth.GetCampaignsByMonthQuery(year));
            return Ok(campaigns);
        }

        /// <summary>
        /// Lấy số lượng ứng viên nộp hồ sơ theo từng tháng của năm (dựa vào SubmissionDate)
        /// </summary>
        // GET: api/v1/dashboard/applications-by-month?year=2025
        [HttpGet("applications-by-month")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicationsByMonth([FromQuery] int year)
        {
            var snapshots = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetApplicationsByMonth.GetApplicationsByMonthQuery(year));
            return Ok(snapshots);
        }

        /// <summary>
        /// Lấy số lượng user pass và failed theo từng tháng của năm (dựa vào UserHistory)
        /// </summary>
        // GET: api/v1/dashboard/userhistory-by-month?year=2025
        [HttpGet("userhistory-by-month")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserHistoryByMonth([FromQuery] int year)
        {
            var result = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetUserHistoryByMonth.GetUserHistoryByMonthQuery(year));
            return Ok(result);
        }

        /// <summary>
        /// Lấy tổng số lượng campaign
        /// </summary>
        // GET: api/v1/dashboard/total-campaigns
        [HttpGet("total-campaigns")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalCampaigns()
        {
            var total = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetTotalCampaigns.GetTotalCampaignsQuery());
            return Ok(total);
        }

        /// <summary>
        /// Lấy tổng số lượng bài test
        /// </summary>
        // GET: api/v1/dashboard/total-tests
        [HttpGet("total-tests")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalTests()
        {
            var total = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetTotalTests.GetTotalTestsQuery());
            return Ok(total);
        }

        /// <summary>
        /// Lấy tổng số lượng user (bao gồm CabinCrew và Candidate)
        /// </summary>
        // GET: api/v1/dashboard/total-users
        [HttpGet("total-users")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalUsers()
        {
            var total = await mediator.Send(new CabinCrewCareer.Application.Dashboard.Queries.GetTotalUsers.GetTotalUsersQuery());
            return Ok(total);
        }
    }
}
