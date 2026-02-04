using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services;
using PromptLab.Infrastructure.Data;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Domain service that manages conversation history and lifecycle
/// </summary>
public class ConversationHistoryService : IConversationHistoryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ConversationHistoryService> _logger;

    private const int MaxConversationTitleLength = 50;
    private const int TruncatedTitleLength = 47;

    public ConversationHistoryService(
        ApplicationDbContext dbContext,
        ILogger<ConversationHistoryService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ConversationMessage>> LoadConversationHistoryAsync(
        Guid conversationId, 
        CancellationToken cancellationToken)
    {
        var conversationHistory = new List<ConversationMessage>();

        var prompts = await _dbContext.Prompts
            .Include(p => p.Responses)
            .Where(p => p.ConversationId == conversationId)
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

        _logger.LogInformation(
            "Loaded {Count} previous messages from conversation {ConversationId}",
            conversationHistory.Count, conversationId);

        return conversationHistory;
    }

    public async Task<Guid> EnsureConversationAsync(
        Guid? conversationId, 
        string userId, 
        string initialPrompt, 
        CancellationToken cancellationToken)
    {
        if (conversationId.HasValue)
        {
            return conversationId.Value;
        }

        var conversation = new Core.Domain.Entities.Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = initialPrompt.Length > MaxConversationTitleLength 
                ? initialPrompt[..TruncatedTitleLength] + "..." 
                : initialPrompt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created new conversation {ConversationId}", conversation.Id);

        return conversation.Id;
    }

    public async Task UpdateConversationTimestampAsync(
        Guid conversationId, 
        CancellationToken cancellationToken)
    {
        var conversation = await _dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
        
        if (conversation != null)
        {
            conversation.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
