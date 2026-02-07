using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Configuration;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services;
using PromptLab.Core.Providers;

namespace PromptLab.Infrastructure.Providers;

/// <summary>
/// Configuration-based provider service that reads provider configurations 
/// from appsettings.json and checks real-time availability via ILlmProvider
/// </summary>
public class ProviderService : IProviderService
{
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<ILlmProvider> _llmProviders;
    private readonly ILogger<ProviderService> _logger;

    public ProviderService(
        IConfiguration configuration,
        IEnumerable<ILlmProvider> llmProviders,
        ILogger<ProviderService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _llmProviders = llmProviders ?? throw new ArgumentNullException(nameof(llmProviders));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ProviderInfo>> GetProvidersAsync(CancellationToken cancellationToken = default)
    {
        var providersConfig = _configuration.GetSection(ProvidersConfiguration.SectionName);
        var providerSections = providersConfig.GetChildren().ToList();

        // Fetch all providers in parallel to reduce total response time
        var providerTasks = providerSections.Select(async providerSection =>
        {
            var providerName = providerSection.Key;
            var providerSettings = new ProviderSettings();
            providerSection.Bind(providerSettings);

            // Map provider name to enum
            var providerEnum = MapProviderNameToEnum(providerName);

            // Find the corresponding ILlmProvider
            var llmProvider = _llmProviders.FirstOrDefault(p => p.Provider == providerEnum);
            
            // Check availability and get real models from provider
            var isAvailable = false;
            var supportedModels = new List<string>();
            
            if (llmProvider != null)
            {
                try
                {
                    isAvailable = await llmProvider.IsAvailable();
                    
                    if (isAvailable)
                    {
                        // Get real models from provider API
                        var models = await llmProvider.GetAvailableModelsAsync(cancellationToken);
                        supportedModels = models.Select(m => m.Name).ToList();
                        
                        _logger.LogInformation(
                            "Retrieved {ModelCount} real models from provider {ProviderName}",
                            supportedModels.Count,
                            providerName);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Provider {ProviderName} is not available, using empty model list",
                            providerName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error retrieving models from provider {ProviderName}, using empty model list",
                        providerName);
                }
            }

            _logger.LogInformation(
                "Provider {ProviderName} loaded with {ModelCount} models, available: {IsAvailable}",
                providerName,
                supportedModels.Count,
                isAvailable);

            return new ProviderInfo
            {
                Provider = providerEnum,
                Name = providerSettings.Name,
                IsAvailable = isAvailable,
                SupportedModels = supportedModels
            };
        });

        var providers = await Task.WhenAll(providerTasks);
        return providers.ToList();
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
            // Check if API key is configured using IConfiguration
            // This reads from User Secrets, appsettings.json, and environment variables
            var apiKey = _configuration.GetValue<string>($"Providers:{providerName}:ApiKey");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                errorMessage = "API key not configured";
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

    private static AiProvider MapProviderNameToEnum(string providerName)
    {
        return providerName.ToLower() switch
        {
            "google" => AiProvider.Google,
            "groq" => AiProvider.Groq,
            _ => throw new NotImplementedException()
        };
    }
}
