using System.Net;

namespace DeferredTaskAPI.Models
{
    public class ApiResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsFailed { get; set; }
        public IEnumerable<string>? Errors {  get; set; }
    }

    public class ApiResult<T> : ApiResult
    {
        public T? Value { get; set; }
    }
}
