using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PromptLab.Core.Configuration;
using PromptLab.Core.RateLimiter;

namespace PromptLab.Infrastructure.RateLimiter;

/// <summary>
/// In-memory implementation of rate limiting using sliding window algorithm
/// </summary>
public class InMemoryRateLimiter : IRateLimiter
{
    private readonly IMemoryCache _cache;
    private readonly RateLimitingOptions _options;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public InMemoryRateLimiter(IMemoryCache cache, IOptions<RateLimitingOptions> options)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<bool> CheckRateLimitAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return true;

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var minuteKey = GetMinuteKey(key);
            var hourKey = GetHourKey(key);

            var minuteRequests = GetRequestTimestamps(minuteKey);
            var hourRequests = GetRequestTimestamps(hourKey);

            var now = DateTimeOffset.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);
            var oneHourAgo = now.AddHours(-1);

            // Remove expired timestamps
            minuteRequests.RemoveAll(t => t < oneMinuteAgo);
            hourRequests.RemoveAll(t => t < oneHourAgo);

            // Check limits
            if (minuteRequests.Count >= _options.RequestsPerMinute)
                return false;

            if (hourRequests.Count >= _options.RequestsPerHour)
                return false;

            return true;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task RecordRequestAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return;

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var minuteKey = GetMinuteKey(key);
            var hourKey = GetHourKey(key);
            var now = DateTimeOffset.UtcNow;

            var minuteRequests = GetRequestTimestamps(minuteKey);
            var hourRequests = GetRequestTimestamps(hourKey);

            minuteRequests.Add(now);
            hourRequests.Add(now);

            // Store with appropriate expiration
            _cache.Set(minuteKey, minuteRequests, TimeSpan.FromMinutes(1));
            _cache.Set(hourKey, hourRequests, TimeSpan.FromHours(1));
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<int> GetRemainingRequestsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return int.MaxValue;

        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            var minuteKey = GetMinuteKey(key);
            var hourKey = GetHourKey(key);

            var minuteRequests = GetRequestTimestamps(minuteKey);
            var hourRequests = GetRequestTimestamps(hourKey);

            var now = DateTimeOffset.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);
            var oneHourAgo = now.AddHours(-1);

            // Remove expired timestamps
            minuteRequests.RemoveAll(t => t < oneMinuteAgo);
            hourRequests.RemoveAll(t => t < oneHourAgo);

            var remainingPerMinute = _options.RequestsPerMinute - minuteRequests.Count;
            var remainingPerHour = _options.RequestsPerHour - hourRequests.Count;

            // Return the most restrictive limit
            return Math.Max(0, Math.Min(remainingPerMinute, remainingPerHour));
        }
        finally
        {
            semaphore.Release();
        }
    }

    private List<DateTimeOffset> GetRequestTimestamps(string key)
    {
        return _cache.GetOrCreate(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
            return new List<DateTimeOffset>();
        }) ?? new List<DateTimeOffset>();
    }

    private static string GetMinuteKey(string key) => $"{key}:minute";
    private static string GetHourKey(string key) => $"{key}:hour";
}
