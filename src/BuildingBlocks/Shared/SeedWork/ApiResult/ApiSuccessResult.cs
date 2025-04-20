namespace Shared.SeedWork.ApiResult
{
    public class ApiSuccessResult<T> : ApiResult<T>
    {
        public ApiSuccessResult(T data)
            : base(true, data, "Success")
        {
        }

        public ApiSuccessResult(T data, string message = "Success")
            : base(true, data, message)
        {
        }
    }
}