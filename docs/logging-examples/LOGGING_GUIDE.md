# Logging Best Practices and Examples

This document provides guidance on implementing structured logging in the PromptLab application using Serilog.

## Table of Contents
- [Overview](#overview)
- [Log Event IDs](#log-event-ids)
- [Logging Patterns](#logging-patterns)
- [Service Implementation Examples](#service-implementation-examples)
- [Controller Logging](#controller-logging)
- [Performance Logging](#performance-logging)
- [Error Logging](#error-logging)

## Overview

The PromptLab application uses Serilog for structured logging with the following configuration:

- **Development**: Debug level, console output with readable format
- **Production**: Information level, console + file output with JSON format
- **Log Files**: Daily rolling with 30-day retention in `logs/` directory
- **Request Logging**: Automatic HTTP request/response logging with enriched context

## Log Event IDs

Custom event IDs are defined in `PromptLab.Core.Logging.LogEvents`:

```csharp
// Prompt Execution Events (1000-1999)
PromptExecutionStarted = 1000
PromptExecutionCompleted = 1001
PromptExecutionFailed = 1002

// API Communication Events (2000-2999)
ApiRequestSent = 2000
ApiResponseReceived = 2001
ApiRequestFailed = 2002
ApiRetryAttempt = 2003

// Rate Limiting Events (3000-3999)
RateLimitExceeded = 3000
RateLimitWarning = 3001

// Configuration Events (4000-4999)
ConfigurationLoaded = 4000
ConfigurationError = 4001

// Health Check Events (5000-5999)
HealthCheckStarted = 5000
HealthCheckCompleted = 5001
HealthCheckFailed = 5002

// Database Events (6000-6999)
DatabaseConnectionOpened = 6000
DatabaseConnectionFailed = 6001
DatabaseQueryExecuted = 6002

// Cache Events (7000-7999)
CacheHit = 7000
CacheMiss = 7001
CacheError = 7002

// Performance Metrics Events (8000-8999)
PerformanceMetricRecorded = 8000
PerformanceThresholdExceeded = 8001
```

## Logging Patterns

### Use Structured Logging with Properties

❌ **Bad - String Interpolation:**
```csharp
_logger.LogInformation($"User {userId} executed prompt {promptId}");
```

✅ **Good - Structured Properties:**
```csharp
_logger.LogInformation("User executed prompt, UserId: {UserId}, PromptId: {PromptId}", userId, promptId);
```

### Include Correlation IDs

Always include correlation IDs for request tracing:

```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId,
    ["UserId"] = userId
}))
{
    _logger.LogInformation("Processing request...");
    // Your code here
}
```

### Log Operation Duration

Use `Stopwatch` or similar to track operation performance:

```csharp
var stopwatch = Stopwatch.StartNew();
try
{
    _logger.LogInformation(
        "Starting prompt execution, PromptId: {PromptId}, EventId: {EventId}",
        promptId,
        LogEvents.PromptExecutionStarted
    );

    var result = await ExecutePromptAsync(prompt);

    stopwatch.Stop();
    _logger.LogInformation(
        "Prompt execution completed, PromptId: {PromptId}, Duration: {DurationMs}ms, EventId: {EventId}",
        promptId,
        stopwatch.ElapsedMilliseconds,
        LogEvents.PromptExecutionCompleted
    );

    return result;
}
catch (Exception ex)
{
    stopwatch.Stop();
    _logger.LogError(
        ex,
        "Prompt execution failed, PromptId: {PromptId}, Duration: {DurationMs}ms, EventId: {EventId}",
        promptId,
        stopwatch.ElapsedMilliseconds,
        LogEvents.PromptExecutionFailed
    );
    throw;
}
```

## Service Implementation Examples

### PromptExecutionService Example

```csharp
using Microsoft.Extensions.Logging;
using PromptLab.Core.Interfaces;
using PromptLab.Core.Logging;
using System.Diagnostics;

public class PromptExecutionService : IPromptExecutionService
{
    private readonly ILogger<PromptExecutionService> _logger;
    private readonly IEnumerable<ILlmProvider> _providers;

    public PromptExecutionService(
        ILogger<PromptExecutionService> logger,
        IEnumerable<ILlmProvider> providers)
    {
        _logger = logger;
        _providers = providers;
    }

    public async Task<IEnumerable<Response>> ExecutePromptAsync(
        Prompt prompt,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["PromptId"] = prompt.Id
        }))
        {
            _logger.LogInformation(
                "Starting prompt execution, PromptId: {PromptId}, EstimatedTokens: {EstimatedTokens}, " +
                "ProviderCount: {ProviderCount}, EventId: {EventId}",
                prompt.Id,
                prompt.EstimatedTokens,
                _providers.Count(),
                LogEvents.PromptExecutionStarted
            );

            try
            {
                var responses = new List<Response>();

                foreach (var provider in _providers)
                {
                    try
                    {
                        _logger.LogInformation(
                            "Executing prompt with provider, Provider: {Provider}",
                            provider.ProviderName
                        );

                        var response = await provider.SendPromptAsync(prompt, cancellationToken);
                        responses.Add(response);

                        _logger.LogInformation(
                            "Provider execution completed, Provider: {Provider}, Tokens: {Tokens}, " +
                            "Cost: {Cost}, LatencyMs: {LatencyMs}",
                            provider.ProviderName,
                            response.Tokens,
                            response.Cost,
                            response.LatencyMs
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Provider execution failed, Provider: {Provider}",
                            provider.ProviderName
                        );
                    }
                }

                stopwatch.Stop();
                _logger.LogInformation(
                    "Prompt execution completed, PromptId: {PromptId}, ResponseCount: {ResponseCount}, " +
                    "TotalDuration: {DurationMs}ms, EventId: {EventId}",
                    prompt.Id,
                    responses.Count,
                    stopwatch.ElapsedMilliseconds,
                    LogEvents.PromptExecutionCompleted
                );

                return responses;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Prompt execution failed, PromptId: {PromptId}, Duration: {DurationMs}ms, " +
                    "EventId: {EventId}",
                    prompt.Id,
                    stopwatch.ElapsedMilliseconds,
                    LogEvents.PromptExecutionFailed
                );
                throw;
            }
        }
    }
}
```

### GoogleGeminiProvider Example

```csharp
using Microsoft.Extensions.Logging;
using PromptLab.Core.Interfaces;
using PromptLab.Core.Logging;
using System.Diagnostics;

public class GoogleGeminiProvider : ILlmProvider
{
    private readonly ILogger<GoogleGeminiProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private int _retryCount = 0;
    private const int MaxRetries = 3;

    public string ProviderName => "GoogleGemini";

    public GoogleGeminiProvider(
        ILogger<GoogleGeminiProvider> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _apiKey = configuration["GoogleGemini:ApiKey"] 
            ?? throw new ArgumentException("GoogleGemini API key not configured");
    }

    public async Task<Response> SendPromptAsync(
        Prompt prompt,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["Provider"] = ProviderName,
            ["PromptId"] = prompt.Id
        }))
        {
            try
            {
                _logger.LogDebug(
                    "Preparing API request, Model: {Model}, EventId: {EventId}",
                    "gemini-pro",
                    LogEvents.ApiRequestSent
                );

                // Build request (without logging sensitive data)
                var request = new
                {
                    prompt = prompt.UserPrompt,
                    context = prompt.Context
                };

                _logger.LogInformation(
                    "Sending API request, RequestId: {RequestId}, PromptLength: {PromptLength}, " +
                    "EventId: {EventId}",
                    requestId,
                    prompt.UserPrompt.Length,
                    LogEvents.ApiRequestSent
                );

                // Make API call
                var apiResponse = await SendWithRetryAsync(request, cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation(
                    "API response received, RequestId: {RequestId}, Tokens: {Tokens}, " +
                    "Latency: {LatencyMs}ms, EventId: {EventId}",
                    requestId,
                    apiResponse.Tokens,
                    stopwatch.ElapsedMilliseconds,
                    LogEvents.ApiResponseReceived
                );

                return new Response
                {
                    Id = Guid.NewGuid(),
                    PromptId = prompt.Id,
                    Provider = AiProvider.Google,
                    Model = "gemini-pro",
                    Content = apiResponse.Content,
                    Tokens = apiResponse.Tokens,
                    Cost = CalculateCost(apiResponse.Tokens),
                    LatencyMs = (int)stopwatch.ElapsedMilliseconds,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    ex,
                    "Rate limit exceeded, RequestId: {RequestId}, EventId: {EventId}",
                    requestId,
                    LogEvents.RateLimitExceeded
                );
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "API request failed, RequestId: {RequestId}, Duration: {DurationMs}ms, " +
                    "EventId: {EventId}",
                    requestId,
                    stopwatch.ElapsedMilliseconds,
                    LogEvents.ApiRequestFailed
                );
                throw;
            }
        }
    }

    private async Task<ApiResponse> SendWithRetryAsync(
        object request,
        CancellationToken cancellationToken)
    {
        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                // Make actual HTTP call here
                return await MakeHttpCallAsync(request, cancellationToken);
            }
            catch (Exception ex) when (i < MaxRetries - 1)
            {
                _retryCount++;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, i));

                _logger.LogWarning(
                    ex,
                    "API request failed, retrying, Attempt: {Attempt}/{MaxAttempts}, " +
                    "DelaySeconds: {DelaySeconds}, EventId: {EventId}",
                    i + 1,
                    MaxRetries,
                    delay.TotalSeconds,
                    LogEvents.ApiRetryAttempt
                );

                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new Exception("Max retries exceeded");
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            _logger.LogDebug("Checking provider availability");
            // Health check logic here
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider availability check failed");
            return false;
        }
    }

    private decimal CalculateCost(int tokens)
    {
        // Pricing logic
        return tokens * 0.0001m;
    }

    private async Task<ApiResponse> MakeHttpCallAsync(object request, CancellationToken cancellationToken)
    {
        // Actual HTTP implementation
        throw new NotImplementedException();
    }

    private class ApiResponse
    {
        public string Content { get; set; } = string.Empty;
        public int Tokens { get; set; }
    }
}
```

## Controller Logging

Controllers should log:
- Incoming requests with parameters
- Validation errors
- Authorization failures
- Response status codes

```csharp
[ApiController]
[Route("api/[controller]")]
public class PromptsController : ControllerBase
{
    private readonly ILogger<PromptsController> _logger;
    private readonly IPromptExecutionService _promptService;

    public PromptsController(
        ILogger<PromptsController> logger,
        IPromptExecutionService promptService)
    {
        _logger = logger;
        _promptService = promptService;
    }

    [HttpPost]
    public async Task<IActionResult> ExecutePrompt(
        [FromBody] PromptRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "Received prompt execution request, CorrelationId: {CorrelationId}, " +
            "PromptLength: {PromptLength}",
            correlationId,
            request.Prompt?.Length ?? 0
        );

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Invalid request model, CorrelationId: {CorrelationId}, Errors: {@ValidationErrors}",
                correlationId,
                ModelState.Values.SelectMany(v => v.Errors)
            );
            return BadRequest(ModelState);
        }

        try
        {
            var prompt = MapToPrompt(request);
            var responses = await _promptService.ExecutePromptAsync(prompt, cancellationToken);

            _logger.LogInformation(
                "Request processed successfully, CorrelationId: {CorrelationId}, " +
                "ResponseCount: {ResponseCount}",
                correlationId,
                responses.Count()
            );

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Request processing failed, CorrelationId: {CorrelationId}",
                correlationId
            );
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private Prompt MapToPrompt(PromptRequest request)
    {
        return new Prompt
        {
            Id = Guid.NewGuid(),
            UserPrompt = request.Prompt ?? string.Empty,
            Context = request.Context,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

## Performance Logging

Track and log performance metrics:

```csharp
public class PerformanceMetricsService
{
    private readonly ILogger<PerformanceMetricsService> _logger;
    private readonly ConcurrentDictionary<string, PerformanceMetric> _metrics = new();

    public void RecordMetric(string operation, long durationMs, bool success)
    {
        var metric = _metrics.GetOrAdd(operation, _ => new PerformanceMetric());
        metric.Record(durationMs, success);

        if (metric.SampleCount >= 100)
        {
            _logger.LogInformation(
                "Performance metric summary, Operation: {Operation}, " +
                "AvgDuration: {AvgDurationMs}ms, SuccessRate: {SuccessRate}%, " +
                "SampleCount: {SampleCount}, EventId: {EventId}",
                operation,
                metric.AverageDuration,
                metric.SuccessRate,
                metric.SampleCount,
                LogEvents.PerformanceMetricRecorded
            );

            metric.Reset();
        }

        if (durationMs > 5000)
        {
            _logger.LogWarning(
                "Performance threshold exceeded, Operation: {Operation}, " +
                "Duration: {DurationMs}ms, Threshold: {ThresholdMs}ms, EventId: {EventId}",
                operation,
                durationMs,
                5000,
                LogEvents.PerformanceThresholdExceeded
            );
        }
    }
}
```

## Error Logging

Always include full context when logging errors:

```csharp
try
{
    await ProcessRequestAsync(request);
}
catch (ValidationException ex)
{
    _logger.LogWarning(
        ex,
        "Validation failed, RequestId: {RequestId}, ValidationErrors: {@Errors}",
        requestId,
        ex.Errors
    );
}
catch (RateLimitException ex)
{
    _logger.LogWarning(
        ex,
        "Rate limit exceeded, RequestId: {RequestId}, RateLimit: {Limit}, " +
        "RetryAfter: {RetryAfterSeconds}s, EventId: {EventId}",
        requestId,
        ex.RateLimit,
        ex.RetryAfter.TotalSeconds,
        LogEvents.RateLimitExceeded
    );
}
catch (Exception ex)
{
    _logger.LogError(
        ex,
        "Unexpected error processing request, RequestId: {RequestId}, " +
        "RequestType: {RequestType}",
        requestId,
        request.GetType().Name
    );
}
```

## Security Considerations

### Never Log Sensitive Data

❌ **Don't log:**
- API keys
- Passwords
- User personal information
- Full credit card numbers
- Authentication tokens

✅ **Safe to log:**
- Request IDs
- User IDs (if non-sensitive)
- Operation names
- Durations
- Status codes
- Token counts (not token values)
- Error messages (sanitized)

```csharp
// Bad
_logger.LogDebug("API Key: {ApiKey}", apiKey);

// Good
_logger.LogDebug("API Key configured: {HasApiKey}", !string.IsNullOrEmpty(apiKey));
```

## Testing Logging

When writing tests, verify that appropriate logging occurs:

```csharp
[Fact]
public async Task ExecutePrompt_LogsStartAndCompletion()
{
    // Arrange
    var logger = new FakeLogger<PromptExecutionService>();
    var service = new PromptExecutionService(logger, _providers);

    // Act
    await service.ExecutePromptAsync(prompt);

    // Assert
    logger.VerifyLog(LogLevel.Information, LogEvents.PromptExecutionStarted);
    logger.VerifyLog(LogLevel.Information, LogEvents.PromptExecutionCompleted);
}
```
