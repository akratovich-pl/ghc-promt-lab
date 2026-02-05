using PromptLab.Api.Extensions;
using PromptLab.Api.Models;
using PromptLab.Core.DTOs;
using PromptLab.Core.Domain.Enums;

namespace PromptLab.Tests.Extensions;

public class PromptMappingExtensionsTests
{
    [Fact]
    public void ToResponse_WithValidPromptExecutionResult_MapsAllProperties()
    {
        // Arrange
        var promptId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        
        var result = new PromptExecutionResult
        {
            PromptId = promptId,
            ResponseId = responseId,
            Content = "Test response content",
            InputTokens = 100,
            OutputTokens = 200,
            Cost = 0.05m,
            LatencyMs = 1500,
            Model = "gemini-1.5-flash",
            Provider = AiProvider.Google,
            CreatedAt = createdAt
        };

        // Act
        var response = result.ToResponse();

        // Assert
        Assert.Equal(promptId, response.Id);
        Assert.Equal("Test response content", response.Content);
        Assert.Equal(100, response.InputTokens);
        Assert.Equal(200, response.OutputTokens);
        Assert.Equal(0.05m, response.Cost);
        Assert.Equal(1500, response.LatencyMs);
        Assert.Equal("gemini-1.5-flash", response.Model);
        Assert.Equal(createdAt, response.CreatedAt);
    }

    [Fact]
    public void ToResponse_WithNullPromptExecutionResult_ThrowsArgumentNullException()
    {
        // Arrange
        PromptExecutionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.ToResponse());
    }

    [Fact]
    public void ToResponse_WithValidTokenEstimate_MapsAllProperties()
    {
        // Arrange
        var estimate = new TokenEstimate
        {
            TokenCount = 500,
            EstimatedCost = 0.025m,
            Model = "gpt-4"
        };

        // Act
        var response = estimate.ToResponse();

        // Assert
        Assert.Equal(500, response.TokenCount);
        Assert.Equal(0.025m, response.EstimatedCost);
        Assert.Equal("gpt-4", response.Model);
    }

    [Fact]
    public void ToResponse_WithNullTokenEstimate_ThrowsArgumentNullException()
    {
        // Arrange
        TokenEstimate? estimate = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => estimate!.ToResponse());
    }

    [Fact]
    public void ToDetailResponse_WithValidPromptExecutionResultAndConversationId_MapsAllProperties()
    {
        // Arrange
        var promptId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        
        var result = new PromptExecutionResult
        {
            PromptId = promptId,
            ResponseId = responseId,
            Content = "Detailed response",
            InputTokens = 150,
            OutputTokens = 250,
            Cost = 0.075m,
            LatencyMs = 2000,
            Model = "claude-3-opus",
            Provider = AiProvider.Anthropic,
            CreatedAt = createdAt
        };

        // Act
        var detailResponse = result.ToDetailResponse(conversationId);

        // Assert
        Assert.Equal(promptId, detailResponse.Id);
        Assert.Equal(conversationId, detailResponse.ConversationId);
        Assert.Equal(string.Empty, detailResponse.UserPrompt);
        Assert.Null(detailResponse.Context);
        Assert.Null(detailResponse.ContextFileId);
        Assert.Equal(0, detailResponse.EstimatedTokens);
        Assert.Equal(400, detailResponse.ActualTokens); // InputTokens + OutputTokens
        Assert.Equal(createdAt, detailResponse.CreatedAt);
        Assert.Single(detailResponse.Responses);
        
        var responseDetail = detailResponse.Responses[0];
        Assert.Equal(responseId, responseDetail.Id);
        Assert.Equal("Anthropic", responseDetail.Provider);
        Assert.Equal("claude-3-opus", responseDetail.Model);
        Assert.Equal("Detailed response", responseDetail.Content);
        Assert.Equal(250, responseDetail.Tokens);
        Assert.Equal(0.075m, responseDetail.Cost);
        Assert.Equal(2000, responseDetail.LatencyMs);
        Assert.Equal(createdAt, responseDetail.CreatedAt);
    }

    [Fact]
    public void ToDetailResponse_WithNullPromptExecutionResult_ThrowsArgumentNullException()
    {
        // Arrange
        PromptExecutionResult? result = null;
        var conversationId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.ToDetailResponse(conversationId));
    }

    [Fact]
    public void ToDetailResponse_WithEmptyConversationId_MapsSuccessfully()
    {
        // Arrange
        var result = new PromptExecutionResult
        {
            PromptId = Guid.NewGuid(),
            ResponseId = Guid.NewGuid(),
            Content = "Test",
            InputTokens = 10,
            OutputTokens = 20,
            Cost = 0.01m,
            LatencyMs = 100,
            Model = "test-model",
            Provider = AiProvider.Google,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var detailResponse = result.ToDetailResponse(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, detailResponse.ConversationId);
    }

    [Fact]
    public void ToResponseDetail_WithValidPromptExecutionResult_MapsAllProperties()
    {
        // Arrange
        var responseId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        
        var result = new PromptExecutionResult
        {
            PromptId = Guid.NewGuid(),
            ResponseId = responseId,
            Content = "Response detail content",
            InputTokens = 50,
            OutputTokens = 100,
            Cost = 0.03m,
            LatencyMs = 800,
            Model = "gpt-3.5-turbo",
            Provider = AiProvider.OpenAI,
            CreatedAt = createdAt
        };

        // Act
        var responseDetail = result.ToResponseDetail();

        // Assert
        Assert.Equal(responseId, responseDetail.Id);
        Assert.Equal("OpenAI", responseDetail.Provider);
        Assert.Equal("gpt-3.5-turbo", responseDetail.Model);
        Assert.Equal("Response detail content", responseDetail.Content);
        Assert.Equal(100, responseDetail.Tokens); // OutputTokens
        Assert.Equal(0.03m, responseDetail.Cost);
        Assert.Equal(800, responseDetail.LatencyMs);
        Assert.Equal(createdAt, responseDetail.CreatedAt);
    }

    [Fact]
    public void ToResponseDetail_WithNullPromptExecutionResult_ThrowsArgumentNullException()
    {
        // Arrange
        PromptExecutionResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.ToResponseDetail());
    }

    [Fact]
    public void ToDetailResponses_WithEmptyCollection_ReturnsEmptyList()
    {
        // Arrange
        var results = new List<PromptExecutionResult>();
        var conversationId = Guid.NewGuid();

        // Act
        var responses = results.ToDetailResponses(conversationId);

        // Assert
        Assert.Empty(responses);
    }

    [Fact]
    public void ToDetailResponses_WithSingleItem_ReturnsSingleMappedResponse()
    {
        // Arrange
        var promptId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        
        var results = new List<PromptExecutionResult>
        {
            new PromptExecutionResult
            {
                PromptId = promptId,
                ResponseId = Guid.NewGuid(),
                Content = "Single response",
                InputTokens = 30,
                OutputTokens = 60,
                Cost = 0.02m,
                LatencyMs = 500,
                Model = "test-model",
                Provider = AiProvider.Google,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Act
        var responses = results.ToDetailResponses(conversationId);

        // Assert
        Assert.Single(responses);
        Assert.Equal(promptId, responses[0].Id);
        Assert.Equal(conversationId, responses[0].ConversationId);
        Assert.Equal(90, responses[0].ActualTokens); // 30 + 60
    }

    [Fact]
    public void ToDetailResponses_WithMultipleItems_ReturnsAllMappedResponses()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        
        var results = new List<PromptExecutionResult>
        {
            new PromptExecutionResult
            {
                PromptId = Guid.NewGuid(),
                ResponseId = Guid.NewGuid(),
                Content = "First response",
                InputTokens = 10,
                OutputTokens = 20,
                Cost = 0.01m,
                LatencyMs = 300,
                Model = "model-1",
                Provider = AiProvider.Google,
                CreatedAt = DateTime.UtcNow
            },
            new PromptExecutionResult
            {
                PromptId = Guid.NewGuid(),
                ResponseId = Guid.NewGuid(),
                Content = "Second response",
                InputTokens = 15,
                OutputTokens = 25,
                Cost = 0.015m,
                LatencyMs = 400,
                Model = "model-2",
                Provider = AiProvider.Anthropic,
                CreatedAt = DateTime.UtcNow
            },
            new PromptExecutionResult
            {
                PromptId = Guid.NewGuid(),
                ResponseId = Guid.NewGuid(),
                Content = "Third response",
                InputTokens = 20,
                OutputTokens = 30,
                Cost = 0.02m,
                LatencyMs = 500,
                Model = "model-3",
                Provider = AiProvider.OpenAI,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Act
        var responses = results.ToDetailResponses(conversationId);

        // Assert
        Assert.Equal(3, responses.Count);
        Assert.All(responses, r => Assert.Equal(conversationId, r.ConversationId));
        Assert.Equal("First response", responses[0].Responses[0].Content);
        Assert.Equal("Second response", responses[1].Responses[0].Content);
        Assert.Equal("Third response", responses[2].Responses[0].Content);
    }

    [Fact]
    public void ToDetailResponses_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<PromptExecutionResult>? results = null;
        var conversationId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => results!.ToDetailResponses(conversationId));
    }
}
