using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromptLab.Core.Interfaces;

namespace PromptLab.Infrastructure.Configuration;

/// <summary>
/// Service for managing LLM provider configurations
/// </summary>
public class LlmProviderConfigService : ILlmProviderConfig
{
    private readonly LlmProvidersOptions _options;
    private readonly ILogger<LlmProviderConfigService> _logger;

    public LlmProviderConfigService(
        IOptions<LlmProvidersOptions> options,
        ILogger<LlmProviderConfigService> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        _logger.LogInformation("LLM Provider Configuration Service initialized");
    }

    public string? GetApiKey(string providerName)
    {
        _logger.LogDebug("Retrieving API key for provider: {ProviderName}", providerName);
        
        var envVarName = providerName switch
        {
            "GoogleGemini" => "GOOGLE_GEMINI_API_KEY",
            _ => null
        };

        if (envVarName == null)
        {
            _logger.LogWarning("No environment variable mapping found for provider: {ProviderName}", providerName);
            return null;
        }

        var apiKey = Environment.GetEnvironmentVariable(envVarName);
        
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("API key not found in environment variable: {EnvVarName}", envVarName);
        }
        else
        {
            _logger.LogInformation("API key successfully retrieved for provider: {ProviderName}", providerName);
        }

        return apiKey;
    }

    public string? GetBaseUrl(string providerName)
    {
        _logger.LogDebug("Retrieving base URL for provider: {ProviderName}", providerName);
        
        return providerName switch
        {
            "GoogleGemini" => _options.GoogleGemini.BaseUrl,
            _ => null
        };
    }

    public string? GetDefaultModel(string providerName)
    {
        _logger.LogDebug("Retrieving default model for provider: {ProviderName}", providerName);
        
        return providerName switch
        {
            "GoogleGemini" => _options.GoogleGemini.DefaultModel,
            _ => null
        };
    }

    public int GetMaxTokens(string providerName)
    {
        _logger.LogDebug("Retrieving max tokens for provider: {ProviderName}", providerName);
        
        return providerName switch
        {
            "GoogleGemini" => _options.GoogleGemini.MaxTokens,
            _ => 0
        };
    }

    public double GetTemperature(string providerName)
    {
        _logger.LogDebug("Retrieving temperature for provider: {ProviderName}", providerName);
        
        return providerName switch
        {
            "GoogleGemini" => _options.GoogleGemini.Temperature,
            _ => 0.0
        };
    }

    public bool IsProviderEnabled(string providerName)
    {
        _logger.LogDebug("Checking if provider is enabled: {ProviderName}", providerName);
        
        return providerName switch
        {
            "GoogleGemini" => _options.GoogleGemini.Enabled,
            _ => false
        };
    }
}
