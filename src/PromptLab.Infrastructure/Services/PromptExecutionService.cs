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
    private readonly ILlmProvider _llmProvider;
    private readonly IRequestPreparationService _requestPreparationService;
    private readonly IConversationHistoryService _conversationHistoryService;
    private readonly IPromptRepository _promptRepository;
    private readonly ILogger<PromptExecutionService> _logger;

    public PromptExecutionService(
        ILlmProvider llmProvider,
        IRequestPreparationService requestPreparationService,
        IConversationHistoryService conversationHistoryService,
        IPromptRepository promptRepository,
        ILogger<PromptExecutionService> logger)
    {
        _llmProvider = llmProvider ?? throw new ArgumentNullException(nameof(llmProvider));
        _requestPreparationService = requestPreparationService ?? throw new ArgumentNullException(nameof(requestPreparationService));
        _conversationHistoryService = conversationHistoryService ?? throw new ArgumentNullException(nameof(conversationHistoryService));
        _promptRepository = promptRepository ?? throw new ArgumentNullException(nameof(promptRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PromptExecutionResult> ExecutePromptAsync(
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

        _logger.LogInformation("Starting prompt execution. CorrelationId: {CorrelationId}, UserId: {UserId}",
            correlationId, userId);

        // 1-4. Prepare request (validation, history, enrichment, building)
        var preparedRequest = await _requestPreparationService.PrepareAsync(
            prompt, systemPrompt, conversationId, contextFileIds, 
            model, maxTokens, temperature, userId, cancellationToken);

        // 5. Call LLM provider
        _logger.LogInformation("Calling LLM provider with model {Model}. CorrelationId: {CorrelationId}",
            preparedRequest.Model, correlationId);

        var llmResponse = await _llmProvider.GenerateAsync(preparedRequest.LlmRequest, cancellationToken);

        stopwatch.Stop();

        // 6. Ensure conversation exists
        var actualConversationId = await _conversationHistoryService.EnsureConversationAsync(
            conversationId, userId, prompt, cancellationToken);

        // 7. Save to database
        var (promptId, responseId) = await _promptRepository.SavePromptExecutionAsync(
            actualConversationId,
            prompt,
            systemPrompt,
            preparedRequest.ContextFileId,
            llmResponse,
            (int)stopwatch.ElapsedMilliseconds,
            cancellationToken);

        // 8. Update conversation timestamp
        await _conversationHistoryService.UpdateConversationTimestampAsync(
            actualConversationId, cancellationToken);

        _logger.LogInformation("Prompt execution completed successfully. CorrelationId: {CorrelationId}, " +
            "PromptId: {PromptId}, ResponseId: {ResponseId}, Tokens: {Tokens}, Cost: {Cost}, Latency: {Latency}ms",
            correlationId, promptId, responseId, llmResponse.PromptTokens + llmResponse.CompletionTokens, 
            llmResponse.Cost, stopwatch.ElapsedMilliseconds);

        return new PromptExecutionResult
        {
            PromptId = promptId,
            ResponseId = responseId,
            Content = llmResponse.Content,
            InputTokens = llmResponse.PromptTokens,
            OutputTokens = llmResponse.CompletionTokens,
            Cost = llmResponse.Cost,
            LatencyMs = (int)stopwatch.ElapsedMilliseconds,
            Model = llmResponse.Model,
            Provider = AiProvider.Google,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<TokenEstimate> EstimateTokensAsync(
        string prompt,
        string? model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenCount = await _llmProvider.EstimateTokensAsync(prompt, cancellationToken);
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




