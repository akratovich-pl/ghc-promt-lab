using PromptLab.Core.Domain.Enums;

namespace PromptLab.Core.Domain.Entities;

/// <summary>
/// Represents an AI provider response to a prompt
/// </summary>
public class Response
{
    public Guid Id { get; set; }
    public Guid PromptId { get; set; }
    public AiProvider Provider { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Tokens { get; set; }
    public decimal Cost { get; set; }
    public int LatencyMs { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public Prompt Prompt { get; set; } = null!;
}
