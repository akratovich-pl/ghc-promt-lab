using PromptLab.Core.DTOs;

namespace PromptLab.Core.Services;

/// <summary>
/// Domain service for managing conversation history and lifecycle
/// </summary>
public interface IConversationHistoryService
{
    /// <summary>
    /// Loads conversation history for a given conversation ID
    /// </summary>
    /// <param name="conversationId">The conversation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of conversation messages in chronological order</returns>
    Task<List<ConversationMessage>> LoadConversationHistoryAsync(
        Guid conversationId, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Ensures a conversation exists or creates a new one
    /// </summary>
    /// <param name="conversationId">Optional existing conversation ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="initialPrompt">Initial prompt to use for title if creating new conversation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The conversation ID (existing or newly created)</returns>
    Task<Guid> EnsureConversationAsync(
        Guid? conversationId, 
        string userId, 
        string initialPrompt, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the conversation timestamp
    /// </summary>
    /// <param name="conversationId">The conversation ID to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateConversationTimestampAsync(Guid conversationId, CancellationToken cancellationToken);
}
