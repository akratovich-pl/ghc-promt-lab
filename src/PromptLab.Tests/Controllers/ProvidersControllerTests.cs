using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PromptLab.Api.Controllers;
using PromptLab.Api.Models;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services;
using PromptLab.Core.Domain.Enums;

namespace PromptLab.Tests.Controllers;

public class ProvidersControllerTests
{
    private readonly Mock<IProviderService> _mockProviderService;
    private readonly Mock<ILogger<ProvidersController>> _mockLogger;
    private readonly ProvidersController _controller;

    public ProvidersControllerTests()
    {
        _mockProviderService = new Mock<IProviderService>();
        _mockLogger = new Mock<ILogger<ProvidersController>>();
        _controller = new ProvidersController(_mockProviderService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProviders_ReturnsOkWithProviderList()
    {
        // Arrange
        var expectedProviders = new List<ProviderInfo>
        {
            new ProviderInfo
            {
                Provider = AiProvider.Google,
                Name = "Google",
                IsAvailable = true,
                SupportedModels = new List<string> { "gemini-1.5-flash", "gemini-1.5-pro" }
            },
            new ProviderInfo
            {
                Provider = AiProvider.OpenAI,
                Name = "OpenAI",
                IsAvailable = false,
                SupportedModels = new List<string> { "gpt-4o", "gpt-3.5-turbo" }
            }
        };

        _mockProviderService
            .Setup(s => s.GetProvidersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProviders);

        // Act
        var result = await _controller.GetProviders(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<ActionResult<List<ProviderInfoResponse>>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var providers = Assert.IsType<List<ProviderInfoResponse>>(objectResult.Value);
        
        Assert.Equal(2, providers.Count);
        Assert.Equal("Google", providers[0].Name);
        Assert.True(providers[0].IsAvailable);
        Assert.Equal("OpenAI", providers[1].Name);
        Assert.False(providers[1].IsAvailable);
    }

    [Fact]
    public async Task GetProviderStatus_ValidProvider_ReturnsOkWithStatus()
    {
        // Arrange
        var providerName = "Google";
        var expectedStatus = new ProviderStatus
        {
            Provider = AiProvider.Google,
            Name = "Google",
            IsHealthy = true,
            ErrorMessage = null,
            LastChecked = DateTime.UtcNow
        };

        _mockProviderService
            .Setup(s => s.GetProviderStatusAsync(providerName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.GetProviderStatus(providerName, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<ActionResult<ProviderStatusResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var status = Assert.IsType<ProviderStatusResponse>(objectResult.Value);
        
        Assert.Equal("Google", status.Name);
        Assert.True(status.IsHealthy);
        Assert.Null(status.ErrorMessage);
    }

    [Fact]
    public async Task GetProviderStatus_InvalidProvider_ReturnsNotFound()
    {
        // Arrange
        var providerName = "InvalidProvider";

        _mockProviderService
            .Setup(s => s.GetProviderStatusAsync(providerName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException($"Unknown provider: {providerName}"));

        // Act
        var result = await _controller.GetProviderStatus(providerName, CancellationToken.None);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ProviderStatusResponse>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(404, problemDetails.Status);
    }

    [Fact]
    public async Task GetProviderModels_ValidProvider_ReturnsOkWithModels()
    {
        // Arrange
        var providerName = "Google";
        var expectedModels = new List<ModelInfo>
        {
            new ModelInfo
            {
                Name = "gemini-1.5-flash",
                DisplayName = "Gemini 1.5 Flash",
                Provider = AiProvider.Google,
                MaxTokens = 8192,
                InputCostPer1kTokens = 0.00007m,
                OutputCostPer1kTokens = 0.0003m
            },
            new ModelInfo
            {
                Name = "gemini-1.5-pro",
                DisplayName = "Gemini 1.5 Pro",
                Provider = AiProvider.Google,
                MaxTokens = 8192,
                InputCostPer1kTokens = 0.00125m,
                OutputCostPer1kTokens = 0.005m
            }
        };

        _mockProviderService
            .Setup(s => s.GetProviderModelsAsync(providerName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedModels);

        // Act
        var result = await _controller.GetProviderModels(providerName, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<ActionResult<List<ModelInfoResponse>>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var models = Assert.IsType<List<ModelInfoResponse>>(objectResult.Value);
        
        Assert.Equal(2, models.Count);
        Assert.Equal("gemini-1.5-flash", models[0].Name);
        Assert.Equal(8192, models[0].MaxTokens);
        Assert.Equal("gemini-1.5-pro", models[1].Name);
    }

    [Fact]
    public async Task GetProviderModels_InvalidProvider_ReturnsNotFound()
    {
        // Arrange
        var providerName = "InvalidProvider";

        _mockProviderService
            .Setup(s => s.GetProviderModelsAsync(providerName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException($"Unknown provider: {providerName}"));

        // Act
        var result = await _controller.GetProviderModels(providerName, CancellationToken.None);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<ModelInfoResponse>>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(404, problemDetails.Status);
    }
}
