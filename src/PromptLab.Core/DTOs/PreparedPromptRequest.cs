using PromptLab.Core.Domain.Enums;

namespace PromptLab.Core.DTOs;

/// <summary>
/// Contains all data needed for LLM execution after request preparation
/// </summary>
public class PreparedPromptRequest
{
    /// <summary>
    /// The AI provider to use
    /// </summary>
    public required AiProvider Provider { get; set; }

    /// <summary>
    /// The built LLM request ready for provider consumption
    /// </summary>
    public required LlmRequest LlmRequest { get; set; }

    /// <summary>
    /// Context file ID if enrichment occurred, null otherwise
    /// </summary>
    public Guid? ContextFileId { get; set; }

    /// <summary>
    /// Loaded conversation history (empty list if none)
    /// </summary>
    public required List<ConversationMessage> ConversationHistory { get; set; }

    /// <summary>
    /// User ID for traceability
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Model name being used
    /// </summary>
    public required string Model { get; set; }
}
