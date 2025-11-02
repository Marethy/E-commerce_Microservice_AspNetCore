using System.Text.Json.Serialization;

namespace Shared.SeedWork.ApiResult
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        
        /// <summary>
        /// Timestamp for tracking request completion (useful for AI analytics)
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Correlation ID for distributed tracing across microservices
        /// </summary>
        public string CorrelationId { get; set; }
        
        /// <summary>
        /// List of validation or error messages
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Errors { get; set; }

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

        /// <summary>
        /// Create a success response with data
        /// </summary>
        public static ApiResult<T> Success(T data, string message = null)
        {
            return new ApiResult<T>(true, data, message);
        }

        /// <summary>
        /// Create a success response without data
        /// </summary>
        public static ApiResult<T> Success(string message = null)
        {
            return new ApiResult<T>(true, default, message);
        }

        /// <summary>
        /// Create an error response with a single error message
        /// </summary>
        public static ApiResult<T> Failure(string error)
        {
            return new ApiResult<T>(false, default, error)
            {
                Errors = new List<string> { error }
            };
        }

        /// <summary>
        /// Create an error response with multiple error messages
        /// </summary>
        public static ApiResult<T> Failure(List<string> errors, string message = null)
        {
            return new ApiResult<T>(false, default, message)
            {
                Errors = errors
            };
        }

        /// <summary>
        /// Set correlation ID for distributed tracing (fluent API)
        /// </summary>
        public ApiResult<T> WithCorrelationId(string correlationId)
        {
            CorrelationId = correlationId;
            return this;
        }

        // Optional: A ToString method for easy debugging
        public override string ToString()
        {
            return $"IsSuccess: {IsSuccess}, Data: {Data}, Message: {Message}, Timestamp: {Timestamp}, CorrelationId: {CorrelationId}";
        }
    }
}