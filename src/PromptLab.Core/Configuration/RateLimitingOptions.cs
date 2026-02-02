namespace PromptLab.Core.Configuration;

/// <summary>
/// Configuration settings for rate limiting
/// </summary>
public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Maximum number of requests allowed per minute
    /// </summary>
    public int RequestsPerMinute { get; set; } = 60;

    /// <summary>
    /// Maximum number of requests allowed per hour
    /// </summary>
    public int RequestsPerHour { get; set; } = 1000;

    /// <summary>
    /// Whether rate limiting is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
}
