using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Repositories;
using PromptLab.Core.Services;
using PromptLab.Infrastructure.Services;

namespace PromptLab.Tests.Services.Pipeline;

public class PromptPersistenceServiceTests
{
    private readonly Mock<IPromptRepository> _promptRepositoryMock;
    private readonly Mock<IConversationHistoryService> _conversationHistoryServiceMock;
    private readonly Mock<ILogger<PromptPersistenceService>> _loggerMock;
    private readonly PromptPersistenceService _service;

    public PromptPersistenceServiceTests()
    {
        _promptRepositoryMock = new Mock<IPromptRepository>();
        _conversationHistoryServiceMock = new Mock<IConversationHistoryService>();
        _loggerMock = new Mock<ILogger<PromptPersistenceService>>();

        _service = new PromptPersistenceService(
            _promptRepositoryMock.Object,
            _conversationHistoryServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PromptPersistenceService(
            null!,
            _conversationHistoryServiceMock.Object,
            _loggerMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullConversationHistoryService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PromptPersistenceService(
            _promptRepositoryMock.Object,
            null!,
            _loggerMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PromptPersistenceService(
            _promptRepositoryMock.Object,
            _conversationHistoryServiceMock.Object,
            null!
        ));
    }

    [Fact]
    public async Task PersistAsync_WithNewConversation_CreatesConversationAndSavesPrompt()
    {
        // Arrange
        var prompt = "Test prompt";
        var systemPrompt = "System instruction";
        Guid? conversationId = null; // New conversation
        var userId = "user-123";
        var correlationId = "correlation-123";
        var latencyMs = 500;

        var executionResult = new LlmExecutionResult
        {
            Response = new LlmResponse
            {
                Content = "Test response",
                Model = "gemini-1.5-flash",
                PromptTokens = 10,
                CompletionTokens = 20,
                Cost = 0.001m
            },
            Provider = AiProvider.Google,
            ProviderName = "Google Gemini",
            ActualModel = "gemini-1.5-flash",
            ExecutionTimestamp = DateTime.UtcNow,
            ContextFileId = Guid.NewGuid()
        };

        var newConversationId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        var responseId = Guid.NewGuid();

        _conversationHistoryServiceMock
            .Setup(s => s.EnsureConversationAsync(conversationId, userId, prompt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newConversationId);

        _promptRepositoryMock
            .Setup(r => r.SavePromptExecutionAsync(
                newConversationId,
                prompt,
                systemPrompt,
                executionResult.ContextFileId,
                executionResult.Response,
                latencyMs,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((promptId, responseId));

        _conversationHistoryServiceMock
            .Setup(s => s.UpdateConversationTimestampAsync(newConversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.PersistAsync(
            prompt, systemPrompt, conversationId, executionResult,
            correlationId, userId, latencyMs, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromptId.Should().Be(promptId);
        result.ResponseId.Should().Be(responseId);
        result.ConversationId.Should().Be(newConversationId);
        result.IsNewConversation.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _conversationHistoryServiceMock.Verify(
            s => s.EnsureConversationAsync(conversationId, userId, prompt, It.IsAny<CancellationToken>()),
            Times.Once);
        _promptRepositoryMock.Verify(
            r => r.SavePromptExecutionAsync(
                newConversationId, prompt, systemPrompt, executionResult.ContextFileId,
                executionResult.Response, latencyMs, It.IsAny<CancellationToken>()),
            Times.Once);
        _conversationHistoryServiceMock.Verify(
            s => s.UpdateConversationTimestampAsync(newConversationId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PersistAsync_WithExistingConversation_UsesExistingConversation()
    {
        // Arrange
        var prompt = "Test prompt";
        var systemPrompt = "System instruction";
        var conversationId = Guid.NewGuid(); // Existing conversation
        var userId = "user-123";
        var correlationId = "correlation-123";
        var latencyMs = 500;

        var executionResult = new LlmExecutionResult
        {
            Response = new LlmResponse
            {
                Content = "Test response",
                Model = "gemini-1.5-flash",
                PromptTokens = 10,
                CompletionTokens = 20,
                Cost = 0.001m
            },
            Provider = AiProvider.Google,
            ProviderName = "Google Gemini",
            ActualModel = "gemini-1.5-flash",
            ExecutionTimestamp = DateTime.UtcNow,
            ContextFileId = null
        };

        var promptId = Guid.NewGuid();
        var responseId = Guid.NewGuid();

        _conversationHistoryServiceMock
            .Setup(s => s.EnsureConversationAsync(conversationId, userId, prompt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId);

        _promptRepositoryMock
            .Setup(r => r.SavePromptExecutionAsync(
                conversationId,
                prompt,
                systemPrompt,
                executionResult.ContextFileId,
                executionResult.Response,
                latencyMs,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((promptId, responseId));

        _conversationHistoryServiceMock
            .Setup(s => s.UpdateConversationTimestampAsync(conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.PersistAsync(
            prompt, systemPrompt, conversationId, executionResult,
            correlationId, userId, latencyMs, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromptId.Should().Be(promptId);
        result.ResponseId.Should().Be(responseId);
        result.ConversationId.Should().Be(conversationId);
        result.IsNewConversation.Should().BeFalse();

        _conversationHistoryServiceMock.Verify(
            s => s.EnsureConversationAsync(conversationId, userId, prompt, It.IsAny<CancellationToken>()),
            Times.Once);
        _promptRepositoryMock.Verify(
            r => r.SavePromptExecutionAsync(
                conversationId, prompt, systemPrompt, executionResult.ContextFileId,
                executionResult.Response, latencyMs, It.IsAny<CancellationToken>()),
            Times.Once);
        _conversationHistoryServiceMock.Verify(
            s => s.UpdateConversationTimestampAsync(conversationId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PersistAsync_WithoutSystemPrompt_SavesSuccessfully()
    {
        // Arrange
        var prompt = "Test prompt";
        string? systemPrompt = null;
        var conversationId = Guid.NewGuid();
        var userId = "user-123";
        var correlationId = "correlation-123";
        var latencyMs = 500;

        var executionResult = new LlmExecutionResult
        {
            Response = new LlmResponse
            {
                Content = "Test response",
                Model = "gemini-1.5-flash",
                PromptTokens = 10,
                CompletionTokens = 20,
                Cost = 0.001m
            },
            Provider = AiProvider.Google,
            ProviderName = "Google Gemini",
            ActualModel = "gemini-1.5-flash",
            ExecutionTimestamp = DateTime.UtcNow,
            ContextFileId = null
        };

        var promptId = Guid.NewGuid();
        var responseId = Guid.NewGuid();

        _conversationHistoryServiceMock
            .Setup(s => s.EnsureConversationAsync(conversationId, userId, prompt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId);

        _promptRepositoryMock
            .Setup(r => r.SavePromptExecutionAsync(
                conversationId,
                prompt,
                systemPrompt,
                executionResult.ContextFileId,
                executionResult.Response,
                latencyMs,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((promptId, responseId));

        _conversationHistoryServiceMock
            .Setup(s => s.UpdateConversationTimestampAsync(conversationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.PersistAsync(
            prompt, systemPrompt, conversationId, executionResult,
            correlationId, userId, latencyMs, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PromptId.Should().Be(promptId);
        result.ResponseId.Should().Be(responseId);

        _promptRepositoryMock.Verify(
            r => r.SavePromptExecutionAsync(
                conversationId, prompt, null, executionResult.ContextFileId,
                executionResult.Response, latencyMs, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PersistAsync_WhenRepositoryFails_PropagatesException()
    {
        // Arrange
        var prompt = "Test prompt";
        var systemPrompt = "System instruction";
        var conversationId = Guid.NewGuid();
        var userId = "user-123";
        var correlationId = "correlation-123";
        var latencyMs = 500;

        var executionResult = new LlmExecutionResult
        {
            Response = new LlmResponse
            {
                Content = "Test response",
                Model = "gemini-1.5-flash",
                PromptTokens = 10,
                CompletionTokens = 20,
                Cost = 0.001m
            },
            Provider = AiProvider.Google,
            ProviderName = "Google Gemini",
            ActualModel = "gemini-1.5-flash",
            ExecutionTimestamp = DateTime.UtcNow,
            ContextFileId = null
        };

        _conversationHistoryServiceMock
            .Setup(s => s.EnsureConversationAsync(conversationId, userId, prompt, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId);

        _promptRepositoryMock
            .Setup(r => r.SavePromptExecutionAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid?>(),
                It.IsAny<LlmResponse>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PersistAsync(
                prompt, systemPrompt, conversationId, executionResult,
                correlationId, userId, latencyMs, CancellationToken.None));

        // Verify timestamp update was never called
        _conversationHistoryServiceMock.Verify(
            s => s.UpdateConversationTimestampAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
