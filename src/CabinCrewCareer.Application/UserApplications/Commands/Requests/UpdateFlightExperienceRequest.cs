namespace CabinCrewCareer.Application.UserApplications.Commands.Requests
{
    public class UpdateFlightExperienceRequest
    {
        public int TotalFlightHours { get; set; }
        public string? Experience { get; set; }
    }
}
