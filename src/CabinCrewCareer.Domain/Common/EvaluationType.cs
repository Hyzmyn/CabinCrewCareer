using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum EvaluationType
    {
        [Description("Appearance")]
        Appearance = 1,
        [Description("Interview")]
        Interview,
    }
}
