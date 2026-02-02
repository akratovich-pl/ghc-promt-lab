using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PromptLab.Core.Configuration;
using PromptLab.Infrastructure.Services;

namespace PromptLab.Tests.Services;

public class InMemoryRateLimitServiceTests
{
    private readonly IMemoryCache _cache;
    private readonly RateLimitingOptions _options;

    public InMemoryRateLimitServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _options = new RateLimitingOptions
        {
            RequestsPerMinute = 5,
            RequestsPerHour = 10,
            Enabled = true
        };
    }

    [Fact]
    public async Task CheckRateLimitAsync_WhenDisabled_ReturnsTrue()
    {
        // Arrange
        var disabledOptions = new RateLimitingOptions { Enabled = false };
        var service = new InMemoryRateLimitService(_cache, Options.Create(disabledOptions));

        // Act
        var result = await service.CheckRateLimitAsync("test-key");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckRateLimitAsync_WithinLimit_ReturnsTrue()
    {
        // Arrange
        var service = new InMemoryRateLimitService(_cache, Options.Create(_options));
        var key = "test-key";

        // Act
        var result = await service.CheckRateLimitAsync(key);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RecordRequestAsync_IncreasesRequestCount()
    {
        // Arrange
        var service = new InMemoryRateLimitService(_cache, Options.Create(_options));
        var key = "test-key";

        // Act
        await service.RecordRequestAsync(key);
        var remaining = await service.GetRemainingRequestsAsync(key);

        // Assert
        Assert.Equal(4, remaining); // 5 limit - 1 recorded = 4 remaining
    }

    [Fact]
    public async Task CheckRateLimitAsync_ExceedsPerMinuteLimit_ReturnsFalse()
    {
        // Arrange
        var service = new InMemoryRateLimitService(_cache, Options.Create(_options));
        var key = "test-key";

        // Act - Record 5 requests (at the limit)
        for (int i = 0; i < 5; i++)
        {
            await service.RecordRequestAsync(key);
        }

        var result = await service.CheckRateLimitAsync(key);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetRemainingRequestsAsync_ReturnsCorrectCount()
    {
        // Arrange
        var service = new InMemoryRateLimitService(_cache, Options.Create(_options));
        var key = "test-key";

        // Act
        await service.RecordRequestAsync(key);
        await service.RecordRequestAsync(key);
        var remaining = await service.GetRemainingRequestsAsync(key);

        // Assert
        Assert.Equal(3, remaining); // 5 limit - 2 recorded = 3 remaining
    }

    [Fact]
    public async Task GetRemainingRequestsAsync_WhenDisabled_ReturnsMaxValue()
    {
        // Arrange
        var disabledOptions = new RateLimitingOptions { Enabled = false };
        var service = new InMemoryRateLimitService(_cache, Options.Create(disabledOptions));

        // Act
        var remaining = await service.GetRemainingRequestsAsync("test-key");

        // Assert
        Assert.Equal(int.MaxValue, remaining);
    }

    [Fact]
    public async Task RateLimit_UsesMinimumOfBothLimits()
    {
        // Arrange
        var service = new InMemoryRateLimitService(_cache, Options.Create(_options));
        var key = "test-key";

        // Act - Record 5 requests (at per-minute limit but under per-hour limit)
        for (int i = 0; i < 5; i++)
        {
            await service.RecordRequestAsync(key);
        }

        var remaining = await service.GetRemainingRequestsAsync(key);

        // Assert
        Assert.Equal(0, remaining); // Limited by per-minute (0 remaining) not per-hour (5 remaining)
    }

    [Fact]
    public async Task RateLimit_ThreadSafe_MultipleRequests()
    {
        // Arrange
        var service = new InMemoryRateLimitService(_cache, Options.Create(_options));
        var key = "test-key";
        var tasks = new List<Task>();

        // Act - Make concurrent requests
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(service.RecordRequestAsync(key));
        }
        await Task.WhenAll(tasks);

        var remaining = await service.GetRemainingRequestsAsync(key);

        // Assert
        Assert.Equal(2, remaining); // 5 limit - 3 recorded = 2 remaining
    }

    [Fact]
    public async Task DifferentKeys_HaveSeparateLimits()
    {
        // Arrange
        var service = new InMemoryRateLimitService(_cache, Options.Create(_options));
        var key1 = "test-key-1";
        var key2 = "test-key-2";

        // Act
        await service.RecordRequestAsync(key1);
        await service.RecordRequestAsync(key1);
        await service.RecordRequestAsync(key2);

        var remaining1 = await service.GetRemainingRequestsAsync(key1);
        var remaining2 = await service.GetRemainingRequestsAsync(key2);

        // Assert
        Assert.Equal(3, remaining1); // 5 - 2 = 3
        Assert.Equal(4, remaining2); // 5 - 1 = 4
    }
}
