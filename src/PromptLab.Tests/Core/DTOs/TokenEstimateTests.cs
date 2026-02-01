using PromptLab.Core.DTOs;

namespace PromptLab.Tests.Core.DTOs;

public class TokenEstimateTests
{
    [Fact]
    public void TokenEstimateRequest_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var request = new TokenEstimateRequest(
            Text: "Sample text to estimate tokens",
            Model: "gemini-pro"
        );

        // Assert
        Assert.Equal("Sample text to estimate tokens", request.Text);
        Assert.Equal("gemini-pro", request.Model);
    }

    [Fact]
    public void TokenEstimateResponse_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var response = new TokenEstimateResponse(
            TokenCount: 42,
            Model: "gemini-pro"
        );

        // Assert
        Assert.Equal(42, response.TokenCount);
        Assert.Equal("gemini-pro", response.Model);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(100000)]
    public void TokenEstimateResponse_WithVariousTokenCounts_ShouldStoreCorrectly(int tokenCount)
    {
        // Arrange & Act
        var response = new TokenEstimateResponse(
            TokenCount: tokenCount,
            Model: "test-model"
        );

        // Assert
        Assert.Equal(tokenCount, response.TokenCount);
    }

    [Fact]
    public void TokenEstimateRequest_WithEmptyText_ShouldBeAllowed()
    {
        // Arrange & Act
        var request = new TokenEstimateRequest(
            Text: "",
            Model: "gemini-pro"
        );

        // Assert
        Assert.Equal("", request.Text);
    }
}
