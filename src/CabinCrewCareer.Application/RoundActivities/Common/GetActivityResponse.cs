namespace CabinCrewCareer.Application.RoundActivities.Common
{
    public class GetActivityResponse
    {
        public int ActivityId { get; set; }

        public string Status { get; set; } = string.Empty;

        public int RoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public int UserId { get; set; }

        public string ImgURL { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public bool HasAppearanceEvaluated { get; set; } = false;

        public bool HasInterviewEvaluated { get; set; } = false;

        public int ApplicationSnapshotId { get; set; }
    }
}
