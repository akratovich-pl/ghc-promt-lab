# Logging Quick Reference

## Basic Logging Patterns

### Information Logging
```csharp
_logger.LogInformation("Operation started, Property: {Value}", propertyValue);
```

### Error Logging
```csharp
_logger.LogError(ex, "Operation failed, Context: {Context}", contextInfo);
```

### With Event IDs
```csharp
_logger.LogInformation(
    "Operation completed, EventId: {EventId}, Property: {Value}",
    LogEvents.OperationCompleted,
    propertyValue
);
```

### With Correlation ID Scope
```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId,
    ["UserId"] = userId
}))
{
    _logger.LogInformation("Processing request...");
}
```

### Measuring Duration
```csharp
var sw = Stopwatch.StartNew();
try
{
    // Your code
    sw.Stop();
    _logger.LogInformation("Operation completed, Duration: {DurationMs}ms", sw.ElapsedMilliseconds);
}
catch (Exception ex)
{
    sw.Stop();
    _logger.LogError(ex, "Operation failed, Duration: {DurationMs}ms", sw.ElapsedMilliseconds);
}
```

## Event ID Reference

| Event ID | Name | Category | Usage |
|----------|------|----------|-------|
| 1000 | PromptExecutionStarted | Execution | Log when prompt execution begins |
| 1001 | PromptExecutionCompleted | Execution | Log when prompt execution succeeds |
| 1002 | PromptExecutionFailed | Execution | Log when prompt execution fails |
| 2000 | ApiRequestSent | API | Log before sending external API request |
| 2001 | ApiResponseReceived | API | Log when API response received |
| 2002 | ApiRequestFailed | API | Log when API request fails |
| 2003 | ApiRetryAttempt | API | Log retry attempts |
| 3000 | RateLimitExceeded | Rate Limiting | Log rate limit violations |
| 3001 | RateLimitWarning | Rate Limiting | Log approaching rate limit |
| 4000 | ConfigurationLoaded | Configuration | Log successful config load |
| 4001 | ConfigurationError | Configuration | Log configuration errors |
| 5000 | HealthCheckStarted | Health | Log health check start |
| 5001 | HealthCheckCompleted | Health | Log health check success |
| 5002 | HealthCheckFailed | Health | Log health check failure |
| 6000 | DatabaseConnectionOpened | Database | Log DB connection |
| 6001 | DatabaseConnectionFailed | Database | Log DB connection failure |
| 6002 | DatabaseQueryExecuted | Database | Log query execution |
| 7000 | CacheHit | Cache | Log cache hit |
| 7001 | CacheMiss | Cache | Log cache miss |
| 7002 | CacheError | Cache | Log cache errors |
| 8000 | PerformanceMetricRecorded | Performance | Log performance metrics |
| 8001 | PerformanceThresholdExceeded | Performance | Log slow operations |

## Log Levels

- **Trace** (0): Very detailed diagnostic logs
- **Debug** (1): Debugging information, development only
- **Information** (2): General operational logs
- **Warning** (3): Abnormal/unexpected events that don't stop execution
- **Error** (4): Errors and exceptions
- **Critical** (5): Critical failures requiring immediate attention

## Configuration

### appsettings.json (Production)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/promptlab-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### appsettings.Development.json
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    },
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

## Common Anti-Patterns

### ❌ Don't
```csharp
// String interpolation
_logger.LogInformation($"User {userId} did something");

// Logging sensitive data
_logger.LogDebug("API Key: {ApiKey}", apiKey);

// Catching without logging
catch (Exception ex) { return null; }

// Too much detail at Information level
_logger.LogInformation("Processing... step 1/100");
```

### ✅ Do
```csharp
// Structured logging
_logger.LogInformation("User action, UserId: {UserId}, Action: {Action}", userId, action);

// Mask sensitive data
_logger.LogDebug("API Key configured: {HasKey}", !string.IsNullOrEmpty(apiKey));

// Always log errors
catch (Exception ex) 
{
    _logger.LogError(ex, "Operation failed, Context: {Context}", context);
    throw;
}

// Appropriate log levels
_logger.LogDebug("Processing step {Step}/{Total}", step, total);
```

## Useful Queries for Log Analysis

### Find errors in last hour
```bash
grep '"Level":"ERR"' logs/promptlab-$(date +%Y%m%d).log | tail -100
```

### Find slow operations
```bash
grep 'Duration.*ms' logs/promptlab-*.log | grep -E 'Duration: [0-9]{4,}'
```

### Count errors by type
```bash
grep '"Level":"ERR"' logs/promptlab-*.log | jq -r '.Exception' | sort | uniq -c
```

### Find specific correlation ID
```bash
grep 'CorrelationId.*abc123' logs/promptlab-*.log
```

## Testing

```csharp
public class ServiceTests
{
    [Fact]
    public async Task Operation_LogsCorrectly()
    {
        // Arrange
        var logger = new Mock<ILogger<MyService>>();
        var service = new MyService(logger.Object);

        // Act
        await service.DoSomethingAsync();

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("expected message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
```

## Performance Considerations

- Use appropriate log levels to control verbosity
- Leverage structured logging for efficient querying
- Use scopes to add context without repeating properties
- Consider async logging for high-throughput scenarios
- Rotate log files to manage disk space
- Use log aggregation services for production monitoring
