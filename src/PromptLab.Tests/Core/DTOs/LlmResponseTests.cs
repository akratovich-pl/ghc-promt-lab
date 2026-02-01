using PromptLab.Core.DTOs;

namespace PromptLab.Tests.Core.DTOs;

public class LlmResponseTests
{
    [Fact]
    public void LlmResponse_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var response = new LlmResponse(
            Content: "Generated content",
            InputTokens: 100,
            OutputTokens: 200,
            Cost: 0.05m,
            LatencyMs: 1500,
            Model: "gemini-pro"
        );

        // Assert
        Assert.Equal("Generated content", response.Content);
        Assert.Equal(100, response.InputTokens);
        Assert.Equal(200, response.OutputTokens);
        Assert.Equal(0.05m, response.Cost);
        Assert.Equal(1500, response.LatencyMs);
        Assert.Equal("gemini-pro", response.Model);
    }

    [Fact]
    public void LlmResponse_WithZeroTokens_ShouldBeValid()
    {
        // Arrange & Act
        var response = new LlmResponse(
            Content: "Generated content",
            InputTokens: 0,
            OutputTokens: 0,
            Cost: 0.0m,
            LatencyMs: 0,
            Model: "gemini-pro"
        );

        // Assert
        Assert.Equal(0, response.InputTokens);
        Assert.Equal(0, response.OutputTokens);
        Assert.Equal(0.0m, response.Cost);
        Assert.Equal(0, response.LatencyMs);
    }

    [Theory]
    [InlineData(100, 50, 0.01)]
    [InlineData(1000, 500, 0.10)]
    [InlineData(5000, 2000, 0.50)]
    public void LlmResponse_WithVariousTokenCounts_ShouldStoreCorrectly(int inputTokens, int outputTokens, decimal cost)
    {
        // Arrange & Act
        var response = new LlmResponse(
            Content: "Test content",
            InputTokens: inputTokens,
            OutputTokens: outputTokens,
            Cost: cost,
            LatencyMs: 1000,
            Model: "test-model"
        );

        // Assert
        Assert.Equal(inputTokens, response.InputTokens);
        Assert.Equal(outputTokens, response.OutputTokens);
        Assert.Equal(cost, response.Cost);
    }
}
