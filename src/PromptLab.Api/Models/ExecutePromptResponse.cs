namespace PromptLab.Api.Models;

/// <summary>
/// Response from executing a prompt
/// </summary>
public class ExecutePromptResponse
{
    /// <summary>
    /// Unique identifier for the prompt
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The AI-generated response content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Number of input tokens consumed
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// Number of output tokens generated
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// Cost in USD for this request
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// Response latency in milliseconds
    /// </summary>
    public int LatencyMs { get; set; }

    /// <summary>
    /// Model used for generation
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the response was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
