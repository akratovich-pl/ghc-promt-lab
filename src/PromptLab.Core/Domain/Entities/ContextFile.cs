namespace PromptLab.Core.Domain.Entities;

/// <summary>
/// Represents a file uploaded as context for prompts
/// </summary>
public class ContextFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    // Navigation property
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
