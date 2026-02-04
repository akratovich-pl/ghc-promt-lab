using Microsoft.Extensions.Options;
using PromptLab.Core.Configuration;
using PromptLab.Core.RateLimiter;

namespace PromptLab.Api.Middleware;

/// <summary>
/// Middleware that enforces rate limiting on API requests
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;
    private readonly RateLimitingOptions _options;

    public RateLimitMiddleware(
        RequestDelegate next,
        ILogger<RateLimitMiddleware> logger,
        IOptions<RateLimitingOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InvokeAsync(HttpContext context, IRateLimiter rateLimiter)
    {
        // Skip rate limiting if disabled
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        // Skip rate limiting for health check endpoints
        if (context.Request.Path.StartsWithSegments("/api/health"))
        {
            await _next(context);
            return;
        }

        // Use IP address as the rate limit key
        var clientKey = GetClientKey(context);

        // Check rate limit
        var isAllowed = await rateLimiter.CheckRateLimitAsync(clientKey, context.RequestAborted);
        var remaining = await rateLimiter.GetRemainingRequestsAsync(clientKey, context.RequestAborted);

        // Add rate limit headers
        context.Response.Headers.Append("X-RateLimit-Limit-Minute", _options.RequestsPerMinute.ToString());
        context.Response.Headers.Append("X-RateLimit-Limit-Hour", _options.RequestsPerHour.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", remaining.ToString());

        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for client: {ClientKey}", clientKey);
            
            // Calculate retry after time (use the minute window)
            var retryAfterSeconds = 60;
            context.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "Too Many Requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = retryAfterSeconds
            });
            
            await context.Response.WriteAsync(jsonResponse);
            return;
        }

        // Record the request
        await rateLimiter.RecordRequestAsync(clientKey, context.RequestAborted);

        await _next(context);
    }

    private static string GetClientKey(HttpContext context)
    {
        // Try to get the real IP address from headers (for scenarios behind proxies)
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                        ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown";

        return $"client:{ipAddress}";
    }
}
