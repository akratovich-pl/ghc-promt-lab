using PromptLab.Core.DTOs;

namespace PromptLab.Core.Services;

/// <summary>
/// Service for executing LLM requests with configured providers
/// </summary>
public interface ILlmExecutionService
{
    /// <summary>
    /// Executes a prepared LLM request and returns the provider response with metadata
    /// </summary>
    /// <param name="request">The prepared request from the preparation stage</param>
    /// <param name="correlationId">Correlation ID for distributed tracing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution result with provider response and metadata</returns>
    Task<LlmExecutionResult> ExecuteAsync(
        PreparedPromptRequest request,
        string correlationId,
        CancellationToken cancellationToken = default);
}
