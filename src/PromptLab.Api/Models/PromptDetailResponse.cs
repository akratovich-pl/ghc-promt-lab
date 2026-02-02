namespace PromptLab.Api.Models;

/// <summary>
/// Detailed response for a prompt including the original prompt data
/// </summary>
public class PromptDetailResponse
{
    /// <summary>
    /// Unique identifier for the prompt
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The conversation this prompt belongs to
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// The user's original prompt text
    /// </summary>
    public string UserPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Optional context included with the prompt
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Context file ID if used
    /// </summary>
    public Guid? ContextFileId { get; set; }

    /// <summary>
    /// Estimated tokens for the prompt
    /// </summary>
    public int EstimatedTokens { get; set; }

    /// <summary>
    /// Actual tokens used
    /// </summary>
    public int ActualTokens { get; set; }

    /// <summary>
    /// When the prompt was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The AI response(s) to this prompt
    /// </summary>
    public List<ResponseDetail> Responses { get; set; } = new();
}

/// <summary>
/// Details of a single AI response
/// </summary>
public class ResponseDetail
{
    /// <summary>
    /// Unique identifier for the response
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// AI provider used
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Model name
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Response content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Tokens consumed
    /// </summary>
    public int Tokens { get; set; }

    /// <summary>
    /// Cost in USD
    /// </summary>
    public decimal Cost { get; set; }

    /// <summary>
    /// Response latency in milliseconds
    /// </summary>
    public int LatencyMs { get; set; }

    /// <summary>
    /// When the response was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
