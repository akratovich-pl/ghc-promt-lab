using PromptLab.Core.Domain.Enums;

namespace PromptLab.Core.DTOs;

/// <summary>
/// Result of executing a prompt
/// </summary>
public class PromptExecutionResult
{
    public Guid PromptId { get; set; }
    public Guid ResponseId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public decimal Cost { get; set; }
    public int LatencyMs { get; set; }
    public string Model { get; set; } = string.Empty;
    public AiProvider Provider { get; set; }
    public DateTime CreatedAt { get; set; }
}
