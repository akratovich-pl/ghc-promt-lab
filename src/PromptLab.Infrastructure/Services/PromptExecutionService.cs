using Microsoft.Extensions.Logging;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Repositories;
using PromptLab.Core.Services;
using PromptLab.Core.Providers;
using System.Diagnostics;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Application service for orchestrating prompt execution with LLM providers.
/// Coordinates request preparation, LLM interaction, and persistence through specialized components.
/// Rate limiting is handled at the middleware layer.
/// </summary>
public class PromptExecutionService : IPromptExecutionService
{
    private readonly ILlmExecutionService _llmExecutionService;
    private readonly IRequestPreparationService _requestPreparationService;
    private readonly IPromptPersistenceService _promptPersistenceService;
    private readonly IPromptRepository _promptRepository;
    private readonly IEnumerable<ILlmProvider> _llmProviders;
    private readonly ILogger<PromptExecutionService> _logger;

    public PromptExecutionService(
        ILlmExecutionService llmExecutionService,
        IRequestPreparationService requestPreparationService,
        IPromptPersistenceService promptPersistenceService,
        IPromptRepository promptRepository,
        IEnumerable<ILlmProvider> llmProviders,
        ILogger<PromptExecutionService> logger)
    {
        _llmExecutionService = llmExecutionService ?? throw new ArgumentNullException(nameof(llmExecutionService));
        _requestPreparationService = requestPreparationService ?? throw new ArgumentNullException(nameof(requestPreparationService));
        _promptPersistenceService = promptPersistenceService ?? throw new ArgumentNullException(nameof(promptPersistenceService));
        _promptRepository = promptRepository ?? throw new ArgumentNullException(nameof(promptRepository));
        _llmProviders = llmProviders ?? throw new ArgumentNullException(nameof(llmProviders));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PromptExecutionResult> ExecutePromptAsync(
        AiProvider provider,
        string prompt,
        string? systemPrompt,
        Guid? conversationId,
        List<Guid>? contextFileIds,
        string? model,
        int? maxTokens,
        double? temperature,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();
        var userId = "system"; // TODO: Get from auth context

        _logger.LogInformation("Starting prompt execution. Provider: {Provider}, CorrelationId: {CorrelationId}, UserId: {UserId}",
            provider, correlationId, userId);

        // 1-4. Prepare request (validation, history, enrichment, building)
        var preparedRequest = await _requestPreparationService.PrepareAsync(
            provider, prompt, systemPrompt, conversationId, contextFileIds, 
            model, maxTokens, temperature, userId, cancellationToken);

        // 5. Execute LLM request
        var executionResult = await _llmExecutionService.ExecuteAsync(
            preparedRequest, correlationId.ToString(), cancellationToken);

        stopwatch.Stop();

        // 6-8. Persist results (conversation management + database save)
        var persistenceResult = await _promptPersistenceService.PersistAsync(
            prompt, systemPrompt, conversationId, executionResult, 
            correlationId.ToString(), userId, (int)stopwatch.ElapsedMilliseconds, 
            cancellationToken);

        var llmResponse = executionResult.Response;

        _logger.LogInformation("Prompt execution completed successfully. CorrelationId: {CorrelationId}, " +
            "PromptId: {PromptId}, ResponseId: {ResponseId}, Tokens: {Tokens}, Cost: {Cost}, Latency: {Latency}ms",
            correlationId, persistenceResult.PromptId, persistenceResult.ResponseId, 
            llmResponse.PromptTokens + llmResponse.CompletionTokens, 
            llmResponse.Cost, stopwatch.ElapsedMilliseconds);

        return new PromptExecutionResult
        {
            PromptId = persistenceResult.PromptId,
            ResponseId = persistenceResult.ResponseId,
            Content = llmResponse.Content,
            InputTokens = llmResponse.PromptTokens,
            OutputTokens = llmResponse.CompletionTokens,
            Cost = llmResponse.Cost,
            LatencyMs = (int)stopwatch.ElapsedMilliseconds,
            Model = llmResponse.Model,
            Provider = executionResult.Provider,
            CreatedAt = persistenceResult.CreatedAt
        };
    }

    public async Task<TokenEstimate> EstimateTokensAsync(
        string prompt,
        string? model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Select provider for token estimation (uses first available provider)
            var provider = _llmProviders.FirstOrDefault();
            if (provider == null)
            {
                throw new InvalidOperationException("No LLM providers available for token estimation");
            }

            var tokenCount = await provider.EstimateTokensAsync(prompt, cancellationToken);
            // Simple cost estimation - this should ideally come from configuration
            var estimatedCost = tokenCount * 0.00001m; // Rough estimate

            return new TokenEstimate
            {
                TokenCount = tokenCount,
                EstimatedCost = estimatedCost,
                Model = model ?? "gemini-1.5-flash"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating tokens for prompt");
            throw;
        }
    }

    public async Task<PromptExecutionResult?> GetPromptByIdAsync(
        Guid promptId,
        CancellationToken cancellationToken)
    {
        var prompt = await _promptRepository.GetPromptByIdAsync(promptId, cancellationToken);

        if (prompt == null)
            return null;

        var response = prompt.Responses?.FirstOrDefault();

        return new PromptExecutionResult
        {
            PromptId = prompt.Id,
            ResponseId = response?.Id ?? Guid.Empty,
            Content = response?.Content ?? string.Empty,
            InputTokens = response?.Tokens / 2 ?? 0, // Rough split since we don't have separate counts
            OutputTokens = response?.Tokens / 2 ?? 0,
            Cost = response?.Cost ?? 0,
            LatencyMs = response?.LatencyMs ?? 0,
            Model = response?.Model ?? "unknown",
            Provider = response?.Provider ?? AiProvider.Google,
            CreatedAt = response?.CreatedAt ?? prompt.CreatedAt
        };
    }

    public async Task<List<PromptExecutionResult>> GetPromptsByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var prompts = await _promptRepository.GetPromptsByConversationIdAsync(
            conversationId, cancellationToken);

        return prompts.Select(p =>
        {
            var response = p.Responses?.FirstOrDefault();
            return new PromptExecutionResult
            {
                PromptId = p.Id,
                ResponseId = response?.Id ?? Guid.Empty,
                Content = response?.Content ?? string.Empty,
                InputTokens = response?.Tokens / 2 ?? 0,
                OutputTokens = response?.Tokens / 2 ?? 0,
                Cost = response?.Cost ?? 0,
                LatencyMs = response?.LatencyMs ?? 0,
                Model = response?.Model ?? "unknown",
                Provider = response?.Provider ?? AiProvider.Google,
                CreatedAt = response?.CreatedAt ?? p.CreatedAt
            };
        }).ToList();
    }
}




