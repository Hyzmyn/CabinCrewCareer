using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum ApplicationStatus
    {
        [Description("Draft")]
        Administrator = 1,
        [Description("Pending")]
        SubjectSpecialistManager = 2,
        [Description("Verified")]
        Teacher = 4,
    }
}
