namespace Shared.SeedWork
{
    public class ApiErrorResult<T> : ApiResult<T>
    {
        public List<string> Errors { get; set; }

        public ApiErrorResult() : this("Something went wrong")
        {
        }

        public ApiErrorResult(string message)
            : base(false, default, message)
        {
            Errors = new List<string> { message };
        }

        public ApiErrorResult(List<string> errors)
            : base(false, string.Join("; ", errors))
        {
            Errors = errors;
        }
    }
}