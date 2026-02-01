namespace PromptLab.Core.Services.Interfaces;

/// <summary>
/// Interface for rate limiting service
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Checks if the request can proceed based on rate limits
    /// </summary>
    Task<RateLimitResult> CheckRateLimitAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records usage for rate limiting
    /// </summary>
    Task RecordUsageAsync(string userId, int tokensUsed, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of rate limit check
/// </summary>
public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public string? Message { get; set; }
    public int? RetryAfterSeconds { get; set; }
}
