using CabinCrewCareer.Domain.Entities.Base.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CabinCrewCareer.Domain.Entities.Base.Implementations
{
    public abstract class AuditableEntity : IAuditableEntity
    {
        [MaxLength(20)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        [MaxLength(20)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
