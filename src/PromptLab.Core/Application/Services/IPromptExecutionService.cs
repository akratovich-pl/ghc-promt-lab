using PromptLab.Core.Application.DTOs;

namespace PromptLab.Core.Application.Services;

/// <summary>
/// Service for executing prompts against LLM providers
/// </summary>
public interface IPromptExecutionService
{
    /// <summary>
    /// Executes a prompt and returns the response
    /// </summary>
    Task<PromptExecutionResult> ExecutePromptAsync(
        string prompt,
        string? systemPrompt = null,
        Guid? conversationId = null,
        List<Guid>? contextFileIds = null,
        string? model = null,
        int? maxTokens = null,
        double? temperature = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the token count and cost for a prompt
    /// </summary>
    Task<TokenEstimate> EstimateTokensAsync(
        string prompt,
        string? model = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets prompt details by ID
    /// </summary>
    Task<PromptExecutionResult?> GetPromptByIdAsync(
        Guid promptId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all prompts in a conversation
    /// </summary>
    Task<List<PromptExecutionResult>> GetPromptsByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
