namespace CabinCrewCareer.Application.Common.Interfaces.Services
{
    public interface IDateTimeProvider
    {
        public DateTime UtcNow { get; }

        public DateOnly Today { get; }
    }
}
