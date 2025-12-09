using CabinCrewCareer.Domain.Common;
using CabinCrewCareer.Domain.Entities.Base.Implementations;

namespace CabinCrewCareer.Domain.Entities
{
    public class Test : AuditableEntity
    {
        public int TestId { get; set; }

        public string TestName { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        public TestType TestType { get; set; }

        public int MaxScore { get; set; }

        public string AudioFileURL { get; set; } = string.Empty;

        public string JoinCode { get; set; } = string.Empty;

        public int DurationInMinutes { get; set; }

        // Foreign Keys
        public int? TestTypeId { get; set; }
        public virtual TypeTest Type { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<TestQuestion> TestQuestions { get; set; } = [];
        public virtual ICollection<TestSession> TestSessions { get; set; } = [];
        public virtual ICollection<Round> Rounds { get; set; } = [];
    }
}
