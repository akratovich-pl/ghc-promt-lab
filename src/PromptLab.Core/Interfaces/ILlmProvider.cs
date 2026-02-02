using PromptLab.Core.Domain.Entities;

namespace PromptLab.Core.Interfaces;

/// <summary>
/// Interface for LLM provider implementations.
/// This is a placeholder interface showing how logging should be implemented.
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// The name of the provider (e.g., "GoogleGemini", "OpenAI")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Sends a prompt to the LLM provider and returns a response
    /// </summary>
    /// <param name="prompt">The prompt to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the provider</returns>
    Task<Response> SendPromptAsync(Prompt prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the provider is available and healthy
    /// </summary>
    /// <returns>True if provider is available</returns>
    Task<bool> IsAvailableAsync();
}
