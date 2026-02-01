using System.ComponentModel.DataAnnotations;

namespace PromptLab.Core.DTOs;

/// <summary>
/// Represents a request to an LLM provider for text generation.
/// </summary>
/// <param name="Prompt">The user prompt to send to the LLM. Must not be empty.</param>
/// <param name="SystemPrompt">Optional system prompt to set behavior context.</param>
/// <param name="Model">The specific model to use (e.g., "gemini-pro", "gpt-4").</param>
/// <param name="MaxTokens">Maximum number of tokens to generate in the response.</param>
/// <param name="Temperature">Controls randomness (0.0 = deterministic, 1.0 = creative).</param>
public record LlmRequest(
    [Required][MinLength(1)] string Prompt,
    string? SystemPrompt,
    [Required][MinLength(1)] string Model,
    [Range(1, 100000)] int MaxTokens = 2048,
    [Range(0.0, 2.0)] double Temperature = 0.7
);
