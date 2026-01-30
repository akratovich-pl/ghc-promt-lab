namespace PromptLab.Core.Domain.Entities;

/// <summary>
/// Represents a single prompt with its context and metadata
/// </summary>
public class Prompt
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string UserPrompt { get; set; } = string.Empty;
    public string? Context { get; set; }
    public Guid? ContextFileId { get; set; }
    public int EstimatedTokens { get; set; }
    public int ActualTokens { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public ContextFile? ContextFile { get; set; }
    public ICollection<Response> Responses { get; set; } = new List<Response>();
}
