using PromptLab.Core.Services.Interfaces;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Stub implementation of rate limit service - to be replaced with actual implementation
/// </summary>
public class StubRateLimitService : IRateLimitService
{
    public Task<RateLimitResult> CheckRateLimitAsync(string userId, CancellationToken cancellationToken = default)
    {
        // For now, always allow requests
        return Task.FromResult(new RateLimitResult
        {
            IsAllowed = true
        });
    }

    public Task RecordUsageAsync(string userId, int tokensUsed, CancellationToken cancellationToken = default)
    {
        // No-op for stub implementation
        return Task.CompletedTask;
    }
}
