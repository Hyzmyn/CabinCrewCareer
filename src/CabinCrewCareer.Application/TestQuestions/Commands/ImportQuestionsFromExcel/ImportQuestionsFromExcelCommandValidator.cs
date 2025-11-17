using FluentValidation;

namespace CabinCrewCareer.Application.TestQuestions.Commands.ImportQuestionsFromExcel
{
    public class ImportQuestionsFromExcelCommandValidator : AbstractValidator<ImportQuestionsFromExcelCommand>
    {
        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public ImportQuestionsFromExcelCommandValidator()
        {
            RuleFor(x => x.TestId)
                .GreaterThan(0)
                .WithMessage("Test ID must be greater than 0");

            RuleFor(x => x.ExcelFile)
                .NotNull()
                .WithMessage("Excel file is required");

            When(x => x.ExcelFile != null, () =>
            {
                RuleFor(x => x.ExcelFile.Length)
                    .LessThanOrEqualTo(MaxFileSize)
                    .WithMessage($"File size must not exceed {MaxFileSize / 1024 / 1024} MB");

                RuleFor(x => x.ExcelFile.FileName)
                    .Must(fileName =>
                    {
                        var extension = Path.GetExtension(fileName).ToLowerInvariant();
                        return AllowedExtensions.Contains(extension);
                    })
                    .WithMessage("Only .xlsx and .xls files are allowed");
            });
        }
    }
}
