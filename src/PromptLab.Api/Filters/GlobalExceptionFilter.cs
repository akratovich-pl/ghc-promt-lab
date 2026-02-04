using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PromptLab.Api.Filters;

/// <summary>
/// Global exception filter that centralizes error handling across all API endpoints
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionFilter(
        ILogger<GlobalExceptionFilter> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var (statusCode, title) = MapExceptionToStatusCode(exception);

        LogException(context, exception, statusCode);

        var problemDetails = CreateProblemDetails(context, exception, statusCode, title);
        
        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }

    private (int statusCode, string title) MapExceptionToStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentException or ArgumentNullException => 
                (StatusCodes.Status400BadRequest, "Invalid Request"),
            InvalidOperationException => 
                (StatusCodes.Status400BadRequest, "Invalid Operation"),
            KeyNotFoundException => 
                (StatusCodes.Status404NotFound, "Not Found"),
            UnauthorizedAccessException => 
                (StatusCodes.Status403Forbidden, "Forbidden"),
            _ => 
                (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
    }

    private void LogException(ExceptionContext context, Exception exception, int statusCode)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;
        
        var logMessage = "Error processing request {Method} {Path}";
        var logArgs = new object[] { request.Method, request.Path };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, logMessage, logArgs);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(exception, logMessage, logArgs);
        }
    }

    private ProblemDetails CreateProblemDetails(
        ExceptionContext context, 
        Exception exception, 
        int statusCode, 
        string title)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = context.HttpContext.Request.Path
        };

        // Add correlation ID if available from HTTP context
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        // For 4xx errors, include detailed exception messages (client-actionable)
        // For 5xx errors in Development, include details; in Production, sanitize
        if (statusCode < 500)
        {
            problemDetails.Detail = exception.Message;
        }
        else if (_environment.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            
            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = new
                {
                    message = exception.InnerException.Message,
                    stackTrace = exception.InnerException.StackTrace
                };
            }
        }
        else
        {
            // Production: sanitized message for 5xx errors
            problemDetails.Detail = "An error occurred while processing your request";
        }

        return problemDetails;
    }
}
