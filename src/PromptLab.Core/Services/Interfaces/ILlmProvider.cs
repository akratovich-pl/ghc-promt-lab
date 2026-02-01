using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;

namespace PromptLab.Core.Services.Interfaces;

/// <summary>
/// Defines the contract for LLM provider implementations.
/// Providers implement this interface to integrate with different LLM APIs (Google Gemini, OpenAI, etc.).
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Gets the name of the LLM provider (e.g., "Google Gemini", "OpenAI").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Generates text content using the LLM based on the provided request.
    /// </summary>
    /// <param name="request">The request containing prompt, model, and generation parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The response containing generated content and usage metrics.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not available or configured.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the number of tokens in the provided text for the specified model.
    /// </summary>
    /// <param name="request">The request containing text and model for token estimation.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The response containing the estimated token count.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not available or configured.</exception>
    Task<TokenEstimateResponse> EstimateTokensAsync(TokenEstimateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the LLM provider is available and properly configured.
    /// </summary>
    /// <returns>True if the provider is ready to process requests; otherwise, false.</returns>
    bool IsAvailable();
}
