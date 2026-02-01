using System.ComponentModel.DataAnnotations;

namespace PromptLab.Core.DTOs;

/// <summary>
/// Represents the response containing estimated token count.
/// </summary>
/// <param name="TokenCount">The estimated number of tokens in the provided text.</param>
/// <param name="Model">The model used for token estimation. Must not be empty.</param>
public record TokenEstimateResponse(
    [Range(0, int.MaxValue)] int TokenCount,
    [Required][MinLength(1)] string Model
);
