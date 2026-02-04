using PromptLab.Core.DTOs;

namespace PromptLab.Core.Builders;

/// <summary>
/// Builder for constructing LLM requests from various inputs
/// </summary>
public interface ILlmRequestBuilder
{
    /// <summary>
    /// Builds an LLM request from the provided parameters
    /// </summary>
    /// <param name="prompt">The user prompt (possibly enriched with context)</param>
    /// <param name="systemPrompt">Optional system prompt</param>
    /// <param name="model">The model to use</param>
    /// <param name="conversationHistory">Optional conversation history</param>
    /// <param name="maxTokens">Optional max tokens</param>
    /// <param name="temperature">Optional temperature</param>
    /// <returns>A configured LLM request</returns>
    LlmRequest BuildRequest(
        string prompt,
        string? systemPrompt,
        string model,
        List<ConversationMessage>? conversationHistory,
        int? maxTokens,
        double? temperature);
}
