namespace CabinCrewCareer.Domain.Wrappers
{
    public class Response<T>
    {
        public Response() { }
        public Response(T? data, string? message = null, int? code = 0)
        {
            Message = message;
            Code = code;
            Data = data;
        }

        public Response(string? message, int? code = 0)
        {
            Message = message;
            Code = code;
        }

        public int? Code { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
