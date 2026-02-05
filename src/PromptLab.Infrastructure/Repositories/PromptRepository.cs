using Microsoft.EntityFrameworkCore;
using PromptLab.Core.Domain.Entities;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Repositories;
using PromptLab.Infrastructure.Data;

namespace PromptLab.Infrastructure.Repositories;

/// <summary>
/// Repository for managing prompt and response database operations
/// </summary>
public class PromptRepository : IPromptRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PromptRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<(Guid promptId, Guid responseId)> SavePromptExecutionAsync(
        Guid conversationId,
        string userPrompt,
        string? systemPrompt,
        Guid? contextFileId,
        LlmResponse llmResponse,
        int latencyMs,
        CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Ensure conversation exists
            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

            if (conversation == null)
            {
                // Create new conversation if it doesn't exist
                conversation = new Conversation
                {
                    Id = conversationId,
                    UserId = "system",
                    Title = "New Conversation",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.Conversations.Add(conversation);
            }
            else
            {
                // Update existing conversation timestamp
                conversation.UpdatedAt = DateTime.UtcNow;
            }

            // Save Prompt entity
            var promptEntity = new Prompt
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserPrompt = userPrompt,
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
                LatencyMs = latencyMs,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Responses.Add(responseEntity);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (promptEntity.Id, responseEntity.Id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Prompt?> GetPromptByIdAsync(
        Guid promptId, 
        CancellationToken cancellationToken)
    {
        return await _dbContext.Prompts
            .Include(p => p.Responses)
            .FirstOrDefaultAsync(p => p.Id == promptId, cancellationToken);
    }

    public async Task<List<Prompt>> GetPromptsByConversationIdAsync(
        Guid conversationId, 
        CancellationToken cancellationToken)
    {
        return await _dbContext.Prompts
            .Include(p => p.Responses)
            .Where(p => p.ConversationId == conversationId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
