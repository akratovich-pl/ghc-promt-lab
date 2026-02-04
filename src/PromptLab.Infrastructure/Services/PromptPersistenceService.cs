using Microsoft.Extensions.Logging;
using PromptLab.Core.DTOs;
using PromptLab.Core.Repositories;
using PromptLab.Core.Services;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Pipeline service for persisting prompt execution results and managing conversation lifecycle.
/// Handles database operations for conversation creation/updates and prompt storage.
/// </summary>
public class PromptPersistenceService : IPromptPersistenceService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IConversationHistoryService _conversationHistoryService;
    private readonly ILogger<PromptPersistenceService> _logger;

    public PromptPersistenceService(
        IPromptRepository promptRepository,
        IConversationHistoryService conversationHistoryService,
        ILogger<PromptPersistenceService> logger)
    {
        _promptRepository = promptRepository ?? throw new ArgumentNullException(nameof(promptRepository));
        _conversationHistoryService = conversationHistoryService ?? throw new ArgumentNullException(nameof(conversationHistoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PromptPersistenceResult> PersistAsync(
        string prompt,
        string? systemPrompt,
        Guid? conversationId,
        LlmExecutionResult executionResult,
        string correlationId,
        string userId,
        int latencyMs,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting prompt persistence. CorrelationId: {CorrelationId}, " +
            "ConversationId: {ConversationId}",
            correlationId, conversationId);

        var isNewConversation = !conversationId.HasValue;

        // Step 1: Ensure conversation exists (create if needed)
        var actualConversationId = await _conversationHistoryService.EnsureConversationAsync(
            conversationId, userId, prompt, cancellationToken);

        _logger.LogDebug("Conversation ensured. CorrelationId: {CorrelationId}, " +
            "ConversationId: {ConversationId}, IsNew: {IsNew}",
            correlationId, actualConversationId, isNewConversation);

        // Step 2: Save prompt execution to database
        var (promptId, responseId) = await _promptRepository.SavePromptExecutionAsync(
            actualConversationId,
            prompt,
            systemPrompt,
            executionResult.ContextFileId,
            executionResult.Response,
            latencyMs,
            cancellationToken);

        _logger.LogDebug("Prompt execution saved. CorrelationId: {CorrelationId}, " +
            "PromptId: {PromptId}, ResponseId: {ResponseId}",
            correlationId, promptId, responseId);

        // Step 3: Update conversation timestamp
        await _conversationHistoryService.UpdateConversationTimestampAsync(
            actualConversationId, cancellationToken);

        _logger.LogInformation("Prompt persistence completed. CorrelationId: {CorrelationId}, " +
            "PromptId: {PromptId}, ResponseId: {ResponseId}, ConversationId: {ConversationId}",
            correlationId, promptId, responseId, actualConversationId);

        return new PromptPersistenceResult
        {
            PromptId = promptId,
            ResponseId = responseId,
            ConversationId = actualConversationId,
            CreatedAt = DateTime.UtcNow,
            IsNewConversation = isNewConversation
        };
    }
}
