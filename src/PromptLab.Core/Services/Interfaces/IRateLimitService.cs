namespace PromptLab.Core.Services.Interfaces;

/// <summary>
/// Service for rate limiting API requests
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Checks if a request is allowed based on rate limits
    /// </summary>
    /// <param name="key">Unique identifier for the rate limit bucket (e.g., IP address, user ID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if request is allowed, false if rate limit exceeded</returns>
    Task<bool> CheckRateLimitAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a request timestamp for rate limiting
    /// </summary>
    /// <param name="key">Unique identifier for the rate limit bucket</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordRequestAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of remaining requests allowed in the current time window
    /// </summary>
    /// <param name="key">Unique identifier for the rate limit bucket</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of remaining requests</returns>
    Task<int> GetRemainingRequestsAsync(string key, CancellationToken cancellationToken = default);
}
