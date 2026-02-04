using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PromptLab.Core.DTOs;
using PromptLab.Infrastructure.Configuration;
using PromptLab.Infrastructure.Providers;
using PromptLab.Infrastructure.Providers.Models;

namespace PromptLab.Tests.Providers;

public class GoogleGeminiProviderTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<GoogleGeminiProvider>> _loggerMock;
    private readonly GoogleGeminiConfig _config;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public GoogleGeminiProviderTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<GoogleGeminiProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        _config = new GoogleGeminiConfig
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://generativelanguage.googleapis.com",
            Model = "gemini-pro",
            ApiVersion = "v1",
            MaxRetries = 3,
            TimeoutSeconds = 30,
            InputTokenCostPer1K = 0.00025m,
            OutputTokenCostPer1K = 0.0005m
        };

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
    }

    [Fact]
    public void Constructor_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new GoogleGeminiProvider(null!, _httpClientFactoryMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullHttpClientFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new GoogleGeminiProvider(_config, null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, null!));
    }

    [Fact]
    public void ProviderName_ReturnsGoogleGemini()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);

        // Act
        var name = provider.ProviderName;

        // Assert
        name.Should().Be("Google Gemini");
    }

    [Fact]
    public async Task GenerateAsync_WithSuccessfulResponse_ReturnsLlmResponse()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);
        
        var geminiResponse = new GeminiGenerateResponse
        {
            Candidates = new List<GeminiCandidate>
            {
                new GeminiCandidate
                {
                    Content = new GeminiContent
                    {
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart { Text = "Generated content" }
                        }
                    },
                    FinishReason = "STOP",
                    Index = 0
                }
            },
            UsageMetadata = new GeminiUsageMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            }
        };

        var responseContent = JsonSerializer.Serialize(geminiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        var request = new LlmRequest
        {
            Prompt = "Test prompt",
            Model = "gemini-pro"
        };

        // Act
        var result = await provider.GenerateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Content.Should().Be("Generated content");
        result.Model.Should().Be("gemini-pro");
        result.PromptTokens.Should().Be(10);
        result.CompletionTokens.Should().Be(20);
        result.FinishReason.Should().Be("STOP");
        result.Cost.Should().BeGreaterThan(0);
        result.LatencyMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateAsync_WithHttpError_ReturnsFailedResponse()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad request error", Encoding.UTF8, "application/json")
            });

        var request = new LlmRequest
        {
            Prompt = "Test prompt",
            Model = "gemini-pro"
        };

        // Act
        var result = await provider.GenerateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("BadRequest");
        result.Content.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateAsync_WithEmptyCandidates_ReturnsFailedResponse()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);
        
        var geminiResponse = new GeminiGenerateResponse
        {
            Candidates = new List<GeminiCandidate>()
        };

        var responseContent = JsonSerializer.Serialize(geminiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        var request = new LlmRequest
        {
            Prompt = "Test prompt",
            Model = "gemini-pro"
        };

        // Act
        var result = await provider.GenerateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No content generated");
    }

    [Fact]
    public async Task GenerateAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GenerateAsync(null!));
    }

    [Fact]
    public async Task GenerateAsync_WithCustomModel_UsesSpecifiedModel()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);
        
        var geminiResponse = new GeminiGenerateResponse
        {
            Candidates = new List<GeminiCandidate>
            {
                new GeminiCandidate
                {
                    Content = new GeminiContent
                    {
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart { Text = "Generated content" }
                        }
                    }
                }
            },
            UsageMetadata = new GeminiUsageMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20
            }
        };

        var responseContent = JsonSerializer.Serialize(geminiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        var request = new LlmRequest
        {
            Prompt = "Test prompt",
            Model = "gemini-pro-vision"
        };

        // Act
        var result = await provider.GenerateAsync(request);

        // Assert
        result.Model.Should().Be("gemini-pro-vision");
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("gemini-pro-vision");
    }

    [Fact]
    public async Task GenerateAsync_WithTemperatureAndMaxTokens_IncludesGenerationConfig()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);
        
        var geminiResponse = new GeminiGenerateResponse
        {
            Candidates = new List<GeminiCandidate>
            {
                new GeminiCandidate
                {
                    Content = new GeminiContent
                    {
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart { Text = "Generated content" }
                        }
                    }
                }
            },
            UsageMetadata = new GeminiUsageMetadata()
        };

        var responseContent = JsonSerializer.Serialize(geminiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        string? capturedRequestBody = null;

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>(async (req, ct) => 
            {
                if (req.Content != null)
                {
                    capturedRequestBody = await req.Content.ReadAsStringAsync();
                }
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        var request = new LlmRequest
        {
            Prompt = "Test prompt",
            Model = "gemini-pro",
            Temperature = 0.7m,
            MaxTokens = 100
        };

        // Act
        await provider.GenerateAsync(request);

        // Assert
        capturedRequestBody.Should().NotBeNull();
        capturedRequestBody.Should().Contain("generationConfig");
        capturedRequestBody.Should().Contain("temperature");
        capturedRequestBody.Should().Contain("maxOutputTokens");
    }

    [Fact]
    public async Task EstimateTokensAsync_WithValidPrompt_ReturnsTokenCount()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);
        
        var countResponse = new GeminiCountTokensResponse
        {
            TotalTokens = 42
        };

        var responseContent = JsonSerializer.Serialize(countResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await provider.EstimateTokensAsync("Test prompt");

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public async Task EstimateTokensAsync_WithEmptyPrompt_ThrowsArgumentException()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => provider.EstimateTokensAsync(""));
    }

    [Fact]
    public async Task EstimateTokensAsync_WithHttpError_ReturnsZero()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Error", Encoding.UTF8, "application/json")
            });

        // Act
        var result = await provider.EstimateTokensAsync("Test prompt");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task IsAvailable_WithValidApiKey_ReturnsTrue()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);
        
        var countResponse = new GeminiCountTokensResponse
        {
            TotalTokens = 1
        };

        var responseContent = JsonSerializer.Serialize(countResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await provider.IsAvailable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailable_WithEmptyApiKey_ReturnsFalse()
    {
        // Arrange
        var configWithoutKey = new GoogleGeminiConfig
        {
            ApiKey = "",
            BaseUrl = "https://generativelanguage.googleapis.com",
            Model = "gemini-pro"
        };

        var provider = new GoogleGeminiProvider(configWithoutKey, _httpClientFactoryMock.Object, _loggerMock.Object);

        // Act
        var result = await provider.IsAvailable();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailable_WithHttpError_ReturnsFalse()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Unauthorized", Encoding.UTF8, "application/json")
            });

        // Act
        var result = await provider.IsAvailable();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateAsync_CalculatesCostCorrectly()
    {
        // Arrange
        var provider = new GoogleGeminiProvider(_config, _httpClientFactoryMock.Object, _loggerMock.Object);
        
        var geminiResponse = new GeminiGenerateResponse
        {
            Candidates = new List<GeminiCandidate>
            {
                new GeminiCandidate
                {
                    Content = new GeminiContent
                    {
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart { Text = "Generated content" }
                        }
                    }
                }
            },
            UsageMetadata = new GeminiUsageMetadata
            {
                PromptTokenCount = 1000,
                CandidatesTokenCount = 2000,
                TotalTokenCount = 3000
            }
        };

        var responseContent = JsonSerializer.Serialize(geminiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        var request = new LlmRequest
        {
            Prompt = "Test prompt"
        };

        // Act
        var result = await provider.GenerateAsync(request);

        // Assert
        // Cost = (1000/1000 * 0.00025) + (2000/1000 * 0.0005) = 0.00025 + 0.001 = 0.00125
        result.Cost.Should().Be(0.00125m);
    }
}
