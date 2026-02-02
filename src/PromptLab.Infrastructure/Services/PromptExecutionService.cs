using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Domain.Entities;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services.Interfaces;
using PromptLab.Infrastructure.Data;
using System.Diagnostics;

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

    public async Task<PromptExecutionResult> ExecutePromptAsync(
        PromptExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Starting prompt execution. CorrelationId: {CorrelationId}, UserId: {UserId}",
            correlationId, request.UserId);

        try
        {
            // 1. Validate input
            var validationResult = await ValidatePromptAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Prompt validation failed. CorrelationId: {CorrelationId}, Errors: {Errors}",
                    correlationId, string.Join(", ", validationResult.Errors));

                return new PromptExecutionResult
                {
                    Success = false,
                    ErrorMessage = string.Join("; ", validationResult.Errors)
                };
            }

            // 2. Check rate limits
            var isAllowed = await _rateLimitService.CheckRateLimitAsync(request.UserId, cancellationToken);
            if (!isAllowed)
            {
                _logger.LogWarning("Rate limit exceeded. CorrelationId: {CorrelationId}, UserId: {UserId}",
                    correlationId, request.UserId);

                return new PromptExecutionResult
                {
                    Success = false,
                    ErrorMessage = "Rate limit exceeded"
                };
            }

            // 3. Load conversation if provided
            List<ConversationMessage> conversationHistory = new();
            if (request.ConversationId.HasValue)
            {
                conversationHistory = await LoadConversationHistoryAsync(
                    request.ConversationId.Value, 
                    cancellationToken);
                
                _logger.LogInformation("Loaded {Count} messages from conversation {ConversationId}. CorrelationId: {CorrelationId}",
                    conversationHistory.Count, request.ConversationId.Value, correlationId);
            }

            // 4. Load context files if provided
            string contextContent = string.Empty;
            List<Guid> loadedContextFileIds = new();
            if (request.ContextFileIds?.Any() == true)
            {
                var contextResult = await LoadContextFilesAsync(request.ContextFileIds, cancellationToken);
                contextContent = contextResult.Content;
                loadedContextFileIds = contextResult.FileIds;

                _logger.LogInformation("Loaded {Count} context files. CorrelationId: {CorrelationId}",
                    loadedContextFileIds.Count, correlationId);
            }

            // 5. Build complete prompt with context
            var llmRequest = BuildLlmRequest(request, conversationHistory, contextContent);

            // 6. Call provider's GenerateAsync()
            _logger.LogInformation("Calling LLM provider {Provider} with model {Model}. CorrelationId: {CorrelationId}",
                request.Provider, request.Model, correlationId);

            var llmResponse = await _llmProvider.GenerateAsync(llmRequest, cancellationToken);

            stopwatch.Stop();

            // 7 & 8. Save to database in a transaction
            var result = await SaveToDatabase(
                request,
                llmResponse,
                (int)stopwatch.ElapsedMilliseconds,
                loadedContextFileIds.FirstOrDefault(),
                correlationId,
                cancellationToken);

            // 9. Record rate limit usage
            await _rateLimitService.RecordRequestAsync(request.UserId, cancellationToken);

            _logger.LogInformation("Prompt execution completed successfully. CorrelationId: {CorrelationId}, " +
                "PromptId: {PromptId}, ResponseId: {ResponseId}, Tokens: {Tokens}, Cost: {Cost}, Latency: {Latency}ms",
                correlationId, result.PromptId, result.ResponseId, result.TokensUsed, result.Cost, result.LatencyMs);

            // 10. Return response with metadata
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error executing prompt. CorrelationId: {CorrelationId}, UserId: {UserId}",
                correlationId, request.UserId);

            return new PromptExecutionResult
            {
                Success = false,
                ErrorMessage = "An error occurred while processing your request. Please try again later.",
                LatencyMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task<IEnumerable<ProviderStatus>> GetProviderStatusAsync(CancellationToken cancellationToken = default)
    {
        var statuses = new List<ProviderStatus>();

        try
        {
            var isAvailable = await _llmProvider.IsAvailable();
            var models = Enumerable.Empty<string>();

            statuses.Add(new ProviderStatus
            {
                Provider = _llmProvider.Provider,
                IsAvailable = isAvailable,
                AvailableModels = models
            });

            _logger.LogInformation("Provider status check completed. Provider: {Provider}, Available: {IsAvailable}",
                _llmProvider.ProviderName, isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking provider status for {Provider}", _llmProvider.ProviderName);
            
            statuses.Add(new ProviderStatus
            {
                Provider = _llmProvider.Provider,
                IsAvailable = false,
                ErrorMessage = ex.Message
            });
        }

        return statuses;
    }

    public async Task<ValidationResult> ValidatePromptAsync(
        PromptExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate prompt not empty
        if (string.IsNullOrWhiteSpace(request.UserPrompt))
        {
            result.IsValid = false;
            result.Errors.Add("User prompt cannot be empty");
        }

        // Validate prompt length
        if (request.UserPrompt?.Length > MaxPromptLength)
        {
            result.IsValid = false;
            result.Errors.Add($"User prompt exceeds maximum length of {MaxPromptLength} characters");
        }

        // Validate model specified
        if (string.IsNullOrWhiteSpace(request.Model))
        {
            result.IsValid = false;
            result.Errors.Add("Model must be specified");
        }

        // Validate user ID
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            result.IsValid = false;
            result.Errors.Add("User ID is required");
        }

        // Validate context files count
        if (request.ContextFileIds?.Count > MaxContextFilesCount)
        {
            result.IsValid = false;
            result.Errors.Add($"Cannot attach more than {MaxContextFilesCount} context files");
        }

        // Validate conversation exists if provided
        if (request.ConversationId.HasValue)
        {
            var conversationExists = await _dbContext.Conversations
                .AnyAsync(c => c.Id == request.ConversationId.Value, cancellationToken);

            if (!conversationExists)
            {
                result.IsValid = false;
                result.Errors.Add($"Conversation with ID {request.ConversationId.Value} not found");
            }
        }

        // Validate context files exist
        if (request.ContextFileIds?.Any() == true)
        {
            var existingFileIds = await _dbContext.ContextFiles
                .Where(cf => request.ContextFileIds.Contains(cf.Id))
                .Select(cf => cf.Id)
                .ToListAsync(cancellationToken);

            var missingFileIds = request.ContextFileIds.Except(existingFileIds).ToList();
            if (missingFileIds.Any())
            {
                result.IsValid = false;
                result.Errors.Add($"Context files not found: {string.Join(", ", missingFileIds)}");
            }
        }

        // Check provider availability
        var isProviderAvailable = await _llmProvider.IsAvailable();
        if (!isProviderAvailable)
        {
            result.IsValid = false;
            result.Errors.Add($"Provider {request.Provider} is not available");
        }

        return result;
    }

    private async Task<List<ConversationMessage>> LoadConversationHistoryAsync(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var prompts = await _dbContext.Prompts
            .Include(p => p.Responses)
            .Where(p => p.ConversationId == conversationId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var messages = new List<ConversationMessage>();

        foreach (var prompt in prompts)
        {
            // Add user message
            messages.Add(new ConversationMessage
            {
                Role = "user",
                Content = prompt.UserPrompt
            });

            // Add assistant response (taking the first/most recent response)
            var response = prompt.Responses.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
            if (response != null)
            {
                messages.Add(new ConversationMessage
                {
                    Role = "assistant",
                    Content = response.Content
                });
            }
        }

        return messages;
    }

    private async Task<(string Content, List<Guid> FileIds)> LoadContextFilesAsync(
        List<Guid> contextFileIds,
        CancellationToken cancellationToken)
    {
        var contextFiles = await _dbContext.ContextFiles
            .Where(cf => contextFileIds.Contains(cf.Id))
            .ToListAsync(cancellationToken);

        var contentBuilder = new System.Text.StringBuilder();
        var loadedFileIds = new List<Guid>();

        foreach (var file in contextFiles)
        {
            try
            {
                // Read file content from storage path
                if (File.Exists(file.StoragePath))
                {
                    var content = await File.ReadAllTextAsync(file.StoragePath, cancellationToken);
                    contentBuilder.AppendLine($"=== File: {file.FileName} ===");
                    contentBuilder.AppendLine(content);
                    contentBuilder.AppendLine();
                    loadedFileIds.Add(file.Id);
                }
                else
                {
                    _logger.LogWarning("Context file not found at path: {Path}, FileId: {FileId}",
                        file.StoragePath, file.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading context file: {FileId}, Path: {Path}",
                    file.Id, file.StoragePath);
            }
        }

        return (contentBuilder.ToString(), loadedFileIds);
    }

    private LlmRequest BuildLlmRequest(
        PromptExecutionRequest request,
        List<ConversationMessage> conversationHistory,
        string contextContent)
    {
        var llmRequest = new LlmRequest
        {
            Prompt = request.UserPrompt ?? string.Empty,
            Model = request.Model,
            SystemPrompt = request.SystemPrompt ?? string.Empty,
            ConversationHistory = conversationHistory,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature
        };

        // Build user prompt with context
        if (!string.IsNullOrEmpty(contextContent))
        {
            llmRequest.UserPrompt = $"Context:\n{contextContent}\n\nUser Request:\n{request.UserPrompt}";
        }
        else
        {
            llmRequest.UserPrompt = request.UserPrompt;
        }

        return llmRequest;
    }

    private async Task<PromptExecutionResult> SaveToDatabase(
        PromptExecutionRequest request,
        LlmResponse llmResponse,
        int latencyMs,
        Guid? contextFileId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Ensure conversation exists or create new one
            Guid conversationId;
            if (request.ConversationId.HasValue)
            {
                conversationId = request.ConversationId.Value;
            }
            else
            {
                // Create a new conversation
                var conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Title = request.UserPrompt.Length > MaxConversationTitleLength 
                        ? request.UserPrompt[..TruncatedTitleLength] + "..." 
                        : request.UserPrompt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Conversations.Add(conversation);
                conversationId = conversation.Id;

                _logger.LogInformation("Created new conversation {ConversationId}. CorrelationId: {CorrelationId}",
                    conversationId, correlationId);
            }

            // Save Prompt entity
            // Note: Current schema only supports a single ContextFileId. 
            // If multiple context files are loaded, only the first is persisted.
            // Consider updating Prompt entity to support multiple context files in the future.
            var prompt = new Prompt
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserPrompt = request.UserPrompt,
                Context = request.SystemPrompt,
                ContextFileId = contextFileId,
                EstimatedTokens = 0, // Could be calculated with token counter
                ActualTokens = llmResponse.PromptTokens + llmResponse.CompletionTokens,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Prompts.Add(prompt);

            // Save Response entity
            var response = new Response
            {
                Id = Guid.NewGuid(),
                PromptId = prompt.Id,
                Provider = request.Provider,
                Model = llmResponse.Model,
                Content = llmResponse.Content,
                Tokens = llmResponse.PromptTokens + llmResponse.CompletionTokens,
                Cost = llmResponse.Cost,
                LatencyMs = latencyMs,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Responses.Add(response);

            // Update conversation timestamp
            var existingConversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
            if (existingConversation != null)
            {
                existingConversation.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new PromptExecutionResult
            {
                PromptId = prompt.Id,
                ResponseId = response.Id,
                Content = response.Content,
                TokensUsed = response.Tokens,
                Cost = response.Cost,
                LatencyMs = response.LatencyMs,
                Success = true
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error saving prompt/response to database. CorrelationId: {CorrelationId}",
                correlationId);
            throw;
        }
    }
}
