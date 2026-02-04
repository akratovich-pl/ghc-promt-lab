using Microsoft.Extensions.Logging;
using PromptLab.Core.DTOs;
using PromptLab.Core.Providers;
using PromptLab.Core.Services;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Service for executing LLM requests with configured providers.
/// Routes requests to appropriate providers and handles responses.
/// </summary>
public class LlmExecutionService : ILlmExecutionService
{
    private readonly IEnumerable<ILlmProvider> _llmProviders;
    private readonly ILogger<LlmExecutionService> _logger;

    public LlmExecutionService(
        IEnumerable<ILlmProvider> llmProviders,
        ILogger<LlmExecutionService> logger)
    {
        _llmProviders = llmProviders ?? throw new ArgumentNullException(nameof(llmProviders));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LlmExecutionResult> ExecuteAsync(
        PreparedPromptRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation(
            "Executing LLM request with model {Model}. CorrelationId: {CorrelationId}",
            request.Model, correlationId);

        // Select provider based on model name
        var provider = SelectProvider(request.Model);

        if (provider == null)
        {
            var errorMessage = $"No provider found for model: {request.Model}";
            _logger.LogError("{ErrorMessage}. CorrelationId: {CorrelationId}", 
                errorMessage, correlationId);
            throw new InvalidOperationException(errorMessage);
        }

        _logger.LogDebug(
            "Selected provider {ProviderName} for model {Model}. CorrelationId: {CorrelationId}",
            provider.ProviderName, request.Model, correlationId);

        // TODO: Add retry policy for transient failures
        // TODO: Add response caching for identical requests
        // TODO: Add circuit breaker for consistently unavailable providers

        try
        {
            // Execute the LLM request
            var response = await provider.GenerateAsync(request.LlmRequest, cancellationToken);

            var executionTimestamp = DateTime.UtcNow;

            _logger.LogInformation(
                "LLM execution completed successfully. Provider: {Provider}, Model: {Model}, " +
                "Tokens: {Tokens}, Cost: {Cost}, Latency: {Latency}ms, CorrelationId: {CorrelationId}",
                provider.ProviderName, response.Model,
                response.PromptTokens + response.CompletionTokens,
                response.Cost, response.LatencyMs, correlationId);

            return new LlmExecutionResult
            {
                Response = response,
                Provider = provider.Provider,
                ProviderName = provider.ProviderName,
                ActualModel = response.Model,
                ExecutionTimestamp = executionTimestamp,
                ContextFileId = request.ContextFileId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "LLM execution failed. Provider: {Provider}, Model: {Model}, CorrelationId: {CorrelationId}",
                provider.ProviderName, request.Model, correlationId);
            throw;
        }
    }

    /// <summary>
    /// Selects the appropriate provider based on model name.
    /// Currently uses model prefix matching (e.g., "gemini" -> Google, "gpt"/"llama"/"mixtral" -> Groq).
    /// TODO: Replace with provider registry/factory pattern when adding dynamic configuration.
    /// </summary>
    private ILlmProvider? SelectProvider(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return _llmProviders.FirstOrDefault();

        var modelLower = modelName.ToLowerInvariant();

        // Match based on model name prefix
        // Gemini models: gemini-*
        if (modelLower.StartsWith("gemini"))
        {
            return _llmProviders.FirstOrDefault(p => p.Provider == Core.Domain.Enums.AiProvider.Google);
        }

        // Groq models: gpt-*, llama-*, mixtral-*, etc.
        if (modelLower.StartsWith("gpt") || 
            modelLower.StartsWith("llama") || 
            modelLower.StartsWith("mixtral"))
        {
            return _llmProviders.FirstOrDefault(p => p.Provider == Core.Domain.Enums.AiProvider.Groq);
        }

        // Default to first available provider
        _logger.LogWarning(
            "No specific provider match for model {Model}, using first available provider",
            modelName);
        return _llmProviders.FirstOrDefault();
    }
}
