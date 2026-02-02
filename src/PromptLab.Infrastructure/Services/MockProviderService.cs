using PromptLab.Core.Application.DTOs;
using PromptLab.Core.Application.Services;
using PromptLab.Core.Domain.Enums;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Mock implementation of provider service
/// This is a placeholder until the actual provider integrations are implemented
/// </summary>
public class MockProviderService : IProviderService
{
    public Task<List<ProviderInfo>> GetProvidersAsync(CancellationToken cancellationToken = default)
    {
        var providers = new List<ProviderInfo>
        {
            new ProviderInfo
            {
                Provider = AiProvider.Google,
                Name = "Google",
                IsAvailable = true,
                SupportedModels = new List<string>
                {
                    "gemini-1.5-flash",
                    "gemini-1.5-pro",
                    "gemini-1.0-pro"
                }
            },
            new ProviderInfo
            {
                Provider = AiProvider.OpenAI,
                Name = "OpenAI",
                IsAvailable = false,
                SupportedModels = new List<string>
                {
                    "gpt-4o",
                    "gpt-4-turbo",
                    "gpt-3.5-turbo"
                }
            },
            new ProviderInfo
            {
                Provider = AiProvider.Anthropic,
                Name = "Anthropic",
                IsAvailable = false,
                SupportedModels = new List<string>
                {
                    "claude-3-5-sonnet",
                    "claude-3-opus",
                    "claude-3-haiku"
                }
            }
        };

        return Task.FromResult(providers);
    }

    public Task<ProviderStatus> GetProviderStatusAsync(
        string providerName,
        CancellationToken cancellationToken = default)
    {
        var status = providerName.ToLower() switch
        {
            "google" => new ProviderStatus
            {
                Provider = AiProvider.Google,
                Name = "Google",
                IsHealthy = true,
                ErrorMessage = null,
                LastChecked = DateTime.UtcNow
            },
            "openai" => new ProviderStatus
            {
                Provider = AiProvider.OpenAI,
                Name = "OpenAI",
                IsHealthy = false,
                ErrorMessage = "Provider not configured",
                LastChecked = DateTime.UtcNow
            },
            "anthropic" => new ProviderStatus
            {
                Provider = AiProvider.Anthropic,
                Name = "Anthropic",
                IsHealthy = false,
                ErrorMessage = "Provider not configured",
                LastChecked = DateTime.UtcNow
            },
            _ => throw new ArgumentException($"Unknown provider: {providerName}", nameof(providerName))
        };

        return Task.FromResult(status);
    }

    public Task<List<ModelInfo>> GetProviderModelsAsync(
        string providerName,
        CancellationToken cancellationToken = default)
    {
        var models = providerName.ToLower() switch
        {
            "google" => new List<ModelInfo>
            {
                new ModelInfo
                {
                    Name = "gemini-1.5-flash",
                    DisplayName = "Gemini 1.5 Flash",
                    Provider = AiProvider.Google,
                    MaxTokens = 8192,
                    InputCostPer1kTokens = 0.00007m,
                    OutputCostPer1kTokens = 0.0003m
                },
                new ModelInfo
                {
                    Name = "gemini-1.5-pro",
                    DisplayName = "Gemini 1.5 Pro",
                    Provider = AiProvider.Google,
                    MaxTokens = 8192,
                    InputCostPer1kTokens = 0.00125m,
                    OutputCostPer1kTokens = 0.005m
                },
                new ModelInfo
                {
                    Name = "gemini-1.0-pro",
                    DisplayName = "Gemini 1.0 Pro",
                    Provider = AiProvider.Google,
                    MaxTokens = 8192,
                    InputCostPer1kTokens = 0.0005m,
                    OutputCostPer1kTokens = 0.0015m
                }
            },
            "openai" => new List<ModelInfo>
            {
                new ModelInfo
                {
                    Name = "gpt-4o",
                    DisplayName = "GPT-4 Omni",
                    Provider = AiProvider.OpenAI,
                    MaxTokens = 4096,
                    InputCostPer1kTokens = 0.005m,
                    OutputCostPer1kTokens = 0.015m
                },
                new ModelInfo
                {
                    Name = "gpt-4-turbo",
                    DisplayName = "GPT-4 Turbo",
                    Provider = AiProvider.OpenAI,
                    MaxTokens = 4096,
                    InputCostPer1kTokens = 0.01m,
                    OutputCostPer1kTokens = 0.03m
                },
                new ModelInfo
                {
                    Name = "gpt-3.5-turbo",
                    DisplayName = "GPT-3.5 Turbo",
                    Provider = AiProvider.OpenAI,
                    MaxTokens = 4096,
                    InputCostPer1kTokens = 0.0015m,
                    OutputCostPer1kTokens = 0.002m
                }
            },
            "anthropic" => new List<ModelInfo>
            {
                new ModelInfo
                {
                    Name = "claude-3-5-sonnet",
                    DisplayName = "Claude 3.5 Sonnet",
                    Provider = AiProvider.Anthropic,
                    MaxTokens = 8192,
                    InputCostPer1kTokens = 0.003m,
                    OutputCostPer1kTokens = 0.015m
                },
                new ModelInfo
                {
                    Name = "claude-3-opus",
                    DisplayName = "Claude 3 Opus",
                    Provider = AiProvider.Anthropic,
                    MaxTokens = 4096,
                    InputCostPer1kTokens = 0.015m,
                    OutputCostPer1kTokens = 0.075m
                },
                new ModelInfo
                {
                    Name = "claude-3-haiku",
                    DisplayName = "Claude 3 Haiku",
                    Provider = AiProvider.Anthropic,
                    MaxTokens = 4096,
                    InputCostPer1kTokens = 0.00025m,
                    OutputCostPer1kTokens = 0.00125m
                }
            },
            _ => throw new ArgumentException($"Unknown provider: {providerName}", nameof(providerName))
        };

        return Task.FromResult(models);
    }
}
