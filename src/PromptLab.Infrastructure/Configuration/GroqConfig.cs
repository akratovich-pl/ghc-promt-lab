namespace PromptLab.Infrastructure.Configuration;

/// <summary>
/// Groq specific configuration
/// </summary>
public class GroqConfig : LlmProviderConfig
{
    public const string ConfigSectionName = "Providers:Groq";

    /// <summary>
    /// API version (default: v1)
    /// </summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Cost per 1K input tokens in USD
    /// </summary>
    public decimal InputTokenCostPer1K { get; set; } = 0.0m;

    /// <summary>
    /// Cost per 1K output tokens in USD
    /// </summary>
    public decimal OutputTokenCostPer1K { get; set; } = 0.0m;
}
