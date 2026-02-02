using System.ComponentModel.DataAnnotations;

namespace PromptLab.Core.DTOs;

/// <summary>
/// Represents a request to estimate token count for a given text.
/// </summary>
/// <param name="Text">The text to estimate tokens for. Must not be empty.</param>
/// <param name="Model">The model to use for token estimation. Must not be empty.</param>
public record TokenEstimateRequest(
    [Required][MinLength(1)] string Text,
    [Required][MinLength(1)] string Model
);
