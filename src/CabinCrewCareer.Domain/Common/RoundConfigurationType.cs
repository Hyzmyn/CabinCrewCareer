using System.ComponentModel;

namespace CabinCrewCareer.Domain.Common
{
    public enum RoundConfigurationType
    {
        [Description("Appearance")]
        Appearance = 2,
        [Description("English Listening Test")]
        EnglishListeningTest = 3,
        [Description("English Speaking Test")]
        EnglishSpeakingTest = 4,
        [Description("Interview")]
        RecruitmentInterview = 5,
        [Description("Practical Test")]
        PracticalTest = 9,
        [Description("Interview")]
        PromotionInterview = 10
    }
}
