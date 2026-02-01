using PromptLab.Core.Domain.Enums;

namespace PromptLab.Core.Application.DTOs;

/// <summary>
/// Information about an AI provider
/// </summary>
public class ProviderInfo
{
    public AiProvider Provider { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public List<string> SupportedModels { get; set; } = new();
}

/// <summary>
/// Health status of a provider
/// </summary>
public class ProviderStatus
{
    public AiProvider Provider { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime LastChecked { get; set; }
}

/// <summary>
/// Information about a model
/// </summary>
public class ModelInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AiProvider Provider { get; set; }
    public int MaxTokens { get; set; }
    public decimal InputCostPer1kTokens { get; set; }
    public decimal OutputCostPer1kTokens { get; set; }
}
