using System.ComponentModel.DataAnnotations;

namespace PromptLab.Core.DTOs;

/// <summary>
/// Represents a request to estimate token count for a given text.
/// </summary>
/// <param name="Text">The text to estimate tokens for.</param>
/// <param name="Model">The model to use for token estimation.</param>
public record TokenEstimateRequest(
    [Required] string Text,
    [Required] string Model
);
