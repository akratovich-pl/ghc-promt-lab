namespace PromptLab.Core.Logging;

/// <summary>
/// Defines custom log event IDs for structured logging throughout the application.
/// </summary>
public static class LogEvents
{
    // Prompt Execution Events (1000-1999)
    public const int PromptExecutionStarted = 1000;
    public const int PromptExecutionCompleted = 1001;
    public const int PromptExecutionFailed = 1002;

    // API Communication Events (2000-2999)
    public const int ApiRequestSent = 2000;
    public const int ApiResponseReceived = 2001;
    public const int ApiRequestFailed = 2002;
    public const int ApiRetryAttempt = 2003;

    // Rate Limiting Events (3000-3999)
    public const int RateLimitExceeded = 3000;
    public const int RateLimitWarning = 3001;

    // Configuration Events (4000-4999)
    public const int ConfigurationLoaded = 4000;
    public const int ConfigurationError = 4001;

    // Health Check Events (5000-5999)
    public const int HealthCheckStarted = 5000;
    public const int HealthCheckCompleted = 5001;
    public const int HealthCheckFailed = 5002;

    // Database Events (6000-6999)
    public const int DatabaseConnectionOpened = 6000;
    public const int DatabaseConnectionFailed = 6001;
    public const int DatabaseQueryExecuted = 6002;

    // Cache Events (7000-7999)
    public const int CacheHit = 7000;
    public const int CacheMiss = 7001;
    public const int CacheError = 7002;

    // Performance Metrics Events (8000-8999)
    public const int PerformanceMetricRecorded = 8000;
    public const int PerformanceThresholdExceeded = 8001;
}
