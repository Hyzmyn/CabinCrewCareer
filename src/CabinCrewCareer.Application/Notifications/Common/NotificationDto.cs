namespace CabinCrewCareer.Application.Notifications.Common
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public bool IsRead { get; set; }
    }
}
