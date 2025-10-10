namespace CabinCrewCareer.Domain.Wrappers
{
    public class PaginationResponse<T>
    {
        public PaginationResponse() { }
        public PaginationResponse(PaginationData<T> data, string message, int code = 0)
        {
            Message = message;
            Code = code;
            Data = data;
        }

        public PaginationResponse(string message, int code = 0)
        {
            Message = message;
            Code = code;
        }
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaginationData<T> Data { get; set; } = null!;
    }

    public class PaginationData<T>
    {
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 0;
        public int TotalPage { get; set; } = 0;
        public long TotalSize { get; set; } = 0;
        public List<T> Items { get; set; } = [];
    }
}
