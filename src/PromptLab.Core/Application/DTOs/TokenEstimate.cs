namespace PromptLab.Core.Application.DTOs;

/// <summary>
/// Result of estimating tokens for a prompt
/// </summary>
public class TokenEstimate
{
    public int TokenCount { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Model { get; set; } = string.Empty;
}
