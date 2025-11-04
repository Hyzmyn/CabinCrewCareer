namespace CabinCrewCareer.Domain.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public bool IsRead { get; set; }

        // Foreign Key
        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? NotificationTypeId { get; set; }
        public virtual NotificationType NotificationType { get; set; } = null!;
    }
}
