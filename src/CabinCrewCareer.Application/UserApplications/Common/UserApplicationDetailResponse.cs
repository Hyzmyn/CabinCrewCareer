namespace CabinCrewCareer.Application.UserApplications.Common
{
    public class DocumentResponse
    {
        public int DocumentId { get; set; }
        public string DocumentURL { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? UploadDate { get; set; }
    }

    public class UserApplicationDetailResponse
    {
        public int ApplicationId { get; set; }
        public string? Status { get; set; }
        public string? SubmissionDate { get; set; }
        public string? Experience { get; set; }
        public int TotalFlightHours { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string? EnglishDegreeNumber { get; set; }
        public string? EndDate { get; set; }
        public string? CitizenId { get; set; }
        public string? EnglishTestDate { get; set; }
        public int ListeningScore { get; set; }
        public int ReadingScore { get; set; }
        public int TotalScore { get; set; }
        public List<DocumentResponse> Documents { get; set; } = [];
    }
}
