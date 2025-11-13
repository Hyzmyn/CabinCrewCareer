namespace CabinCrewCareer.Api.Models;

public class UpdateTestRequest
{
    public string TestName { get; set; } = null!;
    public string Purpose { get; set; } = null!;
    public int TestType { get; set; }
    public int MaxScore { get; set; }
    public int DurationInMinutes { get; set; }
    public IFormFile? AudioFile { get; set; }
    public bool RemoveAudio { get; set; } = false;
}