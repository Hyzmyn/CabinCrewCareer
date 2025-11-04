using CabinCrewCareer.Domain.Entities.Base.Implementations;

namespace CabinCrewCareer.Domain.Entities
{
    public class Blog : AuditableEntity
    {
        public int BlogId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public string? Tags { get; set; } = string.Empty;

        // Foreign Key
        public int? CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public int? UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<Comment> Comments { get; set; } = [];
    }
}
