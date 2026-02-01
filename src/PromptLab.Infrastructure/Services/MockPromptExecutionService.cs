using Microsoft.EntityFrameworkCore;
using PromptLab.Core.Application.DTOs;
using PromptLab.Core.Application.Services;
using PromptLab.Core.Domain.Entities;
using PromptLab.Core.Domain.Enums;
using PromptLab.Infrastructure.Data;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Mock implementation of prompt execution service
/// This is a placeholder until the actual LLM integration is implemented
/// </summary>
public class MockPromptExecutionService : IPromptExecutionService
{
    private readonly ApplicationDbContext _context;

    public MockPromptExecutionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PromptExecutionResult> ExecutePromptAsync(
        string prompt,
        string? systemPrompt = null,
        Guid? conversationId = null,
        List<Guid>? contextFileIds = null,
        string? model = null,
        int? maxTokens = null,
        double? temperature = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));

        // Use provided conversation or create a new one
        var convId = conversationId ?? Guid.NewGuid();
        var conversation = await _context.Conversations.FindAsync(new object[] { convId }, cancellationToken);
        
        if (conversationId.HasValue && conversation == null)
        {
            throw new ArgumentException($"Conversation {conversationId} not found", nameof(conversationId));
        }

        if (conversation == null)
        {
            conversation = new Conversation
            {
                Id = convId,
                Title = $"Conversation {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                UserId = "default-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Conversations.Add(conversation);
        }

        // Validate context files if provided
        if (contextFileIds != null && contextFileIds.Count > 0)
        {
            var existingFiles = await _context.ContextFiles
                .Where(f => contextFileIds.Contains(f.Id))
                .Select(f => f.Id)
                .ToListAsync(cancellationToken);

            var missingFiles = contextFileIds.Except(existingFiles).ToList();
            if (missingFiles.Count > 0)
            {
                throw new ArgumentException($"Context files not found: {string.Join(", ", missingFiles)}", nameof(contextFileIds));
            }
        }

        var modelName = model ?? "gemini-1.5-flash";
        var inputTokens = EstimateTokenCount(prompt);
        var outputTokens = 50; // Mock value
        var latency = Random.Shared.Next(200, 1500);

        // Create prompt entity
        var promptEntity = new Prompt
        {
            Id = Guid.NewGuid(),
            ConversationId = convId,
            UserPrompt = prompt,
            Context = systemPrompt,
            ContextFileId = contextFileIds?.FirstOrDefault(),
            EstimatedTokens = inputTokens,
            ActualTokens = inputTokens + outputTokens,
            CreatedAt = DateTime.UtcNow
        };

        _context.Prompts.Add(promptEntity);

        // Create mock response
        var responseEntity = new Response
        {
            Id = Guid.NewGuid(),
            PromptId = promptEntity.Id,
            Provider = AiProvider.Google,
            Model = modelName,
            Content = $"This is a mock response to: {prompt.Substring(0, Math.Min(50, prompt.Length))}...",
            Tokens = outputTokens,
            Cost = CalculateCost(inputTokens, outputTokens, modelName),
            LatencyMs = latency,
            CreatedAt = DateTime.UtcNow
        };

        _context.Responses.Add(responseEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new PromptExecutionResult
        {
            PromptId = promptEntity.Id,
            ResponseId = responseEntity.Id,
            Content = responseEntity.Content,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            Cost = responseEntity.Cost,
            LatencyMs = latency,
            Model = modelName,
            Provider = AiProvider.Google,
            CreatedAt = responseEntity.CreatedAt
        };
    }

    public Task<TokenEstimate> EstimateTokensAsync(
        string prompt,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));

        var modelName = model ?? "gemini-1.5-flash";
        var tokenCount = EstimateTokenCount(prompt);
        var cost = CalculateCost(tokenCount, 0, modelName);

        return Task.FromResult(new TokenEstimate
        {
            TokenCount = tokenCount,
            EstimatedCost = cost,
            Model = modelName
        });
    }

    public async Task<PromptExecutionResult?> GetPromptByIdAsync(
        Guid promptId,
        CancellationToken cancellationToken = default)
    {
        var prompt = await _context.Prompts
            .Include(p => p.Responses)
            .FirstOrDefaultAsync(p => p.Id == promptId, cancellationToken);

        if (prompt == null || !prompt.Responses.Any())
            return null;

        var response = prompt.Responses.First();

        return new PromptExecutionResult
        {
            PromptId = prompt.Id,
            ResponseId = response.Id,
            Content = response.Content,
            InputTokens = prompt.ActualTokens - response.Tokens,
            OutputTokens = response.Tokens,
            Cost = response.Cost,
            LatencyMs = response.LatencyMs,
            Model = response.Model,
            Provider = response.Provider,
            CreatedAt = response.CreatedAt
        };
    }

    public async Task<List<PromptExecutionResult>> GetPromptsByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var prompts = await _context.Prompts
            .Include(p => p.Responses)
            .Where(p => p.ConversationId == conversationId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return prompts
            .Where(p => p.Responses.Any())
            .Select(p =>
            {
                var response = p.Responses.First();
                return new PromptExecutionResult
                {
                    PromptId = p.Id,
                    ResponseId = response.Id,
                    Content = response.Content,
                    InputTokens = p.ActualTokens - response.Tokens,
                    OutputTokens = response.Tokens,
                    Cost = response.Cost,
                    LatencyMs = response.LatencyMs,
                    Model = response.Model,
                    Provider = response.Provider,
                    CreatedAt = response.CreatedAt
                };
            })
            .ToList();
    }

    private static int EstimateTokenCount(string text)
    {
        // Simple estimation: ~4 characters per token
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    private static decimal CalculateCost(int inputTokens, int outputTokens, string model)
    {
        // Mock pricing (based on Gemini 1.5 Flash rates)
        decimal inputCostPer1k = 0.00007m;
        decimal outputCostPer1k = 0.0003m;

        if (model.Contains("pro", StringComparison.OrdinalIgnoreCase))
        {
            inputCostPer1k = 0.00125m;
            outputCostPer1k = 0.005m;
        }

        return (inputTokens / 1000m * inputCostPer1k) + (outputTokens / 1000m * outputCostPer1k);
    }
}
