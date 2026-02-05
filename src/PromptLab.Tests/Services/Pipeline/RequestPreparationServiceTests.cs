using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PromptLab.Core.Builders;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services;
using PromptLab.Core.Validators;
using PromptLab.Infrastructure.Services;

namespace PromptLab.Tests.Services.Pipeline;

public class RequestPreparationServiceTests
{
    private readonly Mock<IPromptValidator> _promptValidatorMock;
    private readonly Mock<IConversationHistoryService> _conversationHistoryServiceMock;
    private readonly Mock<IPromptEnricher> _promptEnricherMock;
    private readonly Mock<ILlmRequestBuilder> _llmRequestBuilderMock;
    private readonly Mock<ILogger<RequestPreparationService>> _loggerMock;
    private readonly RequestPreparationService _service;

    public RequestPreparationServiceTests()
    {
        _promptValidatorMock = new Mock<IPromptValidator>();
        _conversationHistoryServiceMock = new Mock<IConversationHistoryService>();
        _promptEnricherMock = new Mock<IPromptEnricher>();
        _llmRequestBuilderMock = new Mock<ILlmRequestBuilder>();
        _loggerMock = new Mock<ILogger<RequestPreparationService>>();

        _service = new RequestPreparationService(
            _promptValidatorMock.Object,
            _conversationHistoryServiceMock.Object,
            _promptEnricherMock.Object,
            _llmRequestBuilderMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public void Constructor_WithNullValidator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RequestPreparationService(
            null!,
            _conversationHistoryServiceMock.Object,
            _promptEnricherMock.Object,
            _llmRequestBuilderMock.Object,
            _loggerMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullConversationHistoryService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RequestPreparationService(
            _promptValidatorMock.Object,
            null!,
            _promptEnricherMock.Object,
            _llmRequestBuilderMock.Object,
            _loggerMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullEnricher_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RequestPreparationService(
            _promptValidatorMock.Object,
            _conversationHistoryServiceMock.Object,
            null!,
            _llmRequestBuilderMock.Object,
            _loggerMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullRequestBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RequestPreparationService(
            _promptValidatorMock.Object,
            _conversationHistoryServiceMock.Object,
            _promptEnricherMock.Object,
            null!,
            _loggerMock.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RequestPreparationService(
            _promptValidatorMock.Object,
            _conversationHistoryServiceMock.Object,
            _promptEnricherMock.Object,
            _llmRequestBuilderMock.Object,
            null!
        ));
    }

    [Fact]
    public async Task PrepareAsync_WithValidInputNoConversation_ReturnsValidRequest()
    {
        // Arrange
        var prompt = "Test prompt";
        var systemPrompt = "Test system";
        var userId = "test-user";
        var model = "gemini-1.5-flash";
        var enrichedPrompt = "Enriched test prompt";
        var contextFileId = Guid.NewGuid();

        _promptEnricherMock
            .Setup(x => x.EnrichPromptWithContextAsync(prompt, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((enrichedPrompt, contextFileId));

        var expectedLlmRequest = new LlmRequest
        {
            Prompt = enrichedPrompt,
            SystemPrompt = systemPrompt,
            Model = model
        };

        _llmRequestBuilderMock
            .Setup(x => x.BuildRequest(
                enrichedPrompt,
                systemPrompt,
                model,
                It.IsAny<List<ConversationMessage>>(),
                null,
                null))
            .Returns(expectedLlmRequest);

        // Act
        var result = await _service.PrepareAsync(
            AiProvider.Google, prompt, systemPrompt, null, null, model, null, null, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Provider.Should().Be(AiProvider.Google);
        result.LlmRequest.Should().Be(expectedLlmRequest);
        result.ContextFileId.Should().Be(contextFileId);
        result.ConversationHistory.Should().BeEmpty();
        result.UserId.Should().Be(userId);
        result.Model.Should().Be(model);

        _promptValidatorMock.Verify(x => x.ValidatePromptRequest(prompt, null), Times.Once);
        _promptEnricherMock.Verify(x => x.EnrichPromptWithContextAsync(prompt, null, It.IsAny<CancellationToken>()), Times.Once);
        _llmRequestBuilderMock.Verify(x => x.BuildRequest(enrichedPrompt, systemPrompt, model, It.IsAny<List<ConversationMessage>>(), null, null), Times.Once);
        _conversationHistoryServiceMock.Verify(x => x.LoadConversationHistoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PrepareAsync_WithConversationId_LoadsHistory()
    {
        // Arrange
        var prompt = "Test prompt";
        var conversationId = Guid.NewGuid();
        var userId = "test-user";
        var enrichedPrompt = "Enriched prompt";

        var conversationHistory = new List<ConversationMessage>
        {
            new() { Role = "user", Content = "Previous message" },
            new() { Role = "assistant", Content = "Previous response" }
        };

        _conversationHistoryServiceMock
            .Setup(x => x.LoadConversationHistoryAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationHistory);

        _promptEnricherMock
            .Setup(x => x.EnrichPromptWithContextAsync(prompt, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((enrichedPrompt, (Guid?)null));

        var expectedLlmRequest = new LlmRequest
        {
            Prompt = enrichedPrompt,
            Model = "gemini-1.5-flash",
            ConversationHistory = conversationHistory
        };

        _llmRequestBuilderMock
            .Setup(x => x.BuildRequest(
                enrichedPrompt,
                null,
                "gemini-1.5-flash",
                conversationHistory,
                null,
                null))
            .Returns(expectedLlmRequest);

        // Act
        var result = await _service.PrepareAsync(
            AiProvider.Google, prompt, null, conversationId, null, null, null, null, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConversationHistory.Should().HaveCount(2);
        result.ConversationHistory.Should().BeEquivalentTo(conversationHistory);

        _conversationHistoryServiceMock.Verify(
            x => x.LoadConversationHistoryAsync(conversationId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task PrepareAsync_WithNullModel_UsesDefaultModel()
    {
        // Arrange
        var prompt = "Test prompt";
        var userId = "test-user";
        var enrichedPrompt = "Enriched prompt";

        _promptEnricherMock
            .Setup(x => x.EnrichPromptWithContextAsync(prompt, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((enrichedPrompt, (Guid?)null));

        var expectedLlmRequest = new LlmRequest
        {
            Prompt = enrichedPrompt,
            Model = "gemini-1.5-flash"
        };

        _llmRequestBuilderMock
            .Setup(x => x.BuildRequest(
                enrichedPrompt,
                null,
                "gemini-1.5-flash",
                It.IsAny<List<ConversationMessage>>(),
                null,
                null))
            .Returns(expectedLlmRequest);

        // Act
        var result = await _service.PrepareAsync(
            AiProvider.Google, prompt, null, null, null, null, null, null, userId, CancellationToken.None);

        // Assert
        result.Model.Should().Be("gemini-1.5-flash");
        _llmRequestBuilderMock.Verify(
            x => x.BuildRequest(enrichedPrompt, null, "gemini-1.5-flash", It.IsAny<List<ConversationMessage>>(), null, null), 
            Times.Once);
    }

    [Fact]
    public async Task PrepareAsync_WithContextFiles_EnrichesPrompt()
    {
        // Arrange
        var prompt = "Test prompt";
        var contextFileIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var userId = "test-user";
        var enrichedPrompt = "Enriched with context";
        var firstContextFileId = contextFileIds[0];

        _promptEnricherMock
            .Setup(x => x.EnrichPromptWithContextAsync(prompt, contextFileIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync((enrichedPrompt, firstContextFileId));

        var expectedLlmRequest = new LlmRequest
        {
            Prompt = enrichedPrompt,
            Model = "gemini-1.5-flash"
        };

        _llmRequestBuilderMock
            .Setup(x => x.BuildRequest(
                enrichedPrompt,
                null,
                "gemini-1.5-flash",
                It.IsAny<List<ConversationMessage>>(),
                null,
                null))
            .Returns(expectedLlmRequest);

        // Act
        var result = await _service.PrepareAsync(
            AiProvider.Google, prompt, null, null, contextFileIds, null, null, null, userId, CancellationToken.None);

        // Assert
        result.LlmRequest.Prompt.Should().Be(enrichedPrompt);
        result.ContextFileId.Should().Be(firstContextFileId);

        _promptValidatorMock.Verify(x => x.ValidatePromptRequest(prompt, contextFileIds), Times.Once);
        _promptEnricherMock.Verify(
            x => x.EnrichPromptWithContextAsync(prompt, contextFileIds, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task PrepareAsync_WithMaxTokensAndTemperature_PassesToBuilder()
    {
        // Arrange
        var prompt = "Test prompt";
        var userId = "test-user";
        var enrichedPrompt = "Enriched prompt";
        var maxTokens = 1000;
        var temperature = 0.5;

        _promptEnricherMock
            .Setup(x => x.EnrichPromptWithContextAsync(prompt, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((enrichedPrompt, (Guid?)null));

        var expectedLlmRequest = new LlmRequest
        {
            Prompt = enrichedPrompt,
            Model = "gemini-1.5-flash",
            MaxTokens = maxTokens,
            Temperature = (decimal)temperature
        };

        _llmRequestBuilderMock
            .Setup(x => x.BuildRequest(
                enrichedPrompt,
                null,
                "gemini-1.5-flash",
                It.IsAny<List<ConversationMessage>>(),
                maxTokens,
                temperature))
            .Returns(expectedLlmRequest);

        // Act
        var result = await _service.PrepareAsync(
            AiProvider.Google, prompt, null, null, null, null, maxTokens, temperature, userId, CancellationToken.None);

        // Assert
        _llmRequestBuilderMock.Verify(
            x => x.BuildRequest(enrichedPrompt, null, "gemini-1.5-flash", It.IsAny<List<ConversationMessage>>(), maxTokens, temperature),
            Times.Once);
    }

    [Fact]
    public async Task PrepareAsync_CallsValidatorFirst()
    {
        // Arrange
        var prompt = "Test prompt";
        var userId = "test-user";
        var contextFileIds = new List<Guid> { Guid.NewGuid() };
        var callOrder = new List<string>();

        _promptValidatorMock
            .Setup(x => x.ValidatePromptRequest(prompt, contextFileIds))
            .Callback(() => callOrder.Add("validate"));

        _promptEnricherMock
            .Setup(x => x.EnrichPromptWithContextAsync(prompt, contextFileIds, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("enrich"))
            .ReturnsAsync(("enriched", (Guid?)null));

        _llmRequestBuilderMock
            .Setup(x => x.BuildRequest(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<ConversationMessage>>(),
                It.IsAny<int?>(),
                It.IsAny<double?>()))
            .Callback(() => callOrder.Add("build"))
            .Returns(new LlmRequest { Prompt = "enriched", Model = "gemini-1.5-flash" });

        // Act
        await _service.PrepareAsync(
            AiProvider.Google, prompt, null, null, contextFileIds, null, null, null, userId, CancellationToken.None);

        // Assert
        callOrder.Should().HaveCount(3);
        callOrder[0].Should().Be("validate");
        callOrder[1].Should().Be("enrich");
        callOrder[2].Should().Be("build");
    }
}
