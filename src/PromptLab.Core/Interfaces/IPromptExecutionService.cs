using PromptLab.Core.Domain.Entities;

namespace PromptLab.Core.Interfaces;

/// <summary>
/// Service interface for executing prompts against LLM providers.
/// This is a placeholder interface showing how logging should be implemented.
/// </summary>
public interface IPromptExecutionService
{
    /// <summary>
    /// Executes a prompt and returns responses from configured providers
    /// </summary>
    /// <param name="prompt">The prompt to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of responses from different providers</returns>
    Task<IEnumerable<Response>> ExecutePromptAsync(Prompt prompt, CancellationToken cancellationToken = default);
}
