using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromptLab.Infrastructure.Configuration;

namespace PromptLab.Tests.Configuration;

public class LlmProviderConfigServiceTests
{
    private readonly ILogger<LlmProviderConfigService> _logger;

    public LlmProviderConfigServiceTests()
    {
        _logger = new LoggerFactory().CreateLogger<LlmProviderConfigService>();
    }

    [Fact]
    public void GetBaseUrl_GoogleGemini_ReturnsCorrectUrl()
    {
        // Arrange
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.GetBaseUrl("GoogleGemini");

        // Assert
        Assert.Equal("https://generativelanguage.googleapis.com/v1beta", result);
    }

    [Fact]
    public void GetDefaultModel_GoogleGemini_ReturnsCorrectModel()
    {
        // Arrange
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.GetDefaultModel("GoogleGemini");

        // Assert
        Assert.Equal("gemini-pro", result);
    }

    [Fact]
    public void GetMaxTokens_GoogleGemini_ReturnsCorrectValue()
    {
        // Arrange
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.GetMaxTokens("GoogleGemini");

        // Assert
        Assert.Equal(8192, result);
    }

    [Fact]
    public void GetTemperature_GoogleGemini_ReturnsCorrectValue()
    {
        // Arrange
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.GetTemperature("GoogleGemini");

        // Assert
        Assert.Equal(0.7, result);
    }

    [Fact]
    public void IsProviderEnabled_GoogleGemini_ReturnsTrue()
    {
        // Arrange
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.IsProviderEnabled("GoogleGemini");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsProviderEnabled_DisabledProvider_ReturnsFalse()
    {
        // Arrange
        var options = CreateOptions(enabled: false);
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.IsProviderEnabled("GoogleGemini");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetApiKey_WithEnvironmentVariable_ReturnsKey()
    {
        // Arrange
        var testApiKey = "test-api-key-12345";
        Environment.SetEnvironmentVariable("GOOGLE_GEMINI_API_KEY", testApiKey);
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        try
        {
            // Act
            var result = service.GetApiKey("GoogleGemini");

            // Assert
            Assert.Equal(testApiKey, result);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("GOOGLE_GEMINI_API_KEY", null);
        }
    }

    [Fact]
    public void GetApiKey_WithoutEnvironmentVariable_ReturnsNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GOOGLE_GEMINI_API_KEY", null);
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.GetApiKey("GoogleGemini");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetBaseUrl_UnknownProvider_ReturnsNull()
    {
        // Arrange
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.GetBaseUrl("UnknownProvider");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetMaxTokens_UnknownProvider_ReturnsZero()
    {
        // Arrange
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.GetMaxTokens("UnknownProvider");

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void IsProviderEnabled_UnknownProvider_ReturnsFalse()
    {
        // Arrange
        var options = CreateOptions();
        var service = new LlmProviderConfigService(options, _logger);

        // Act
        var result = service.IsProviderEnabled("UnknownProvider");

        // Assert
        Assert.False(result);
    }

    private IOptions<LlmProvidersOptions> CreateOptions(bool enabled = true)
    {
        var llmOptions = new LlmProvidersOptions
        {
            GoogleGemini = new GoogleGeminiOptions
            {
                Enabled = enabled,
                BaseUrl = "https://generativelanguage.googleapis.com/v1beta",
                DefaultModel = "gemini-pro",
                MaxTokens = 8192,
                Temperature = 0.7
            }
        };

        return Options.Create(llmOptions);
    }
}
