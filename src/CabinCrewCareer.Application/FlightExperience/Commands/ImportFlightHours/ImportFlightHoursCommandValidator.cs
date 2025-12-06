using FluentValidation;

namespace CabinCrewCareer.Application.FlightExperience.Commands.ImportFlightHours
{
    public class ImportFlightHoursCommandValidator : AbstractValidator<ImportFlightHoursCommand>
    {
        public ImportFlightHoursCommandValidator()
        {
            RuleFor(x => x.RoundId)
                .GreaterThan(0)
                .WithMessage("Round ID must be greater than 0");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required");

            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("File cannot be empty")
                .When(x => x.File != null);

            RuleFor(x => x.File.FileName)
                .Must(filename => filename.EndsWith(".xlsx") || filename.EndsWith(".xls"))
                .WithMessage("File must be an Excel file (.xlsx or .xls)")
                .When(x => x.File != null);
        }
    }
}
