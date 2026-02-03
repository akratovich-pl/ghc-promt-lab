using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Configuration;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services.Interfaces;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Configuration-based provider service that reads provider configurations 
/// from appsettings.json and checks real-time availability via ILlmProvider
/// </summary>
public class ConfigurationProviderService : IProviderService
{
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<ILlmProvider> _llmProviders;
    private readonly ILogger<ConfigurationProviderService> _logger;

    public ConfigurationProviderService(
        IConfiguration configuration,
        IEnumerable<ILlmProvider> llmProviders,
        ILogger<ConfigurationProviderService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _llmProviders = llmProviders ?? throw new ArgumentNullException(nameof(llmProviders));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ProviderInfo>> GetProvidersAsync(CancellationToken cancellationToken = default)
    {
        var providers = new List<ProviderInfo>();
        var providersConfig = _configuration.GetSection(ProvidersConfiguration.SectionName);

        foreach (var providerSection in providersConfig.GetChildren())
        {
            var providerName = providerSection.Key;
            var providerSettings = new ProviderSettings();
            providerSection.Bind(providerSettings);

            // Map provider name to enum
            var providerEnum = MapProviderNameToEnum(providerName);

            // Find the corresponding ILlmProvider
            var llmProvider = _llmProviders.FirstOrDefault(p => p.Provider == providerEnum);
            
            // Check availability
            var isAvailable = false;
            if (llmProvider != null)
            {
                isAvailable = await llmProvider.IsAvailable();
            }

            var supportedModels = providerSettings.Models.Select(m => m.Name).ToList();

            providers.Add(new ProviderInfo
            {
                Provider = providerEnum,
                Name = providerSettings.Name,
                IsAvailable = isAvailable,
                SupportedModels = supportedModels
            });

            _logger.LogInformation(
                "Provider {ProviderName} loaded with {ModelCount} models, available: {IsAvailable}",
                providerName,
                supportedModels.Count,
                isAvailable);
        }

        return providers;
    }

    public async Task<ProviderStatus> GetProviderStatusAsync(
        string providerName,
        CancellationToken cancellationToken = default)
    {
        var providerEnum = MapProviderNameToEnum(providerName);
        var llmProvider = _llmProviders.FirstOrDefault(p => p.Provider == providerEnum);

        if (llmProvider == null)
        {
            throw new ArgumentException($"Unknown provider: {providerName}", nameof(providerName));
        }

        var isHealthy = false;
        string? errorMessage = null;

        try
        {
            // Check if API key is configured
            var apiKeyEnvVar = GetApiKeyEnvironmentVariable(providerName);
            var apiKey = Environment.GetEnvironmentVariable(apiKeyEnvVar);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                errorMessage = $"API key not configured (missing {apiKeyEnvVar} environment variable)";
                _logger.LogWarning("Provider {ProviderName} unavailable: {ErrorMessage}", providerName, errorMessage);
            }
            else
            {
                // Call provider's health check
                isHealthy = await llmProvider.IsAvailable();
                
                if (!isHealthy)
                {
                    errorMessage = "Provider health check failed";
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Health check error: {ex.Message}";
            _logger.LogError(ex, "Error checking provider {ProviderName} status", providerName);
        }

        return new ProviderStatus
        {
            Provider = providerEnum,
            Name = providerName,
            IsHealthy = isHealthy,
            ErrorMessage = errorMessage,
            LastChecked = DateTime.UtcNow
        };
    }

    public Task<List<ModelInfo>> GetProviderModelsAsync(
        string providerName,
        CancellationToken cancellationToken = default)
    {
        var providerEnum = MapProviderNameToEnum(providerName);
        var providerSection = _configuration.GetSection($"{ProvidersConfiguration.SectionName}:{providerName}");

        if (!providerSection.Exists())
        {
            throw new ArgumentException($"Unknown provider: {providerName}", nameof(providerName));
        }

        var providerSettings = new ProviderSettings();
        providerSection.Bind(providerSettings);

        var models = providerSettings.Models.Select(m => new ModelInfo
        {
            Name = m.Name,
            DisplayName = m.DisplayName,
            Provider = providerEnum,
            MaxTokens = m.MaxTokens,
            InputCostPer1kTokens = m.InputCostPer1kTokens,
            OutputCostPer1kTokens = m.OutputCostPer1kTokens
        }).ToList();

        _logger.LogInformation(
            "Retrieved {ModelCount} models for provider {ProviderName}",
            models.Count,
            providerName);

        return Task.FromResult(models);
    }

    private AiProvider MapProviderNameToEnum(string providerName)
    {
        return providerName.ToLower() switch
        {
            "google" => AiProvider.Google,
            "groq" => AiProvider.Groq,
            "openai" => AiProvider.OpenAI,
            "anthropic" => AiProvider.Anthropic,
            _ => AiProvider.Other
        };
    }

    private string GetApiKeyEnvironmentVariable(string providerName)
    {
        return providerName.ToLower() switch
        {
            "google" => "GOOGLE_API_KEY",
            "groq" => "GROQ_API_KEY",
            "openai" => "OPENAI_API_KEY",
            "anthropic" => "ANTHROPIC_API_KEY",
            _ => $"{providerName.ToUpper()}_API_KEY"
        };
    }
}
