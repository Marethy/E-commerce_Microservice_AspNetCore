namespace Shared.SeedWork
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }

        // Default constructor for creating an empty result
        public ApiResult()
        { }

        // Constructor for success or failure with an optional message
        public ApiResult(bool isSuccess, string message = null) : this(isSuccess, default, message) { }

        // Constructor for success or failure with data and an optional message
        public ApiResult(bool isSuccess, T data, string message = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
        }

        // Optional: A ToString method for easy debugging
        public override string ToString()
        {
            return $"IsSuccess: {IsSuccess}, Data: {Data}, Message: {Message}";
        }
    }
}