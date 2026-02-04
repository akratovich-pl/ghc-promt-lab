using PromptLab.Core.Domain.Entities;
using PromptLab.Core.DTOs;

namespace PromptLab.Core.Repositories;

/// <summary>
/// Repository for managing prompt and response persistence
/// </summary>
public interface IPromptRepository
{
    /// <summary>
    /// Saves a prompt execution result to the database
    /// </summary>
    /// <param name="conversationId">The conversation ID</param>
    /// <param name="userPrompt">The original user prompt</param>
    /// <param name="systemPrompt">Optional system prompt</param>
    /// <param name="contextFileId">Optional context file ID</param>
    /// <param name="llmResponse">The LLM response</param>
    /// <param name="latencyMs">Execution latency in milliseconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing the prompt ID and response ID</returns>
    Task<(Guid promptId, Guid responseId)> SavePromptExecutionAsync(
        Guid conversationId,
        string userPrompt,
        string? systemPrompt,
        Guid? contextFileId,
        LlmResponse llmResponse,
        int latencyMs,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a prompt by ID with its responses
    /// </summary>
    /// <param name="promptId">The prompt ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The prompt entity with responses, or null if not found</returns>
    Task<Prompt?> GetPromptByIdAsync(Guid promptId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all prompts for a conversation
    /// </summary>
    /// <param name="conversationId">The conversation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of prompts with responses</returns>
    Task<List<Prompt>> GetPromptsByConversationIdAsync(
        Guid conversationId, 
        CancellationToken cancellationToken);
}
