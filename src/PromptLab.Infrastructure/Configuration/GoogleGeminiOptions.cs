using System.ComponentModel.DataAnnotations;

namespace PromptLab.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Google Gemini provider
/// </summary>
public class GoogleGeminiOptions
{
    /// <summary>
    /// Whether the Google Gemini provider is enabled
    /// </summary>
    [Required]
    public bool Enabled { get; set; }

    /// <summary>
    /// Base URL for Google Gemini API
    /// </summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Default model to use for Google Gemini
    /// </summary>
    [Required]
    public string DefaultModel { get; set; } = string.Empty;

    /// <summary>
    /// Maximum tokens for requests
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int MaxTokens { get; set; }

    /// <summary>
    /// Temperature setting for model responses
    /// </summary>
    [Required]
    [Range(0.0, 2.0)]
    public double Temperature { get; set; }
}
