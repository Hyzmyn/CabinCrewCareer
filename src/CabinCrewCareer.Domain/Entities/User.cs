using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities.Base.Implementations;

namespace CabinCrewCareer.Domain.Entities
{
    public class User : AuditableEntity
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public string? ImgURL { get; set; } = string.Empty;

        public string? Address { get; set; } = string.Empty;

        public string? ResetPasswordOtp { get; set; }

        // Foreign Keys
        public int? RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        public int? WardId { get; set; }
        public virtual Ward Ward { get; set; } = null!;

        public int? PartnerId { get; set; }
        public virtual AirlinePartner AirlinePartner { get; set; } = null!;

        public virtual UserApplication? Application { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<Blog> Blogs { get; set; } = [];
        public virtual ICollection<Comment> Comments { get; set; } = [];
        public virtual ICollection<Notification> Notifications { get; set; } = [];
        public virtual ICollection<CampaignRequest> CampaignRequests { get; set; } = [];
        public virtual ICollection<Campaign> Campaigns { get; set; } = [];
    }
}
