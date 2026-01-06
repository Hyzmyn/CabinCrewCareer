using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum RoundConfigurationType
    {
        [Description("Appearance")]
        Appearance = 1,
        [Description("Test")]
        Test,
        [Description("Interview")]
        Interview,
    }
}
