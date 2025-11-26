using CabinCrewCareer.Application.Common.Interfaces.Services;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow.AddHours(7);

        public DateOnly Today => DateOnly.FromDateTime(UtcNow.AddHours(7));
    }
}
