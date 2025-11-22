namespace CabinCrewCareer.Application.UserApplications.Common
{
    public class UserApplicationResponse
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }

    public class Response<T>
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public object? Errors { get; set; }
    }
}
