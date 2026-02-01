using PromptLab.Core.Domain.Enums;

namespace PromptLab.Core.Services.Interfaces;

/// <summary>
/// Interface for LLM provider implementations
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Gets the provider type
    /// </summary>
    AiProvider Provider { get; }

    /// <summary>
    /// Gets available models for this provider
    /// </summary>
    Task<IEnumerable<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the provider is available and configured
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a response from the LLM
    /// </summary>
    Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to an LLM provider
/// </summary>
public class LlmRequest
{
    public string Model { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public List<ConversationMessage> ConversationHistory { get; set; } = new();
    public int? MaxTokens { get; set; }
    public decimal? Temperature { get; set; }
}

/// <summary>
/// Response from an LLM provider
/// </summary>
public class LlmResponse
{
    public string Content { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public decimal Cost { get; set; }
    public int LatencyMs { get; set; }
    public string Model { get; set; } = string.Empty;
}

/// <summary>
/// Represents a message in conversation history
/// </summary>
public class ConversationMessage
{
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
}
