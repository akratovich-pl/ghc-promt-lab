using PromptLab.Api.Middleware;

namespace PromptLab.Api.Extensions;

/// <summary>
/// Extension methods for configuring rate limiting middleware
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the application pipeline.
    /// Rate limiting is enforced based on client IP address.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitMiddleware>();
    }
}
