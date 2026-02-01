using System.ComponentModel.DataAnnotations;

namespace PromptLab.Infrastructure.Configuration;

/// <summary>
/// Configuration options for all LLM providers
/// </summary>
public class LlmProvidersOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "LlmProviders";

    /// <summary>
    /// Google Gemini provider configuration
    /// </summary>
    [Required]
    public GoogleGeminiOptions GoogleGemini { get; set; } = new();
}
