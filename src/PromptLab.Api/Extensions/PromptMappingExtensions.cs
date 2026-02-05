using PromptLab.Api.Models;
using PromptLab.Core.DTOs;

namespace PromptLab.Api.Extensions;

/// <summary>
/// Extension methods for mapping between service layer DTOs and API response models
/// </summary>
public static class PromptMappingExtensions
{
    /// <summary>
    /// Maps PromptExecutionResult to ExecutePromptResponse
    /// </summary>
    /// <param name="result">The prompt execution result to map</param>
    /// <returns>The mapped ExecutePromptResponse</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null</exception>
    public static ExecutePromptResponse ToResponse(this PromptExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ExecutePromptResponse
        {
            Id = result.PromptId,
            Content = result.Content,
            InputTokens = result.InputTokens,
            OutputTokens = result.OutputTokens,
            Cost = result.Cost,
            LatencyMs = result.LatencyMs,
            Model = result.Model,
            CreatedAt = result.CreatedAt
        };
    }

    /// <summary>
    /// Maps TokenEstimate to EstimateTokensResponse
    /// </summary>
    /// <param name="estimate">The token estimate to map</param>
    /// <returns>The mapped EstimateTokensResponse</returns>
    /// <exception cref="ArgumentNullException">Thrown when estimate is null</exception>
    public static EstimateTokensResponse ToResponse(this TokenEstimate estimate)
    {
        ArgumentNullException.ThrowIfNull(estimate);

        return new EstimateTokensResponse
        {
            TokenCount = estimate.TokenCount,
            EstimatedCost = estimate.EstimatedCost,
            Model = estimate.Model
        };
    }

    /// <summary>
    /// Maps PromptExecutionResult to PromptDetailResponse
    /// </summary>
    /// <param name="result">The prompt execution result to map</param>
    /// <param name="conversationId">The conversation ID to associate with the prompt</param>
    /// <returns>The mapped PromptDetailResponse</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null</exception>
    public static PromptDetailResponse ToDetailResponse(
        this PromptExecutionResult result,
        Guid conversationId)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new PromptDetailResponse
        {
            Id = result.PromptId,
            ConversationId = conversationId,
            UserPrompt = string.Empty,  // Would be fetched from DB
            Context = null,
            ContextFileId = null,
            EstimatedTokens = 0,
            ActualTokens = result.InputTokens + result.OutputTokens,
            CreatedAt = result.CreatedAt,
            Responses = new List<ResponseDetail>
            {
                result.ToResponseDetail()
            }
        };
    }

    /// <summary>
    /// Maps PromptExecutionResult to ResponseDetail (nested response object)
    /// </summary>
    /// <param name="result">The prompt execution result to map</param>
    /// <returns>The mapped ResponseDetail</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null</exception>
    public static ResponseDetail ToResponseDetail(this PromptExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ResponseDetail
        {
            Id = result.ResponseId,
            Provider = result.Provider.ToString(),
            Model = result.Model,
            Content = result.Content,
            Tokens = result.OutputTokens,
            Cost = result.Cost,
            LatencyMs = result.LatencyMs,
            CreatedAt = result.CreatedAt
        };
    }

    /// <summary>
    /// Maps a collection of PromptExecutionResult to a list of PromptDetailResponse
    /// </summary>
    /// <param name="results">The collection of prompt execution results to map</param>
    /// <param name="conversationId">The conversation ID to associate with all prompts</param>
    /// <returns>A list of mapped PromptDetailResponse objects</returns>
    /// <exception cref="ArgumentNullException">Thrown when results is null</exception>
    public static List<PromptDetailResponse> ToDetailResponses(
        this IEnumerable<PromptExecutionResult> results,
        Guid conversationId)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results
            .Select(result => result.ToDetailResponse(conversationId))
            .ToList();
    }
}
