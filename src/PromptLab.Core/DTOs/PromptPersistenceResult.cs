namespace PromptLab.Core.DTOs;

/// <summary>
/// Contains the result of prompt persistence including database IDs and metadata
/// </summary>
public class PromptPersistenceResult
{
    /// <summary>
    /// ID of the saved prompt entity
    /// </summary>
    public required Guid PromptId { get; set; }

    /// <summary>
    /// ID of the saved response entity
    /// </summary>
    public required Guid ResponseId { get; set; }

    /// <summary>
    /// Conversation ID (existing or newly created)
    /// </summary>
    public required Guid ConversationId { get; set; }

    /// <summary>
    /// Timestamp when the prompt was created
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// Indicates if a new conversation was created
    /// </summary>
    public required bool IsNewConversation { get; set; }
}
