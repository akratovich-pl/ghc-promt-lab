using System.ComponentModel.DataAnnotations;

namespace PromptLab.Core.DTOs;

/// <summary>
/// Represents the response from an LLM provider after text generation.
/// </summary>
/// <param name="Content">The generated text content from the LLM.</param>
/// <param name="InputTokens">Number of tokens in the input prompt.</param>
/// <param name="OutputTokens">Number of tokens in the generated output.</param>
/// <param name="Cost">Estimated cost of the API call in USD.</param>
/// <param name="LatencyMs">Request latency in milliseconds.</param>
/// <param name="Model">The actual model used for generation.</param>
public record LlmResponse(
    [Required] string Content,
    [Range(0, int.MaxValue)] int InputTokens,
    [Range(0, int.MaxValue)] int OutputTokens,
    [Range(0, double.MaxValue)] decimal Cost,
    [Range(0, int.MaxValue)] int LatencyMs,
    [Required] string Model
);
