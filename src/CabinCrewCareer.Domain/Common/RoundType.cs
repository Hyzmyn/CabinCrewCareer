using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum RoundType
    {
        [Description("Screening")]
        Screening = 1,
        [Description("Appearance")]
        Appearance,
        [Description("Flight Hours Confirmation")]
        FlightHoursConfirmation,
        [Description("English Listening Test")]
        EnglishListeningTest,
        [Description("English Speaking Test")]
        EnglishSpeakingTest,
        [Description("Practical Test")]
        PracticalTest,
        [Description("Interview")]
        Interview,
        [Description("Final")]
        Final
    }
}
