using Asp.Versioning;
using CabinCrewCareer.Application.Common.Enums;
using CabinCrewCareer.Application.Notifications.Commands.MarkNotificationAsRead;
using CabinCrewCareer.Application.Notifications.Common;
using CabinCrewCareer.Application.Notifications.Queries.GetMyNotifications;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CabinCrewCareer.Api.Controllers
{
    /// <summary>
    /// Controller for notification management
    /// </summary>
    [Route("api/v{version:apiVersion}/notifications")]
    [ApiVersion(1)]
    [Authorize]
    public class NotificationsController : ApiController
    {
        public NotificationsController(IMediator mediator, ILogger<NotificationsController> logger) 
            : base(mediator, logger)
        {
        }

        /// <summary>
        /// Get my notifications
        /// </summary>
        /// <param name="isRead">Filter: null=all, true=read, false=unread</param>
        /// <returns>List of notifications</returns>
        /// <response code="200">Success</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [ProducesResponseType(typeof(Response<List<NotificationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<List<NotificationDto>>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyNotifications([FromQuery] bool? isRead = null)
        {
            var query = new GetMyNotificationsQuery { IsRead = isRead };
            var response = await mediator.Send(query);
            return Ok(response);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>Success status</returns>
        /// <response code="200">Success</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Notification not found</response>
        [HttpPut("{notificationId}/read")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var command = new MarkNotificationAsReadCommand { NotificationId = notificationId };
            var response = await mediator.Send(command);
            
            if (response.Code == (int)ResponseCode.UPDATED_SUCCESS)
            {
                return Ok(response);
            }

            return NotFound(response);
        }
    }
}
