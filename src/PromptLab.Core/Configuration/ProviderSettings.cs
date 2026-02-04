namespace PromptLab.Core.Configuration;

/// <summary>
/// Configuration for a single AI provider
/// </summary>
public class ProviderSettings
{
    /// <summary>
    /// Provider name (e.g., "Google", "Groq")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the provider's API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API version (e.g., "v1")
    /// </summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Default model to use for this provider
    /// </summary>
    public string DefaultModel { get; set; } = string.Empty;

    /// <summary>
    /// Collection of models available from this provider
    /// </summary>
    public List<ModelSettings> Models { get; set; } = new();
}

/// <summary>
/// Configuration for a single AI model
/// </summary>
public class ModelSettings
{
    /// <summary>
    /// Model identifier (e.g., "gemini-1.5-flash")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable model name (e.g., "Gemini 1.5 Flash")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Maximum tokens supported by the model
    /// </summary>
    public int MaxTokens { get; set; }

    /// <summary>
    /// Cost per 1000 input tokens in USD
    /// </summary>
    public decimal InputCostPer1kTokens { get; set; }

    /// <summary>
    /// Cost per 1000 output tokens in USD
    /// </summary>
    public decimal OutputCostPer1kTokens { get; set; }
}

/// <summary>
/// Root configuration for all providers
/// </summary>
public class ProvidersConfiguration
{
    public const string SectionName = "Providers";

    /// <summary>
    /// Dictionary of providers by name
    /// </summary>
    public Dictionary<string, ProviderSettings> Items { get; set; } = new();
}
