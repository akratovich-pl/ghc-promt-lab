using PromptLab.Core.DTOs;

namespace PromptLab.Tests.Core.DTOs;

public class LlmResponseTests
{
    [Fact]
    public void LlmResponse_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var response = new LlmResponse
        {
            Content = "Generated content",
            PromptTokens = 100,
            CompletionTokens = 200,
            Cost = 0.05m,
            LatencyMs = 1500,
            Model = "gemini-pro",
            Success = true
        };

        // Assert
        Assert.Equal("Generated content", response.Content);
        Assert.Equal(100, response.PromptTokens);
        Assert.Equal(200, response.CompletionTokens);
        Assert.Equal(0.05m, response.Cost);
        Assert.Equal(1500, response.LatencyMs);
        Assert.Equal("gemini-pro", response.Model);
        Assert.True(response.Success);
    }

    [Fact]
    public void LlmResponse_WithZeroTokens_ShouldBeValid()
    {
        // Arrange & Act
        var response = new LlmResponse
        {
            Content = "Generated content",
            PromptTokens = 0,
            CompletionTokens = 0,
            Cost = 0.0m,
            LatencyMs = 0,
            Model = "gemini-pro",
            Success = true
        };

        // Assert
        Assert.Equal(0, response.PromptTokens);
        Assert.Equal(0, response.CompletionTokens);
        Assert.Equal(0.0m, response.Cost);
        Assert.Equal(0, response.LatencyMs);
    }

    [Theory]
    [InlineData(100, 50, 0.01)]
    [InlineData(1000, 500, 0.10)]
    [InlineData(5000, 2000, 0.50)]
    public void LlmResponse_WithVariousTokenCounts_ShouldStoreCorrectly(int promptTokens, int completionTokens, decimal cost)
    {
        // Arrange & Act
        var response = new LlmResponse
        {
            Content = "Test content",
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            Cost = cost,
            LatencyMs = 1000,
            Model = "test-model",
            Success = true
        };

        // Assert
        Assert.Equal(promptTokens, response.PromptTokens);
        Assert.Equal(completionTokens, response.CompletionTokens);
        Assert.Equal(cost, response.Cost);
    }
}
