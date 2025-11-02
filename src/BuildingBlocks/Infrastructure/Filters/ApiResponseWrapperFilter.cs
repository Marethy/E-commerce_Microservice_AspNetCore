using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.SeedWork.ApiResult;

namespace Infrastructure.Filters;

/// <summary>
/// Action filter to automatically wrap controller responses in ApiResult<T>
/// Simplifies controller code and ensures consistent response format
/// </summary>
public class ApiResponseWrapperFilter : IActionFilter
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Nothing to do before action executes
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Skip wrapping for already wrapped results or error responses handled by middleware
        if (context.Result is ObjectResult objectResult && objectResult.StatusCode >= 400)
        {
            return;
        }

        // Get correlation ID from HttpContext
        var correlationId = context.HttpContext.Items.TryGetValue(CorrelationIdHeader, out var corrId)
            ? corrId?.ToString()
            : null;

        // Wrap successful responses
        if (context.Result is ObjectResult okResult)
        {
            var value = okResult.Value;
            
            // If already wrapped, just add correlation ID
            if (IsAlreadyWrapped(value))
            {
                AddCorrelationId(value, correlationId);
                return;
            }

            // Wrap in ApiResult
            var wrappedResult = new ApiResult<object>
            {
                IsSuccess = true,
                Data = value,
                Message = GetSuccessMessage(context),
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            };

            context.Result = new ObjectResult(wrappedResult)
            {
                StatusCode = okResult.StatusCode ?? 200
            };
        }
        else if (context.Result is StatusCodeResult statusCodeResult)
        {
            // Handle status code only results (e.g., NoContent, Accepted)
            var wrappedResult = new ApiResult<object>
            {
                IsSuccess = statusCodeResult.StatusCode >= 200 && statusCodeResult.StatusCode < 300,
                Message = GetStatusCodeMessage(statusCodeResult.StatusCode),
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            };

            context.Result = new ObjectResult(wrappedResult)
            {
                StatusCode = statusCodeResult.StatusCode
            };
        }
    }

    private bool IsAlreadyWrapped(object value)
    {
        if (value == null) return false;
        
        var type = value.GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResult<>);
    }

    private void AddCorrelationId(object result, string correlationId)
    {
        if (string.IsNullOrEmpty(correlationId)) return;

        var property = result.GetType().GetProperty("CorrelationId");
        if (property != null && property.CanWrite)
        {
            property.SetValue(result, correlationId);
        }
    }

    private string GetSuccessMessage(ActionExecutedContext context)
    {
        var method = context.HttpContext.Request.Method;
        return method switch
        {
            "POST" => "Resource created successfully",
            "PUT" => "Resource updated successfully",
            "DELETE" => "Resource deleted successfully",
            "PATCH" => "Resource patched successfully",
            _ => "Request completed successfully"
        };
    }

    private string GetStatusCodeMessage(int statusCode)
    {
        return statusCode switch
        {
            200 => "Request completed successfully",
            201 => "Resource created successfully",
            202 => "Request accepted for processing",
            204 => "Request completed with no content",
            _ => $"Request completed with status {statusCode}"
        };
    }
}
