using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum RoundType
    {
        [Description("Screening")]
        Screening = 1,
        [Description("Appearance")]
        Appearance,
        [Description("Test")]
        Test,
        [Description("Interview")]
        Interview,
        [Description("Final")]
        Final
    }
}
