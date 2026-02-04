namespace PromptLab.Core.Builders;

/// <summary>
/// Enriches prompts with context from files
/// </summary>
public interface IPromptEnricher
{
    /// <summary>
    /// Loads context files and enriches the user prompt with their content
    /// </summary>
    /// <param name="prompt">The original user prompt</param>
    /// <param name="contextFileIds">List of context file IDs to load</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the enriched prompt and the first context file ID (if any)</returns>
    Task<(string enrichedPrompt, Guid? firstContextFileId)> EnrichPromptWithContextAsync(
        string prompt, 
        List<Guid>? contextFileIds, 
        CancellationToken cancellationToken);
}
