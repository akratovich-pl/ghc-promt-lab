using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PromptLab.Api.Controllers;
using PromptLab.Api.Models;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services;
using PromptLab.Core.Domain.Enums;

namespace PromptLab.Tests.Controllers;

public class PromptsControllerTests
{
    private readonly Mock<IPromptExecutionService> _mockPromptService;
    private readonly Mock<ILogger<PromptsController>> _mockLogger;
    private readonly PromptsController _controller;

    public PromptsControllerTests()
    {
        _mockPromptService = new Mock<IPromptExecutionService>();
        _mockLogger = new Mock<ILogger<PromptsController>>();
        _controller = new PromptsController(_mockPromptService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecutePrompt_ValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var request = new ExecutePromptRequest
        {
            Prompt = "What is AI?",
            Model = "gemini-1.5-flash"
        };

        var expectedResult = new PromptExecutionResult
        {
            PromptId = Guid.NewGuid(),
            ResponseId = Guid.NewGuid(),
            Content = "AI is artificial intelligence",
            InputTokens = 10,
            OutputTokens = 20,
            Cost = 0.001m,
            LatencyMs = 500,
            Model = "gemini-1.5-flash",
            Provider = AiProvider.Google,
            CreatedAt = DateTime.UtcNow
        };

        _mockPromptService
            .Setup(s => s.ExecutePromptAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<List<Guid>?>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<double?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ExecutePrompt(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<ActionResult<ExecutePromptResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<ExecutePromptResponse>(objectResult.Value);
        
        Assert.Equal(expectedResult.Content, response.Content);
        Assert.Equal(expectedResult.InputTokens, response.InputTokens);
        Assert.Equal(expectedResult.OutputTokens, response.OutputTokens);
        Assert.Equal(expectedResult.Model, response.Model);
    }

    [Fact]
    public async Task ExecutePrompt_ServiceThrowsArgumentException_ThrowsException()
    {
        // Arrange
        var request = new ExecutePromptRequest { Prompt = "Test" };

        _mockPromptService
            .Setup(s => s.ExecutePromptAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<List<Guid>?>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<double?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid prompt"));

        // Act & Assert
        // Exception is now handled by GlobalExceptionFilter, not in controller
        await Assert.ThrowsAsync<ArgumentException>(
            () => _controller.ExecutePrompt(request, CancellationToken.None));
    }

    [Fact]
    public async Task EstimateTokens_ValidRequest_ReturnsOkWithEstimate()
    {
        // Arrange
        var request = new EstimateTokensRequest
        {
            Prompt = "What is AI?",
            Model = "gemini-1.5-flash"
        };

        var expectedEstimate = new TokenEstimate
        {
            TokenCount = 10,
            EstimatedCost = 0.0001m,
            Model = "gemini-1.5-flash"
        };

        _mockPromptService
            .Setup(s => s.EstimateTokensAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEstimate);

        // Act
        var result = await _controller.EstimateTokens(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<ActionResult<EstimateTokensResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<EstimateTokensResponse>(objectResult.Value);
        
        Assert.Equal(expectedEstimate.TokenCount, response.TokenCount);
        Assert.Equal(expectedEstimate.EstimatedCost, response.EstimatedCost);
        Assert.Equal(expectedEstimate.Model, response.Model);
    }

    [Fact]
    public async Task GetPromptById_ExistingPrompt_ReturnsOkWithDetails()
    {
        // Arrange
        var promptId = Guid.NewGuid();
        var expectedResult = new PromptExecutionResult
        {
            PromptId = promptId,
            ResponseId = Guid.NewGuid(),
            Content = "Test response",
            InputTokens = 10,
            OutputTokens = 20,
            Cost = 0.001m,
            LatencyMs = 500,
            Model = "gemini-1.5-flash",
            Provider = AiProvider.Google,
            CreatedAt = DateTime.UtcNow
        };

        _mockPromptService
            .Setup(s => s.GetPromptByIdAsync(promptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetPromptById(promptId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<ActionResult<PromptDetailResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<PromptDetailResponse>(objectResult.Value);
        
        Assert.Equal(promptId, response.Id);
        Assert.Single(response.Responses);
    }

    [Fact]
    public async Task GetPromptById_NonExistingPrompt_ReturnsNotFound()
    {
        // Arrange
        var promptId = Guid.NewGuid();

        _mockPromptService
            .Setup(s => s.GetPromptByIdAsync(promptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PromptExecutionResult?)null);

        // Act
        var result = await _controller.GetPromptById(promptId, CancellationToken.None);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PromptDetailResponse>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(404, problemDetails.Status);
    }

    [Fact]
    public async Task GetPromptsByConversation_ExistingConversation_ReturnsOkWithList()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var expectedResults = new List<PromptExecutionResult>
        {
            new PromptExecutionResult
            {
                PromptId = Guid.NewGuid(),
                ResponseId = Guid.NewGuid(),
                Content = "Response 1",
                InputTokens = 10,
                OutputTokens = 20,
                Cost = 0.001m,
                LatencyMs = 500,
                Model = "gemini-1.5-flash",
                Provider = AiProvider.Google,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockPromptService
            .Setup(s => s.GetPromptsByConversationIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _controller.GetPromptsByConversation(conversationId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<ActionResult<List<PromptDetailResponse>>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var responses = Assert.IsType<List<PromptDetailResponse>>(objectResult.Value);
        
        Assert.Single(responses);
        Assert.Equal(expectedResults[0].PromptId, responses[0].Id);
    }
}
