using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using PromptLab.Core.Domain.Entities;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.Services.Interfaces;
using PromptLab.Infrastructure.Data;
using PromptLab.Infrastructure.Services;

namespace PromptLab.Tests.Services;

public class PromptExecutionServiceTests : IDisposable
{
    private readonly Mock<ILlmProvider> _mockLlmProvider;
    private readonly Mock<IRateLimitService> _mockRateLimitService;
    private readonly Mock<ILogger<PromptExecutionService>> _mockLogger;
    private readonly ApplicationDbContext _dbContext;
    private readonly PromptExecutionService _service;

    public PromptExecutionServiceTests()
    {
        _mockLlmProvider = new Mock<ILlmProvider>();
        _mockRateLimitService = new Mock<IRateLimitService>();
        _mockLogger = new Mock<ILogger<PromptExecutionService>>();

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new ApplicationDbContext(options);

        _service = new PromptExecutionService(
            _mockLlmProvider.Object,
            _mockRateLimitService.Object,
            _dbContext,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecutePromptAsync_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "Test prompt",
            Model = "gemini-pro",
            Provider = AiProvider.Google
        };

        _mockLlmProvider.Setup(x => x.Provider).Returns(AiProvider.Google);
        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRateLimitService.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitResult { IsAllowed = true });
        _mockLlmProvider.Setup(x => x.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LlmResponse
            {
                Content = "Test response",
                TokensUsed = 100,
                Cost = 0.001m,
                LatencyMs = 500,
                Model = "gemini-pro"
            });

        // Act
        var result = await _service.ExecutePromptAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Test response", result.Content);
        Assert.Equal(100, result.TokensUsed);
        Assert.Equal(0.001m, result.Cost);

        // Verify database persistence
        var savedPrompt = await _dbContext.Prompts.FirstOrDefaultAsync(p => p.Id == result.PromptId);
        var savedResponse = await _dbContext.Responses.FirstOrDefaultAsync(r => r.Id == result.ResponseId);

        Assert.NotNull(savedPrompt);
        Assert.NotNull(savedResponse);
        Assert.Equal("Test prompt", savedPrompt.UserPrompt);
        Assert.Equal("Test response", savedResponse.Content);
    }

    [Fact]
    public async Task ExecutePromptAsync_EmptyPrompt_ReturnsValidationError()
    {
        // Arrange
        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "",
            Model = "gemini-pro",
            Provider = AiProvider.Google
        };

        // Act
        var result = await _service.ExecutePromptAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User prompt cannot be empty", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecutePromptAsync_RateLimitExceeded_ReturnsError()
    {
        // Arrange
        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "Test prompt",
            Model = "gemini-pro",
            Provider = AiProvider.Google
        };

        _mockLlmProvider.Setup(x => x.Provider).Returns(AiProvider.Google);
        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRateLimitService.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitResult { IsAllowed = false, Message = "Rate limit exceeded" });

        // Act
        var result = await _service.ExecutePromptAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Rate limit exceeded", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecutePromptAsync_WithConversationHistory_LoadsPreviousMessages()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = "user123",
            Title = "Test Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var existingPrompt = new Prompt
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            UserPrompt = "Previous prompt",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var existingResponse = new Response
        {
            Id = Guid.NewGuid(),
            PromptId = existingPrompt.Id,
            Provider = AiProvider.Google,
            Model = "gemini-pro",
            Content = "Previous response",
            Tokens = 50,
            Cost = 0.0005m,
            LatencyMs = 300,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _dbContext.Conversations.Add(conversation);
        _dbContext.Prompts.Add(existingPrompt);
        _dbContext.Responses.Add(existingResponse);
        await _dbContext.SaveChangesAsync();

        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "New prompt",
            Model = "gemini-pro",
            Provider = AiProvider.Google,
            ConversationId = conversationId
        };

        _mockLlmProvider.Setup(x => x.Provider).Returns(AiProvider.Google);
        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRateLimitService.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitResult { IsAllowed = true });

        LlmRequest? capturedRequest = null;
        _mockLlmProvider.Setup(x => x.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .Callback<LlmRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new LlmResponse
            {
                Content = "New response",
                TokensUsed = 100,
                Cost = 0.001m,
                LatencyMs = 500,
                Model = "gemini-pro"
            });

        // Act
        var result = await _service.ExecutePromptAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(2, capturedRequest.ConversationHistory.Count);
        Assert.Equal("user", capturedRequest.ConversationHistory[0].Role);
        Assert.Equal("Previous prompt", capturedRequest.ConversationHistory[0].Content);
        Assert.Equal("assistant", capturedRequest.ConversationHistory[1].Role);
        Assert.Equal("Previous response", capturedRequest.ConversationHistory[1].Content);
    }

    [Fact]
    public async Task ExecutePromptAsync_WithContextFiles_IncludesContextInPrompt()
    {
        // Arrange
        var contextFileId = Guid.NewGuid();
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{contextFileId}.txt");
        await File.WriteAllTextAsync(tempFilePath, "Context file content");

        var contextFile = new ContextFile
        {
            Id = contextFileId,
            FileName = "test.txt",
            FileSize = 100,
            ContentType = "text/plain",
            StoragePath = tempFilePath,
            UploadedAt = DateTime.UtcNow
        };

        _dbContext.ContextFiles.Add(contextFile);
        await _dbContext.SaveChangesAsync();

        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "Test prompt",
            Model = "gemini-pro",
            Provider = AiProvider.Google,
            ContextFileIds = new List<Guid> { contextFileId }
        };

        _mockLlmProvider.Setup(x => x.Provider).Returns(AiProvider.Google);
        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRateLimitService.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitResult { IsAllowed = true });

        LlmRequest? capturedRequest = null;
        _mockLlmProvider.Setup(x => x.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .Callback<LlmRequest, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new LlmResponse
            {
                Content = "Test response",
                TokensUsed = 100,
                Cost = 0.001m,
                LatencyMs = 500,
                Model = "gemini-pro"
            });

        // Act
        var result = await _service.ExecutePromptAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Contains("Context file content", capturedRequest.UserPrompt);
        Assert.Contains("Test prompt", capturedRequest.UserPrompt);

        // Cleanup
        File.Delete(tempFilePath);
    }

    [Fact]
    public async Task ValidatePromptAsync_ValidRequest_ReturnsValid()
    {
        // Arrange
        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "Valid prompt",
            Model = "gemini-pro",
            Provider = AiProvider.Google
        };

        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ValidatePromptAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidatePromptAsync_EmptyPrompt_ReturnsInvalid()
    {
        // Arrange
        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "",
            Model = "gemini-pro",
            Provider = AiProvider.Google
        };

        // Act
        var result = await _service.ValidatePromptAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("User prompt cannot be empty"));
    }

    [Fact]
    public async Task ValidatePromptAsync_MissingModel_ReturnsInvalid()
    {
        // Arrange
        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "Test prompt",
            Model = "",
            Provider = AiProvider.Google
        };

        // Act
        var result = await _service.ValidatePromptAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Model must be specified"));
    }

    [Fact]
    public async Task ValidatePromptAsync_NonExistentConversation_ReturnsInvalid()
    {
        // Arrange
        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "Test prompt",
            Model = "gemini-pro",
            Provider = AiProvider.Google,
            ConversationId = Guid.NewGuid()
        };

        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ValidatePromptAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Conversation") && e.Contains("not found"));
    }

    [Fact]
    public async Task GetProviderStatusAsync_AvailableProvider_ReturnsAvailableStatus()
    {
        // Arrange
        _mockLlmProvider.Setup(x => x.Provider).Returns(AiProvider.Google);
        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockLlmProvider.Setup(x => x.GetAvailableModelsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "gemini-pro", "gemini-pro-vision" });

        // Act
        var statuses = await _service.GetProviderStatusAsync();
        var status = statuses.First();

        // Assert
        Assert.Equal(AiProvider.Google, status.Provider);
        Assert.True(status.IsAvailable);
        Assert.Equal(2, status.AvailableModels.Count());
    }

    [Fact]
    public async Task GetProviderStatusAsync_UnavailableProvider_ReturnsUnavailableStatus()
    {
        // Arrange
        _mockLlmProvider.Setup(x => x.Provider).Returns(AiProvider.Google);
        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var statuses = await _service.GetProviderStatusAsync();
        var status = statuses.First();

        // Assert
        Assert.Equal(AiProvider.Google, status.Provider);
        Assert.False(status.IsAvailable);
        Assert.Empty(status.AvailableModels);
    }

    [Fact]
    public async Task ExecutePromptAsync_CreatesNewConversation_WhenNotProvided()
    {
        // Arrange
        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "Test prompt",
            Model = "gemini-pro",
            Provider = AiProvider.Google
        };

        _mockLlmProvider.Setup(x => x.Provider).Returns(AiProvider.Google);
        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRateLimitService.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitResult { IsAllowed = true });
        _mockLlmProvider.Setup(x => x.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LlmResponse
            {
                Content = "Test response",
                TokensUsed = 100,
                Cost = 0.001m,
                LatencyMs = 500,
                Model = "gemini-pro"
            });

        // Act
        var result = await _service.ExecutePromptAsync(request);

        // Assert
        Assert.True(result.Success);

        var conversation = await _dbContext.Conversations.FirstOrDefaultAsync();
        Assert.NotNull(conversation);
        Assert.Equal("user123", conversation.UserId);
        Assert.Equal("Test prompt", conversation.Title);
    }

    [Fact]
    public async Task ExecutePromptAsync_RecordsRateLimitUsage_AfterSuccessfulExecution()
    {
        // Arrange
        var request = new PromptExecutionRequest
        {
            UserId = "user123",
            UserPrompt = "Test prompt",
            Model = "gemini-pro",
            Provider = AiProvider.Google
        };

        _mockLlmProvider.Setup(x => x.Provider).Returns(AiProvider.Google);
        _mockLlmProvider.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRateLimitService.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RateLimitResult { IsAllowed = true });
        _mockLlmProvider.Setup(x => x.GenerateAsync(It.IsAny<LlmRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LlmResponse
            {
                Content = "Test response",
                TokensUsed = 150,
                Cost = 0.001m,
                LatencyMs = 500,
                Model = "gemini-pro"
            });

        // Act
        await _service.ExecutePromptAsync(request);

        // Assert
        _mockRateLimitService.Verify(
            x => x.RecordUsageAsync("user123", 150, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
