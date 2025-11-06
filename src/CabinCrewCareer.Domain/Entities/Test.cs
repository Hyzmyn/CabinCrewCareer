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

        // Foreign Keys

        // Navigation Properties
        public virtual ICollection<TestQuestion> TestQuestions { get; set; } = [];
        public virtual ICollection<TestSession> TestSessions { get; set; } = [];
        //public virtual ICollection<TestResult> TestResults { get; set; } = [];
    }
}
