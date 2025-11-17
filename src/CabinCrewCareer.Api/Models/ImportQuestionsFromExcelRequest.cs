namespace CabinCrewCareer.Api.Models
{
    public class ImportQuestionsFromExcelRequest
    {
        public int TestId { get; set; }
        public IFormFile ExcelFile { get; set; } = null!;
    }
}
