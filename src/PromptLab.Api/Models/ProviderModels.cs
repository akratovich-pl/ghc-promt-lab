namespace PromptLab.Api.Models;

/// <summary>
/// Information about an AI provider
/// </summary>
public class ProviderInfoResponse
{
    /// <summary>
    /// Provider name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the provider is currently available
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// List of supported model names
    /// </summary>
    public List<string> SupportedModels { get; set; } = new();
}

/// <summary>
/// Health status of a provider
/// </summary>
public class ProviderStatusResponse
{
    /// <summary>
    /// Provider name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the provider is healthy and operational
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Error message if unhealthy
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// When the status was last checked
    /// </summary>
    public DateTime LastChecked { get; set; }
}

/// <summary>
/// Information about a specific model
/// </summary>
public class ModelInfoResponse
{
    /// <summary>
    /// Model identifier
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable model name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Provider that offers this model
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Maximum tokens this model supports
    /// </summary>
    public int MaxTokens { get; set; }

    /// <summary>
    /// Cost per 1,000 input tokens in USD
    /// </summary>
    public decimal InputCostPer1kTokens { get; set; }

    /// <summary>
    /// Cost per 1,000 output tokens in USD
    /// </summary>
    public decimal OutputCostPer1kTokens { get; set; }
}
