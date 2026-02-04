using System.Text.Json.Serialization;

namespace PromptLab.Infrastructure.Providers.Models;

/// <summary>
/// Request model for Google Gemini countTokens endpoint
/// </summary>
public class GeminiCountTokensRequest
{
    [JsonPropertyName("contents")]
    public required List<GeminiContent> Contents { get; set; }
}

/// <summary>
/// Response model from Google Gemini countTokens endpoint
/// </summary>
public class GeminiCountTokensResponse
{
    [JsonPropertyName("totalTokens")]
    public int TotalTokens { get; set; }
}
