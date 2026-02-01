namespace PromptLab.Api.Models;

/// <summary>
/// Response with token estimation
/// </summary>
public class EstimateTokensResponse
{
    /// <summary>
    /// Estimated number of tokens
    /// </summary>
    public int TokenCount { get; set; }

    /// <summary>
    /// Estimated cost in USD based on model pricing
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// Model used for estimation
    /// </summary>
    public string Model { get; set; } = string.Empty;
}
