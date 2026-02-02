using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Domain.Entities;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services.Interfaces;
using PromptLab.Infrastructure.Data;
using System.Diagnostics;
using IPromptExecutionService = PromptLab.Core.Services.Interfaces.IPromptExecutionService;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Service for orchestrating prompt execution with LLM providers
/// </summary>
public class PromptExecutionService : IPromptExecutionService
{
    private readonly ILlmProvider _llmProvider;
    private readonly IRateLimitService _rateLimitService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PromptExecutionService> _logger;

    private const int MaxPromptLength = 100000; // 100k characters
    private const int MaxContextFilesCount = 10;
    private const int MaxConversationTitleLength = 50;
    private const int TruncatedTitleLength = 47;

    public PromptExecutionService(
        ILlmProvider llmProvider,
        IRateLimitService rateLimitService,
        ApplicationDbContext dbContext,
        ILogger<PromptExecutionService> logger)
    {
        _llmProvider = llmProvider ?? throw new ArgumentNullException(nameof(llmProvider));
        _rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Explicit interface implementations for Core.Services.Interfaces.IPromptExecutionService
    async Task<Core.DTOs.PromptExecutionResult> Core.Services.Interfaces.IPromptExecutionService.ExecutePromptAsync(
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
        var requestModel = model ?? "gemini-1.5-flash";

        _logger.LogInformation("Starting prompt execution. CorrelationId: {CorrelationId}, UserId: {UserId}",
            correlationId, userId);

        // 1. Validate input
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("User prompt cannot be empty", nameof(prompt));
        }

        if (prompt.Length > MaxPromptLength)
        {
            throw new ArgumentException($"User prompt exceeds maximum length of {MaxPromptLength} characters", nameof(prompt));
        }

        // 2. Check rate limits
        var isAllowed = await _rateLimitService.CheckRateLimitAsync(userId, cancellationToken);
        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit exceeded. CorrelationId: {CorrelationId}, UserId: {UserId}",
                correlationId, userId);
            throw new InvalidOperationException("Rate limit exceeded");
        }

        // 3. Load conversation history if provided
        var conversationHistory = new List<ConversationMessage>();
        
        if (conversationId.HasValue)
        {
            var prompts = await _dbContext.Prompts
                .Include(p => p.Responses)
                .Where(p => p.ConversationId == conversationId.Value)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            foreach (var prevPrompt in prompts)
            {
                conversationHistory.Add(new ConversationMessage
                {
                    Role = "user",
                    Content = prevPrompt.UserPrompt
                });
                
                var prevResponse = prevPrompt.Responses?.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
                if (prevResponse != null)
                {
                    conversationHistory.Add(new ConversationMessage
                    {
                        Role = "assistant",
                        Content = prevResponse.Content
                    });
                }
            }

            _logger.LogInformation("Loaded {Count} previous messages from conversation {ConversationId}. CorrelationId: {CorrelationId}",
                conversationHistory.Count, conversationId.Value, correlationId);
        }

        // 4. Load context files if provided
        Guid? contextFileId = null;
        string userPromptWithContext = prompt;
        
        if (contextFileIds?.Any() == true)
        {
            contextFileId = contextFileIds.First();
            var contextFiles = await _dbContext.ContextFiles
                .Where(cf => contextFileIds.Contains(cf.Id))
                .ToListAsync(cancellationToken);

            if (contextFiles.Any())
            {
                var contextBuilder = new System.Text.StringBuilder();
                foreach (var file in contextFiles)
                {
                    try
                    {
                        if (File.Exists(file.StoragePath))
                        {
                            var content = await File.ReadAllTextAsync(file.StoragePath, cancellationToken);
                            contextBuilder.AppendLine($"=== File: {file.FileName} ===");
                            contextBuilder.AppendLine(content);
                            contextBuilder.AppendLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading context file: {FileId}", file.Id);
                    }
                }

                var contextContent = contextBuilder.ToString();
                if (!string.IsNullOrWhiteSpace(contextContent))
                {
                    userPromptWithContext = $"Context:\n{contextContent}\n\nUser Request:\n{prompt}";
                }
            }
        }

        // 5. Build LLM request
        var llmRequest = new LlmRequest
        {
            Prompt = userPromptWithContext,
            SystemPrompt = systemPrompt,
            Model = requestModel,
            ConversationHistory = conversationHistory,
            MaxTokens = maxTokens,
            Temperature = temperature.HasValue ? (decimal?)temperature.Value : null
        };

        // 6. Call LLM provider
        _logger.LogInformation("Calling LLM provider with model {Model}. CorrelationId: {CorrelationId}",
            requestModel, correlationId);

        var llmResponse = await _llmProvider.GenerateAsync(llmRequest, cancellationToken);

        stopwatch.Stop();

        // 7. Save to database in a transaction
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Ensure conversation exists or create new one
            Guid actualConversationId;
            if (conversationId.HasValue)
            {
                actualConversationId = conversationId.Value;
            }
            else
            {
                var conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = prompt.Length > MaxConversationTitleLength 
                        ? prompt[..TruncatedTitleLength] + "..." 
                        : prompt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Conversations.Add(conversation);
                actualConversationId = conversation.Id;

                _logger.LogInformation("Created new conversation {ConversationId}. CorrelationId: {CorrelationId}",
                    actualConversationId, correlationId);
            }

            // Save Prompt entity
            var promptEntity = new Prompt
            {
                Id = Guid.NewGuid(),
                ConversationId = actualConversationId,
                UserPrompt = prompt,
                Context = systemPrompt,
                ContextFileId = contextFileId,
                EstimatedTokens = 0,
                ActualTokens = llmResponse.PromptTokens + llmResponse.CompletionTokens,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Prompts.Add(promptEntity);

            // Save Response entity
            var responseEntity = new Response
            {
                Id = Guid.NewGuid(),
                PromptId = promptEntity.Id,
                Provider = AiProvider.Google,
                Model = llmResponse.Model,
                Content = llmResponse.Content,
                Tokens = llmResponse.PromptTokens + llmResponse.CompletionTokens,
                Cost = llmResponse.Cost,
                LatencyMs = (int)stopwatch.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Responses.Add(responseEntity);

            // Update conversation timestamp
            var existingConversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.Id == actualConversationId, cancellationToken);
            if (existingConversation != null)
            {
                existingConversation.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 8. Record rate limit usage
            await _rateLimitService.RecordRequestAsync(userId, cancellationToken);

            _logger.LogInformation("Prompt execution completed successfully. CorrelationId: {CorrelationId}, " +
                "PromptId: {PromptId}, ResponseId: {ResponseId}, Tokens: {Tokens}, Cost: {Cost}, Latency: {Latency}ms",
                correlationId, promptEntity.Id, responseEntity.Id, responseEntity.Tokens, responseEntity.Cost, responseEntity.LatencyMs);

            return new Core.DTOs.PromptExecutionResult
            {
                PromptId = promptEntity.Id,
                ResponseId = responseEntity.Id,
                Content = responseEntity.Content,
                InputTokens = llmResponse.PromptTokens,
                OutputTokens = llmResponse.CompletionTokens,
                Cost = responseEntity.Cost,
                LatencyMs = responseEntity.LatencyMs,
                Model = responseEntity.Model,
                Provider = responseEntity.Provider,
                CreatedAt = responseEntity.CreatedAt
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    async Task<Core.DTOs.TokenEstimate> Core.Services.Interfaces.IPromptExecutionService.EstimateTokensAsync(
        string prompt,
        string? model,
        CancellationToken cancellationToken)
    {
        try
        {
            var tokenCount = await _llmProvider.EstimateTokensAsync(prompt, cancellationToken);
            // Simple cost estimation - this should ideally come from configuration
            var estimatedCost = tokenCount * 0.00001m; // Rough estimate

            return new Core.DTOs.TokenEstimate
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

    async Task<Core.DTOs.PromptExecutionResult?> Core.Services.Interfaces.IPromptExecutionService.GetPromptByIdAsync(
        Guid promptId,
        CancellationToken cancellationToken)
    {
        var prompt = await _dbContext.Prompts
            .Include(p => p.Responses)
            .FirstOrDefaultAsync(p => p.Id == promptId, cancellationToken);

        if (prompt == null)
            return null;

        var response = prompt.Responses?.FirstOrDefault();

        return new Core.DTOs.PromptExecutionResult
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

    async Task<List<Core.DTOs.PromptExecutionResult>> Core.Services.Interfaces.IPromptExecutionService.GetPromptsByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var prompts = await _dbContext.Prompts
            .Include(p => p.Responses)
            .Where(p => p.ConversationId == conversationId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return prompts.Select(p =>
        {
            var response = p.Responses?.FirstOrDefault();
            return new Core.DTOs.PromptExecutionResult
            {
                PromptId = p.Id,
                ResponseId = response?.Id ?? Guid.Empty,
                Content = response?.Content ?? string.Empty,
                InputTokens = response?.Tokens / 2 ?? 0, // Rough split since we don't have separate counts
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



