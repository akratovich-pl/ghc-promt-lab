namespace PromptLab.Core.DTOs;

/// <summary>
/// Represents a message in a conversation history
/// </summary>
public class ConversationMessage
{
    /// <summary>
    /// The role of the message sender (e.g., "user", "assistant", "system")
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// The content of the message
    /// </summary>
    public required string Content { get; set; }
}
