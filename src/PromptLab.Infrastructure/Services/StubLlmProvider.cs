using PromptLab.Core.Domain.Enums;
using PromptLab.Core.Services.Interfaces;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Stub implementation of LLM provider - to be replaced with actual Google Gemini implementation
/// </summary>
public class StubLlmProvider : ILlmProvider
{
    public AiProvider Provider => AiProvider.Google;

    public Task<IEnumerable<string>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<string>>(new List<string>());
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Google Gemini provider not yet implemented");
    }
}
