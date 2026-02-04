namespace PromptLab.Core.Validators;

/// <summary>
/// Validator for prompt execution requests
/// </summary>
public interface IPromptValidator
{
    /// <summary>
    /// Validates the prompt execution request parameters
    /// </summary>
    /// <param name="prompt">The user prompt to validate</param>
    /// <param name="contextFileIds">Optional context file IDs to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    void ValidatePromptRequest(string prompt, List<Guid>? contextFileIds);
}
