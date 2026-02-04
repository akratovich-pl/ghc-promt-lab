using PromptLab.Core.Validators;

namespace PromptLab.Infrastructure.Validators;

/// <summary>
/// Validates prompt execution requests
/// </summary>
public class PromptValidator : IPromptValidator
{
    private const int MaxPromptLength = 100000; // 100k characters
    private const int MaxContextFilesCount = 10;

    public void ValidatePromptRequest(string prompt, List<Guid>? contextFileIds)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("User prompt cannot be empty", nameof(prompt));
        }

        if (prompt.Length > MaxPromptLength)
        {
            throw new ArgumentException(
                $"User prompt exceeds maximum length of {MaxPromptLength} characters", 
                nameof(prompt));
        }

        if (contextFileIds?.Count > MaxContextFilesCount)
        {
            throw new ArgumentException(
                $"Cannot attach more than {MaxContextFilesCount} context files", 
                nameof(contextFileIds));
        }
    }
}
