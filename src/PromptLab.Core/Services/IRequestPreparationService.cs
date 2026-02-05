using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;

namespace PromptLab.Core.Services;

/// <summary>
/// Service for preparing LLM requests by validating, enriching, and building requests
/// </summary>
public interface IRequestPreparationService
{
    /// <summary>
    /// Prepares a complete LLM request from raw user input parameters
    /// </summary>
    /// <param name="provider">The AI provider to use</param>
    /// <param name="prompt">The user prompt</param>
    /// <param name="systemPrompt">Optional system prompt</param>
    /// <param name="conversationId">Optional conversation ID for history</param>
    /// <param name="contextFileIds">Optional context file IDs for enrichment</param>
    /// <param name="model">Optional model name (defaults to provider's default model)</param>
    /// <param name="maxTokens">Optional maximum tokens</param>
    /// <param name="temperature">Optional temperature setting</param>
    /// <param name="userId">User ID for traceability</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A prepared request ready for LLM execution</returns>
    Task<PreparedPromptRequest> PrepareAsync(
        AiProvider provider,
        string prompt,
        string? systemPrompt,
        Guid? conversationId,
        List<Guid>? contextFileIds,
        string? model,
        int? maxTokens,
        double? temperature,
        string userId,
        CancellationToken cancellationToken);
}
