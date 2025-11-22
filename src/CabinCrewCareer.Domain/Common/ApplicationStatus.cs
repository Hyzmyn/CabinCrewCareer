using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum ApplicationStatus
    {
        [Description("Draft")]
        Draft = 1,
        [Description("Submitted")]
        Submitted = 2
    }
}
