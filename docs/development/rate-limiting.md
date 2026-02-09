# Rate Limiting Configuration

## Overview

The PromptLab API implements a rate limiting system to prevent API quota exhaustion and protect against abuse. The rate limiting is implemented using an in-memory sliding window algorithm that tracks requests per client IP address.

## Features

- **Sliding Window Algorithm**: Accurate rate limiting that prevents request bursts
- **Configurable Limits**: Separate limits for per-minute and per-hour requests
- **Thread-Safe**: Concurrent request handling without race conditions
- **Health Check Bypass**: Health check endpoints bypass rate limiting
- **Informative Headers**: Returns rate limit information in response headers
- **Proper HTTP Status**: Returns 429 Too Many Requests when limit exceeded

## Configuration

Rate limiting is configured in `appsettings.json`:

```json
{
  "RateLimiting": {
    "RequestsPerMinute": 60,
    "RequestsPerHour": 1000,
    "Enabled": true
  }
}
```

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `RequestsPerMinute` | int | 60 | Maximum requests allowed per minute per client |
| `RequestsPerHour` | int | 1000 | Maximum requests allowed per hour per client |
| `Enabled` | bool | true | Whether rate limiting is enabled |

## How It Works

1. **Client Identification**: Clients are identified by their IP address, with support for proxies via `X-Forwarded-For` and `X-Real-IP` headers
2. **Request Tracking**: Each request timestamp is stored in memory with automatic expiration
3. **Limit Enforcement**: Both per-minute and per-hour limits are checked; the most restrictive applies
4. **Response Headers**: All responses include rate limit information

## Response Headers

Every API response includes the following headers:

| Header | Description | Example |
|--------|-------------|---------|
| `X-RateLimit-Limit-Minute` | Requests allowed per minute | `60` |
| `X-RateLimit-Limit-Hour` | Requests allowed per hour | `1000` |
| `X-RateLimit-Remaining` | Requests remaining in current window | `45` |
| `Retry-After` | Seconds to wait before retrying (when rate limited) | `60` |

## HTTP Status Codes

- **200-299**: Request successful, rate limit not exceeded
- **429 Too Many Requests**: Rate limit exceeded, includes `Retry-After` header

## Rate Limit Response Example

When rate limit is exceeded, the API returns:

```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json
Retry-After: 60
X-RateLimit-Limit-Minute: 60
X-RateLimit-Limit-Hour: 1000
X-RateLimit-Remaining: 0

{
  "error": "Too Many Requests",
  "message": "Rate limit exceeded. Please try again later.",
  "retryAfter": 60
}
```

## Bypassed Endpoints

The following endpoints bypass rate limiting:

- `/api/health` - Health check endpoint

## Testing Rate Limits

To test rate limiting in your environment:

1. **Set Low Limits**: Configure low limits in `appsettings.Development.json`:
   ```json
   {
     "RateLimiting": {
       "RequestsPerMinute": 5,
       "RequestsPerHour": 20,
       "Enabled": true
     }
   }
   ```

2. **Make Multiple Requests**: Use curl or any HTTP client:
   ```bash
   # Make 10 rapid requests
   for i in {1..10}; do
     curl -i http://localhost:5000/api/test
   done
   ```

3. **Check Headers**: Monitor the `X-RateLimit-Remaining` header to see remaining requests

## Disabling Rate Limiting

To disable rate limiting (not recommended for production):

```json
{
  "RateLimiting": {
    "Enabled": false
  }
}
```

## Architecture

### Components

1. **IRateLimitService**: Interface defining rate limiting operations
   - `CheckRateLimitAsync()`: Checks if request is allowed
   - `RecordRequestAsync()`: Records request timestamp
   - `GetRemainingRequestsAsync()`: Gets remaining requests count

2. **InMemoryRateLimitService**: Implementation using `MemoryCache`
   - Stores request timestamps with automatic expiration
   - Uses `SemaphoreSlim` for thread-safe operations
   - Implements sliding window algorithm

3. **RateLimitMiddleware**: ASP.NET Core middleware
   - Intercepts all requests (except bypassed endpoints)
   - Adds rate limit headers to responses
   - Returns 429 status when limit exceeded

### Service Registration

Services are registered in `Program.cs`:

```csharp
// Add Memory Cache
builder.Services.AddMemoryCache();

// Configure Rate Limiting Options
builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection(RateLimitingOptions.SectionName));

// Register Rate Limit Service as Singleton
builder.Services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();

// Add Middleware to pipeline
app.UseMiddleware<RateLimitMiddleware>();
```

## Future Enhancements (Phase 2)

The following enhancements are planned for Phase 2:

1. **Distributed Cache**: Migrate to Redis for multi-instance deployments
2. **Per-User Limits**: Different limits based on authenticated users
3. **API Key Tiers**: Varying limits based on subscription level
4. **Custom Rate Limit Rules**: Fine-grained control per endpoint
5. **Rate Limit Analytics**: Track and report rate limit metrics

## Troubleshooting

### Rate Limit Not Working

1. **Check Configuration**: Verify `Enabled` is set to `true`
2. **Check Middleware Order**: Ensure middleware is registered before controllers
3. **Check Logs**: Look for rate limit warnings in application logs

### Different Clients Share Limits

This occurs when all requests come from the same IP (e.g., behind a proxy):

1. **Configure Proxy Headers**: Ensure your reverse proxy sets `X-Forwarded-For`
2. **Use Authentication**: Implement per-user rate limiting (Phase 2)

### Memory Usage Concerns

The in-memory implementation stores timestamps for each client. For high-traffic scenarios:

1. **Monitor Memory**: Check application memory usage
2. **Adjust Limits**: Reduce time windows if needed
3. **Plan for Phase 2**: Migrate to Redis for distributed caching

## Testing

The rate limiting implementation includes comprehensive tests:

### Unit Tests (9 tests)
- Service behavior with enabled/disabled state
- Request recording and remaining count
- Sliding window algorithm correctness
- Thread safety with concurrent requests
- Multiple client isolation

### Integration Tests (5 tests)
- Middleware behavior with rate limits
- HTTP status codes and headers
- Health check endpoint bypass
- X-Forwarded-For header handling
- Disabled state behavior

Run tests with:
```bash
dotnet test
```

## Security Considerations

1. **DOS Protection**: Rate limiting helps prevent denial-of-service attacks
2. **Quota Management**: Prevents single clients from consuming all API resources
3. **Fair Usage**: Ensures API availability for all users
4. **No Authentication Required**: Works with anonymous clients via IP address

## Performance

- **Memory Cache**: Fast in-memory operations with O(1) lookups
- **Thread-Safe**: Uses efficient locking with `SemaphoreSlim`
- **Auto-Cleanup**: Expired entries automatically removed by cache
- **Minimal Overhead**: Negligible impact on request processing time
