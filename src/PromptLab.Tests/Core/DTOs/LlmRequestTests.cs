using System.ComponentModel.DataAnnotations;
using PromptLab.Core.DTOs;

namespace PromptLab.Tests.Core.DTOs;

public class LlmRequestTests
{
    [Fact]
    public void LlmRequest_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var request = new LlmRequest(
            Prompt: "Test prompt",
            SystemPrompt: "Test system prompt",
            Model: "gemini-pro",
            MaxTokens: 1024,
            Temperature: 0.5
        );

        // Assert
        Assert.Equal("Test prompt", request.Prompt);
        Assert.Equal("Test system prompt", request.SystemPrompt);
        Assert.Equal("gemini-pro", request.Model);
        Assert.Equal(1024, request.MaxTokens);
        Assert.Equal(0.5, request.Temperature);
    }

    [Fact]
    public void LlmRequest_WithDefaultValues_ShouldUseDefaults()
    {
        // Arrange & Act
        var request = new LlmRequest(
            Prompt: "Test prompt",
            SystemPrompt: null,
            Model: "gemini-pro"
        );

        // Assert
        Assert.Equal(2048, request.MaxTokens);
        Assert.Equal(0.7, request.Temperature);
    }

    [Fact]
    public void LlmRequest_WithNullSystemPrompt_ShouldBeValid()
    {
        // Arrange & Act
        var request = new LlmRequest(
            Prompt: "Test prompt",
            SystemPrompt: null,
            Model: "gemini-pro"
        );

        // Assert
        Assert.Null(request.SystemPrompt);
        Assert.NotNull(request.Prompt);
        Assert.NotNull(request.Model);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(2.0)]
    public void LlmRequest_WithValidTemperature_ShouldBeValid(double temperature)
    {
        // Arrange & Act
        var request = new LlmRequest(
            Prompt: "Test prompt",
            SystemPrompt: null,
            Model: "gemini-pro",
            Temperature: temperature
        );

        // Assert
        Assert.Equal(temperature, request.Temperature);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(100000)]
    public void LlmRequest_WithValidMaxTokens_ShouldBeValid(int maxTokens)
    {
        // Arrange & Act
        var request = new LlmRequest(
            Prompt: "Test prompt",
            SystemPrompt: null,
            Model: "gemini-pro",
            MaxTokens: maxTokens
        );

        // Assert
        Assert.Equal(maxTokens, request.MaxTokens);
    }
}
