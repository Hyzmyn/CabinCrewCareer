namespace CabinCrewCareer.Application.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static string GetFormatDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }

        public static string GetFormatDate(this DateOnly date)
        {
            return date.ToString("dd/MM/yyyy");
        }
    }
}
