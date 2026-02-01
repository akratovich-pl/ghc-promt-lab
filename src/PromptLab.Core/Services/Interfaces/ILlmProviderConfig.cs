using PromptLab.Core.Domain.Enums;

namespace PromptLab.Core.Services.Interfaces;

/// <summary>
/// Defines the contract for retrieving LLM provider configuration.
/// Implementations should retrieve configuration from environment variables, configuration files, or secrets management.
/// </summary>
public interface ILlmProviderConfig
{
    /// <summary>
    /// Retrieves the API key for the specified LLM provider.
    /// </summary>
    /// <param name="provider">The AI provider to get the API key for.</param>
    /// <returns>The API key if configured; otherwise, null.</returns>
    string? GetApiKey(AiProvider provider);

    /// <summary>
    /// Retrieves the base URL for the specified LLM provider's API.
    /// </summary>
    /// <param name="provider">The AI provider to get the base URL for.</param>
    /// <returns>The base API URL if configured; otherwise, null.</returns>
    string? GetBaseUrl(AiProvider provider);

    /// <summary>
    /// Checks if the specified LLM provider is enabled in the configuration.
    /// </summary>
    /// <param name="provider">The AI provider to check.</param>
    /// <returns>True if the provider is enabled and configured; otherwise, false.</returns>
    bool IsProviderEnabled(AiProvider provider);
}
