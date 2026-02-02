namespace PromptLab.Core.Domain.DTOs;

/// <summary>
/// Response data from LLM providers
/// </summary>
public class LlmResponse
{
    /// <summary>
    /// The generated text content
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Model used for generation
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// Number of tokens in the prompt
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// Number of tokens in the completion
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total tokens used (prompt + completion)
    /// </summary>
    public int TotalTokens => PromptTokens + CompletionTokens;

    /// <summary>
    /// Estimated cost in USD
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// Latency in milliseconds
    /// </summary>
    public long LatencyMs { get; set; }

    /// <summary>
    /// Optional finish reason (e.g., "stop", "length", etc.)
    /// </summary>
    public string? FinishReason { get; set; }

    /// <summary>
    /// Indicates if the response was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if the request failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
