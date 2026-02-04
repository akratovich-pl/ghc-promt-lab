using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Providers;
using PromptLab.Infrastructure.Services;

namespace PromptLab.Tests.Services.Pipeline;

public class LlmExecutionServiceTests
{
    private readonly Mock<ILlmProvider> _googleProviderMock;
    private readonly Mock<ILlmProvider> _groqProviderMock;
    private readonly Mock<ILogger<LlmExecutionService>> _loggerMock;
    private readonly LlmExecutionService _service;

    public LlmExecutionServiceTests()
    {
        _googleProviderMock = new Mock<ILlmProvider>();
        _googleProviderMock.Setup(p => p.Provider).Returns(AiProvider.Google);
        _googleProviderMock.Setup(p => p.ProviderName).Returns("Google Gemini");

        _groqProviderMock = new Mock<ILlmProvider>();
        _groqProviderMock.Setup(p => p.Provider).Returns(AiProvider.Groq);
        _groqProviderMock.Setup(p => p.ProviderName).Returns("Groq");

        _loggerMock = new Mock<ILogger<LlmExecutionService>>();

        var providers = new List<ILlmProvider> { _googleProviderMock.Object, _groqProviderMock.Object };
        _service = new LlmExecutionService(providers, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullProviders_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LlmExecutionService(
            null!,
            _loggerMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LlmExecutionService(
            new List<ILlmProvider> { _googleProviderMock.Object },
            null!
        ));
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.ExecuteAsync(null!, "correlation-123", CancellationToken.None)
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithGeminiModel_RoutesToGoogleProvider()
    {
        // Arrange
        var request = new PreparedPromptRequest
        {
            LlmRequest = new LlmRequest
            {
                Prompt = "Test prompt",
                Model = "gemini-1.5-flash"
            },
            ContextFileId = null,
            ConversationHistory = new List<ConversationMessage>(),
            UserId = "user-123",
            Model = "gemini-1.5-flash"
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Test response",
            Model = "gemini-1.5-flash",
            PromptTokens = 10,
            CompletionTokens = 20,
            Cost = 0.01m,
            LatencyMs = 100,
            Success = true
        };

        _googleProviderMock
            .Setup(p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.ExecuteAsync(request, "correlation-123", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Response.Should().Be(expectedResponse);
        result.Provider.Should().Be(AiProvider.Google);
        result.ProviderName.Should().Be("Google Gemini");
        result.ActualModel.Should().Be("gemini-1.5-flash");
        result.ContextFileId.Should().BeNull();

        _googleProviderMock.Verify(
            p => p.GenerateAsync(It.Is<LlmRequest>(r => r.Model == "gemini-1.5-flash"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _groqProviderMock.Verify(
            p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithLlamaModel_RoutesToGroqProvider()
    {
        // Arrange
        var request = new PreparedPromptRequest
        {
            LlmRequest = new LlmRequest
            {
                Prompt = "Test prompt",
                Model = "llama-3.3-70b-versatile"
            },
            ContextFileId = Guid.NewGuid(),
            ConversationHistory = new List<ConversationMessage>(),
            UserId = "user-123",
            Model = "llama-3.3-70b-versatile"
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Test response from Groq",
            Model = "llama-3.3-70b-versatile",
            PromptTokens = 15,
            CompletionTokens = 25,
            Cost = 0.02m,
            LatencyMs = 150,
            Success = true
        };

        _groqProviderMock
            .Setup(p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.ExecuteAsync(request, "correlation-456", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Response.Should().Be(expectedResponse);
        result.Provider.Should().Be(AiProvider.Groq);
        result.ProviderName.Should().Be("Groq");
        result.ActualModel.Should().Be("llama-3.3-70b-versatile");
        result.ContextFileId.Should().Be(request.ContextFileId);

        _groqProviderMock.Verify(
            p => p.GenerateAsync(It.Is<LlmRequest>(r => r.Model == "llama-3.3-70b-versatile"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _googleProviderMock.Verify(
            p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedModel_ThrowsInvalidOperationException()
    {
        // Arrange
        var emptyProviders = new List<ILlmProvider>();
        var serviceWithoutProviders = new LlmExecutionService(emptyProviders, _loggerMock.Object);

        var request = new PreparedPromptRequest
        {
            LlmRequest = new LlmRequest
            {
                Prompt = "Test prompt",
                Model = "unknown-model"
            },
            ContextFileId = null,
            ConversationHistory = new List<ConversationMessage>(),
            UserId = "user-123",
            Model = "unknown-model"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            serviceWithoutProviders.ExecuteAsync(request, "correlation-789", CancellationToken.None)
        );

        exception.Message.Should().Contain("No provider found for model");
    }

    [Fact]
    public async Task ExecuteAsync_WhenProviderThrows_PropagatesException()
    {
        // Arrange
        var request = new PreparedPromptRequest
        {
            LlmRequest = new LlmRequest
            {
                Prompt = "Test prompt",
                Model = "gemini-1.5-flash"
            },
            ContextFileId = null,
            ConversationHistory = new List<ConversationMessage>(),
            UserId = "user-123",
            Model = "gemini-1.5-flash"
        };

        _googleProviderMock
            .Setup(p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Provider API error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            _service.ExecuteAsync(request, "correlation-error", CancellationToken.None)
        );

        exception.Message.Should().Contain("Provider API error");
    }

    [Fact]
    public async Task ExecuteAsync_SetsExecutionTimestamp()
    {
        // Arrange
        var beforeExecution = DateTime.UtcNow;
        
        var request = new PreparedPromptRequest
        {
            LlmRequest = new LlmRequest
            {
                Prompt = "Test prompt",
                Model = "gemini-1.5-flash"
            },
            ContextFileId = null,
            ConversationHistory = new List<ConversationMessage>(),
            UserId = "user-123",
            Model = "gemini-1.5-flash"
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Test response",
            Model = "gemini-1.5-flash",
            PromptTokens = 10,
            CompletionTokens = 20,
            Cost = 0.01m,
            LatencyMs = 100,
            Success = true
        };

        _googleProviderMock
            .Setup(p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.ExecuteAsync(request, "correlation-123", CancellationToken.None);

        var afterExecution = DateTime.UtcNow;

        // Assert
        result.ExecutionTimestamp.Should().BeOnOrAfter(beforeExecution);
        result.ExecutionTimestamp.Should().BeOnOrBefore(afterExecution);
    }

    [Fact]
    public async Task ExecuteAsync_WithGptModel_RoutesToGroqProvider()
    {
        // Arrange
        var request = new PreparedPromptRequest
        {
            LlmRequest = new LlmRequest
            {
                Prompt = "Test prompt",
                Model = "gpt-4o-mini"
            },
            ContextFileId = null,
            ConversationHistory = new List<ConversationMessage>(),
            UserId = "user-123",
            Model = "gpt-4o-mini"
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Test response",
            Model = "gpt-4o-mini",
            PromptTokens = 10,
            CompletionTokens = 20,
            Cost = 0.01m,
            LatencyMs = 100,
            Success = true
        };

        _groqProviderMock
            .Setup(p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.ExecuteAsync(request, "correlation-gpt", CancellationToken.None);

        // Assert
        result.Provider.Should().Be(AiProvider.Groq);
        _groqProviderMock.Verify(
            p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithMixtralModel_RoutesToGroqProvider()
    {
        // Arrange
        var request = new PreparedPromptRequest
        {
            LlmRequest = new LlmRequest
            {
                Prompt = "Test prompt",
                Model = "mixtral-8x7b-32768"
            },
            ContextFileId = null,
            ConversationHistory = new List<ConversationMessage>(),
            UserId = "user-123",
            Model = "mixtral-8x7b-32768"
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Test response",
            Model = "mixtral-8x7b-32768",
            PromptTokens = 10,
            CompletionTokens = 20,
            Cost = 0.01m,
            LatencyMs = 100,
            Success = true
        };

        _groqProviderMock
            .Setup(p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.ExecuteAsync(request, "correlation-mixtral", CancellationToken.None);

        // Assert
        result.Provider.Should().Be(AiProvider.Groq);
        _groqProviderMock.Verify(
            p => p.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
