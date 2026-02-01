namespace PromptLab.Core.Domain.Interfaces;

/// <summary>
/// Configuration interface for LLM providers
/// </summary>
public interface ILlmProviderConfig
{
    /// <summary>
    /// API key for the LLM provider
    /// </summary>
    string ApiKey { get; }

    /// <summary>
    /// Base URL for the API endpoint
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// Default model to use for requests
    /// </summary>
    string Model { get; }

    /// <summary>
    /// Maximum number of retries for failed requests
    /// </summary>
    int MaxRetries { get; }

    /// <summary>
    /// Timeout in seconds for API requests
    /// </summary>
    int TimeoutSeconds { get; }
}
