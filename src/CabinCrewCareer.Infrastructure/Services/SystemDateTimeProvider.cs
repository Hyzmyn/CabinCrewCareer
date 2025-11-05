using CabinCrewCareer.Application.Common.Interfaces.Services;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow.AddHours(7);
    }
}
