using Microsoft.AspNetCore.Mvc;

namespace CabinCrewCareer.Api.Models;

public class CreateTestRequest
{
    public string TestName { get; set; } = null!;
    public string Purpose { get; set; } = null!;
    public int TestType { get; set; }
    public int MaxScore { get; set; }
    public IFormFile? AudioFile { get; set; }
}
