using PromptLab.Core.DTOs;

namespace PromptLab.Core.Services;

/// <summary>
/// Service for managing AI providers and models
/// </summary>
public interface IProviderService
{
    /// <summary>
    /// Gets all available providers
    /// </summary>
    Task<List<ProviderInfo>> GetProvidersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the health status of a specific provider
    /// </summary>
    Task<ProviderStatus> GetProviderStatusAsync(
        string providerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available models for a provider
    /// </summary>
    Task<List<ModelInfo>> GetProviderModelsAsync(
        string providerName,
        CancellationToken cancellationToken = default);
}
