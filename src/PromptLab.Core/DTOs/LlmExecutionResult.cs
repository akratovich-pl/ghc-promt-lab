namespace PromptLab.Core.DTOs;

/// <summary>
/// Contains the result of LLM execution including provider response and metadata
/// </summary>
public class LlmExecutionResult
{
    /// <summary>
    /// The provider's response to the LLM request
    /// </summary>
    public required LlmResponse Response { get; set; }

    /// <summary>
    /// Provider type enum
    /// </summary>
    public required Domain.Enums.AiProvider Provider { get; set; }

    /// <summary>
    /// Name of the provider that handled the request
    /// </summary>
    public required string ProviderName { get; set; }

    /// <summary>
    /// Actual model used by the provider
    /// </summary>
    public required string ActualModel { get; set; }

    /// <summary>
    /// Timestamp when execution completed
    /// </summary>
    public required DateTime ExecutionTimestamp { get; set; }

    /// <summary>
    /// Context file ID from preparation stage (pass-through)
    /// </summary>
    public Guid? ContextFileId { get; set; }
}
