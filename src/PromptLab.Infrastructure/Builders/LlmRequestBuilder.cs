using PromptLab.Core.Builders;
using PromptLab.Core.DTOs;

namespace PromptLab.Infrastructure.Builders;

/// <summary>
/// Builds LLM requests from various parameters
/// </summary>
public class LlmRequestBuilder : ILlmRequestBuilder
{
    public LlmRequest BuildRequest(
        string prompt,
        string? systemPrompt,
        string model,
        List<ConversationMessage>? conversationHistory,
        int? maxTokens,
        double? temperature)
    {
        return new LlmRequest
        {
            Prompt = prompt,
            SystemPrompt = systemPrompt,
            Model = model,
            ConversationHistory = conversationHistory ?? new List<ConversationMessage>(),
            MaxTokens = maxTokens,
            Temperature = temperature.HasValue ? (decimal?)temperature.Value : null
        };
    }
}
