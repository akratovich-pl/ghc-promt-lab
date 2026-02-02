namespace PromptLab.Core.Domain.DTOs;

/// <summary>
/// Request data for LLM providers
/// </summary>
public class LlmRequest
{
    /// <summary>
    /// The prompt text to send to the LLM
    /// </summary>
    public required string Prompt { get; set; }

    /// <summary>
    /// Optional model override (uses default if not specified)
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Optional temperature parameter (0.0 to 1.0)
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// Optional maximum tokens to generate
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Optional system message/instructions
    /// </summary>
    public string? SystemMessage { get; set; }

    /// <summary>
    /// Whether to stream the response
    /// </summary>
    public bool Stream { get; set; } = false;
}
