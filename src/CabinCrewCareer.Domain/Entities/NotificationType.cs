namespace CabinCrewCareer.Domain.Entities
{
    public class NotificationType
    {
        public int NotificationTypeId { get; set; }
        public string NotificationTypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation
        public virtual ICollection<Notification> Notifications { get; set; } = [];
    }
}
