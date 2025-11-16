using CabinCrewCareer.Application.Notifications.Common;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;

namespace CabinCrewCareer.Application.Notifications.Queries.GetMyNotifications
{
    public class GetMyNotificationsQuery : IRequest<Response<List<NotificationDto>>>
    {
        public bool? IsRead { get; set; } // null = all, true = read only, false = unread only
    }
}
