using System.Text.Json.Serialization;

namespace PromptLab.Infrastructure.Providers.Models;

/// <summary>
/// Request model for Google Gemini generateContent endpoint
/// </summary>
public class GeminiGenerateRequest
{
    [JsonPropertyName("contents")]
    public required List<GeminiContent> Contents { get; set; }

    [JsonPropertyName("generationConfig")]
    public GeminiGenerationConfig? GenerationConfig { get; set; }
}

public class GeminiContent
{
    [JsonPropertyName("parts")]
    public required List<GeminiPart> Parts { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class GeminiGenerationConfig
{
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("maxOutputTokens")]
    public int? MaxOutputTokens { get; set; }

    [JsonPropertyName("topP")]
    public double? TopP { get; set; }

    [JsonPropertyName("topK")]
    public int? TopK { get; set; }
}
