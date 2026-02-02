using System.ComponentModel.DataAnnotations;

namespace PromptLab.Core.DTOs;

/// <summary>
/// Represents the response from an LLM provider after text generation.
/// </summary>
public class LlmResponse
{
    /// <summary>
    /// The generated text content from the LLM
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The model used for generation
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Number of tokens in the input prompt
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// Number of tokens in the generated output
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Estimated cost of the API call in USD
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// Request latency in milliseconds
    /// </summary>
    public long LatencyMs { get; set; }

    /// <summary>
    /// Reason why generation finished (e.g., "STOP", "MAX_TOKENS", "SAFETY")
    /// </summary>
    public string? FinishReason { get; set; }

    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the request failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
