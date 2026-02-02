using PromptLab.Core.Domain.Enums;

namespace PromptLab.Core.Services.Interfaces;

/// <summary>
/// Service for orchestrating prompt execution
/// </summary>
public interface IPromptExecutionService
{
    /// <summary>
    /// Executes a prompt and returns the response
    /// </summary>
    Task<PromptExecutionResult> ExecutePromptAsync(
        PromptExecutionRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of all AI providers
    /// </summary>
    Task<IEnumerable<ProviderStatus>> GetProviderStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a prompt request before execution
    /// </summary>
    Task<ValidationResult> ValidatePromptAsync(
        PromptExecutionRequest request, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for prompt execution
/// </summary>
public class PromptExecutionRequest
{
    public string UserId { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public string? SystemPrompt { get; set; }
    public Guid? ConversationId { get; set; }
    public List<Guid>? ContextFileIds { get; set; }
    public string Model { get; set; } = string.Empty;
    public AiProvider Provider { get; set; }
    public int? MaxTokens { get; set; }
    public decimal? Temperature { get; set; }
}

/// <summary>
/// Result of prompt execution
/// </summary>
public class PromptExecutionResult
{
    public Guid PromptId { get; set; }
    public Guid ResponseId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public decimal Cost { get; set; }
    public int LatencyMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Status of an AI provider
/// </summary>
public class ProviderStatus
{
    public AiProvider Provider { get; set; }
    public bool IsAvailable { get; set; }
    public IEnumerable<string> AvailableModels { get; set; } = new List<string>();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of validation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
