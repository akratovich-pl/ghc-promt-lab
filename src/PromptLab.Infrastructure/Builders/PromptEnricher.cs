using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Builders;
using PromptLab.Infrastructure.Data;
using System.Text;

namespace PromptLab.Infrastructure.Builders;

/// <summary>
/// Enriches prompts with context from files
/// </summary>
public class PromptEnricher : IPromptEnricher
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PromptEnricher> _logger;

    public PromptEnricher(
        ApplicationDbContext dbContext,
        ILogger<PromptEnricher> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(string enrichedPrompt, Guid? firstContextFileId)> EnrichPromptWithContextAsync(
        string prompt, 
        List<Guid>? contextFileIds, 
        CancellationToken cancellationToken)
    {
        if (contextFileIds?.Any() != true)
        {
            return (prompt, null);
        }

        var firstContextFileId = contextFileIds.First();
        var contextFiles = await _dbContext.ContextFiles
            .Where(cf => contextFileIds.Contains(cf.Id))
            .ToListAsync(cancellationToken);

        if (!contextFiles.Any())
        {
            return (prompt, null);
        }

        var contextBuilder = new StringBuilder();
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
                else
                {
                    _logger.LogWarning("Context file not found at path: {Path}", file.StoragePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading context file: {FileId}", file.Id);
            }
        }

        var contextContent = contextBuilder.ToString();
        if (string.IsNullOrWhiteSpace(contextContent))
        {
            return (prompt, firstContextFileId);
        }

        var enrichedPrompt = $"Context:\n{contextContent}\n\nUser Request:\n{prompt}";
        return (enrichedPrompt, firstContextFileId);
    }
}
