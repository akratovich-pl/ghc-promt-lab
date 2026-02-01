namespace PromptLab.Core.Interfaces;

/// <summary>
/// Interface for LLM provider configuration service
/// </summary>
public interface ILlmProviderConfig
{
    /// <summary>
    /// Gets the API key for the specified provider
    /// </summary>
    /// <param name="providerName">Name of the provider (e.g., "GoogleGemini")</param>
    /// <returns>API key if found, null otherwise</returns>
    string? GetApiKey(string providerName);

    /// <summary>
    /// Gets the base URL for the specified provider
    /// </summary>
    /// <param name="providerName">Name of the provider</param>
    /// <returns>Base URL if found, null otherwise</returns>
    string? GetBaseUrl(string providerName);

    /// <summary>
    /// Gets the default model for the specified provider
    /// </summary>
    /// <param name="providerName">Name of the provider</param>
    /// <returns>Default model name if found, null otherwise</returns>
    string? GetDefaultModel(string providerName);

    /// <summary>
    /// Gets the max tokens for the specified provider
    /// </summary>
    /// <param name="providerName">Name of the provider</param>
    /// <returns>Max tokens if found, 0 otherwise</returns>
    int GetMaxTokens(string providerName);

    /// <summary>
    /// Gets the temperature for the specified provider
    /// </summary>
    /// <param name="providerName">Name of the provider</param>
    /// <returns>Temperature if found, 0.0 otherwise</returns>
    double GetTemperature(string providerName);

    /// <summary>
    /// Checks if the specified provider is enabled
    /// </summary>
    /// <param name="providerName">Name of the provider</param>
    /// <returns>True if enabled, false otherwise</returns>
    bool IsProviderEnabled(string providerName);
}
