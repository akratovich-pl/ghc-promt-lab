namespace PromptLab.Infrastructure.Configuration;

/// <summary>
/// Configuration for LLM providers
/// </summary>
public class LlmProviderConfig
{
    public const string SectionName = "LlmProviders";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Google Gemini specific configuration
/// </summary>
public class GoogleGeminiConfig : LlmProviderConfig
{
    public const string ConfigSectionName = "Providers:Google";

    /// <summary>
    /// API version (default: v1)
    /// </summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Cost per 1K input tokens in USD
    /// </summary>
    public decimal InputTokenCostPer1K { get; set; } = 0.00025m;

    /// <summary>
    /// Cost per 1K output tokens in USD
    /// </summary>
    public decimal OutputTokenCostPer1K { get; set; } = 0.0005m;
}
