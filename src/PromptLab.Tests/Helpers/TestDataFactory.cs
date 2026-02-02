using PromptLab.Core.Domain.Entities;
using PromptLab.Core.Domain.Enums;

namespace PromptLab.Tests.Helpers;

/// <summary>
/// Factory class for creating test data
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// Creates a test conversation
    /// </summary>
    public static Conversation CreateTestConversation(
        string? userId = null,
        string? title = null)
    {
        return new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? "test-user-123",
            Title = title ?? "Test Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test prompt
    /// </summary>
    public static Prompt CreateTestPrompt(
        Guid conversationId,
        string? userPrompt = null,
        string? context = null,
        Guid? contextFileId = null)
    {
        return new Prompt
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            UserPrompt = userPrompt ?? "What is the capital of France?",
            Context = context,
            ContextFileId = contextFileId,
            EstimatedTokens = 10,
            ActualTokens = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test response
    /// </summary>
    public static Response CreateTestResponse(
        Guid promptId,
        AiProvider provider = AiProvider.Google,
        string? model = null,
        string? content = null,
        int tokens = 50,
        decimal cost = 0.001m,
        int latencyMs = 250)
    {
        return new Response
        {
            Id = Guid.NewGuid(),
            PromptId = promptId,
            Provider = provider,
            Model = model ?? "gemini-pro",
            Content = content ?? "The capital of France is Paris.",
            Tokens = tokens,
            Cost = cost,
            LatencyMs = latencyMs,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test context file
    /// </summary>
    public static ContextFile CreateTestContextFile(
        string? fileName = null,
        long fileSize = 1024,
        string? contentType = null)
    {
        return new ContextFile
        {
            Id = Guid.NewGuid(),
            FileName = fileName ?? "test-file.txt",
            FileSize = fileSize,
            ContentType = contentType ?? "text/plain",
            StoragePath = $"/test/path/{Guid.NewGuid()}.txt",
            UploadedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates sample prompts of different lengths
    /// </summary>
    public static class SamplePrompts
    {
        public static string Short => "Hello";
        
        public static string Medium => "Can you explain the concept of object-oriented programming in simple terms?";
        
        public static string Long => string.Join(" ", Enumerable.Repeat(
            "This is a very long prompt that contains many words to test the handling of large inputs.",
            20));
        
        public static string ExtremelyLong => string.Join(" ", Enumerable.Repeat(
            "This is an extremely long prompt designed to exceed typical token limits.",
            1000));
    }

    /// <summary>
    /// Creates sample API responses
    /// </summary>
    public static class SampleApiResponses
    {
        public static string SuccessResponse => @"{
            ""candidates"": [{
                ""content"": {
                    ""parts"": [{
                        ""text"": ""This is a test response from Gemini API.""
                    }]
                }
            }],
            ""usageMetadata"": {
                ""promptTokenCount"": 10,
                ""candidatesTokenCount"": 15,
                ""totalTokenCount"": 25
            }
        }";

        public static string ErrorResponse400 => @"{
            ""error"": {
                ""code"": 400,
                ""message"": ""Invalid request parameters"",
                ""status"": ""INVALID_ARGUMENT""
            }
        }";

        public static string ErrorResponse429 => @"{
            ""error"": {
                ""code"": 429,
                ""message"": ""Resource has been exhausted"",
                ""status"": ""RESOURCE_EXHAUSTED""
            }
        }";

        public static string ErrorResponse500 => @"{
            ""error"": {
                ""code"": 500,
                ""message"": ""Internal server error"",
                ""status"": ""INTERNAL""
            }
        }";

        public static string MalformedResponse => @"{
            ""unexpected"": ""format""
        }";
    }
}
