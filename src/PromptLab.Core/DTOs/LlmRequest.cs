using System.ComponentModel.DataAnnotations;

namespace PromptLab.Core.DTOs;

/// <summary>
/// Represents a request to an LLM provider for text generation.
/// </summary>
public class LlmRequest
{
    /// <summary>
    /// The user prompt to send to the LLM
    /// </summary>
    [Required]
    [MinLength(1)]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Optional system prompt to set behavior context
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// The specific model to use (e.g., "gemini-pro", "gpt-4")
    /// </summary>
    [Required]
    [MinLength(1)]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of tokens to generate in the response
    /// </summary>
    [Range(1, 100000)]
    public int? MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Controls randomness (0.0 = deterministic, 1.0 = creative)
    /// </summary>
    [Range(0.0, 2.0)]
    public decimal? Temperature { get; set; } = 0.7m;

    /// <summary>
    /// Optional conversation history for context
    /// </summary>
    public List<ConversationMessage>? ConversationHistory { get; set; }

    /// <summary>
    /// The user prompt (alternative property name for compatibility)
    /// </summary>
    public string UserPrompt
    {
        get => Prompt;
        set => Prompt = value;
    }

    /// <summary>
    /// System message (alternative property name for compatibility)
    /// </summary>
    public string? SystemMessage
    {
        get => SystemPrompt;
        set => SystemPrompt = value;
    }
}
