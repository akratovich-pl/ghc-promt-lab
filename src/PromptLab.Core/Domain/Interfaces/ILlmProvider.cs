using PromptLab.Core.Domain.DTOs;

namespace PromptLab.Core.Domain.Interfaces;

/// <summary>
/// Interface for LLM provider implementations
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Generate a response from the LLM
    /// </summary>
    /// <param name="request">The request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The LLM response</returns>
    Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimate token count for a given prompt
    /// </summary>
    /// <param name="prompt">The prompt text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Estimated token count</returns>
    Task<int> EstimateTokensAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the provider is available and properly configured
    /// </summary>
    /// <returns>True if available, false otherwise</returns>
    Task<bool> IsAvailable();

    /// <summary>
    /// Get the provider name
    /// </summary>
    string ProviderName { get; }
}
