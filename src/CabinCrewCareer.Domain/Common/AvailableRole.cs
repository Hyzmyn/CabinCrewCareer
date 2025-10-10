using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum AvailableRole
    {
        [Description("Administator")]
        Administrator = 1,
        [Description("Subject Specialist Manager")]
        SubjectSpecialistManager = 2,
        [Description("Teacher")]
        Teacher = 4,
    }
}
