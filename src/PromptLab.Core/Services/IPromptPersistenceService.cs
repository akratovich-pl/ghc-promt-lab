using PromptLab.Core.DTOs;

namespace PromptLab.Core.Services;

/// <summary>
/// Pipeline service for persisting prompt execution results and managing conversation lifecycle
/// </summary>
public interface IPromptPersistenceService
{
    /// <summary>
    /// Persists the prompt execution result to database and manages conversation lifecycle
    /// </summary>
    /// <param name="prompt">The user prompt text</param>
    /// <param name="systemPrompt">Optional system prompt</param>
    /// <param name="conversationId">Optional existing conversation ID</param>
    /// <param name="executionResult">Result from LLM execution stage</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="userId">User ID</param>
    /// <param name="latencyMs">Total execution latency in milliseconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Persistence result with saved prompt details</returns>
    Task<PromptPersistenceResult> PersistAsync(
        string prompt,
        string? systemPrompt,
        Guid? conversationId,
        LlmExecutionResult executionResult,
        string correlationId,
        string userId,
        int latencyMs,
        CancellationToken cancellationToken);
}
