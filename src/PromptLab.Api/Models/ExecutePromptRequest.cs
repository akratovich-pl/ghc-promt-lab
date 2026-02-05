using System.ComponentModel.DataAnnotations;
using PromptLab.Core.Domain.Enums;

namespace PromptLab.Api.Models;

/// <summary>
/// Request to execute a prompt
/// </summary>
public class ExecutePromptRequest
{
    /// <summary>
    /// The AI provider to use (required)
    /// </summary>
    [Required(ErrorMessage = "Provider is required")]
    public AiProvider Provider { get; set; }

    /// <summary>
    /// The user's prompt text (required, max 50,000 characters)
    /// </summary>
    [Required(ErrorMessage = "Prompt is required")]
    [StringLength(50000, ErrorMessage = "Prompt cannot exceed 50,000 characters")]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Optional system prompt to guide the AI's behavior
    /// </summary>
    [StringLength(10000, ErrorMessage = "SystemPrompt cannot exceed 10,000 characters")]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Optional conversation ID to continue an existing conversation
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// Optional list of context file IDs to include with the prompt
    /// </summary>
    public List<Guid>? ContextFileIds { get; set; }

    /// <summary>
    /// Optional model name (defaults to provider's default model)
    /// </summary>
    [StringLength(100, ErrorMessage = "Model name cannot exceed 100 characters")]
    public string? Model { get; set; }

    /// <summary>
    /// Maximum tokens to generate (1-8192)
    /// </summary>
    [Range(1, 8192, ErrorMessage = "MaxTokens must be between 1 and 8192")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Temperature for response randomness (0.0-2.0)
    /// </summary>
    [Range(0.0, 2.0, ErrorMessage = "Temperature must be between 0.0 and 2.0")]
    public double? Temperature { get; set; }
}
