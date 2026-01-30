namespace PromptLab.Core.Domain.Entities;

/// <summary>
/// Represents a conversation session containing multiple prompts and responses
/// </summary>
public class Conversation
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
