using System.Text.Json.Serialization;

namespace PromptLab.Infrastructure.Providers.Models;

/// <summary>
/// Response model from Google Gemini list models endpoint
/// </summary>
public class GeminiModelsResponse
{
    [JsonPropertyName("models")]
    public List<GeminiModel>? Models { get; set; }
}

public class GeminiModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("inputTokenLimit")]
    public int? InputTokenLimit { get; set; }

    [JsonPropertyName("outputTokenLimit")]
    public int? OutputTokenLimit { get; set; }

    [JsonPropertyName("supportedGenerationMethods")]
    public List<string>? SupportedGenerationMethods { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("topP")]
    public double? TopP { get; set; }

    [JsonPropertyName("topK")]
    public int? TopK { get; set; }
}
