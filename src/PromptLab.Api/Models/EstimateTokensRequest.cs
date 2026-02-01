using System.ComponentModel.DataAnnotations;

namespace PromptLab.Api.Models;

/// <summary>
/// Request to estimate tokens for a prompt
/// </summary>
public class EstimateTokensRequest
{
    /// <summary>
    /// The prompt text to estimate tokens for (required, max 50,000 characters)
    /// </summary>
    [Required(ErrorMessage = "Prompt is required")]
    [StringLength(50000, ErrorMessage = "Prompt cannot exceed 50,000 characters")]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Optional model name to use for estimation (affects token counting)
    /// </summary>
    [StringLength(100, ErrorMessage = "Model name cannot exceed 100 characters")]
    public string? Model { get; set; }
}
