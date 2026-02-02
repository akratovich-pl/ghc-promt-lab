using System.Net;
using Moq;
using Moq.Protected;

namespace PromptLab.Tests.Helpers;

/// <summary>
/// Helper class for mocking HTTP message handlers for external API calls
/// </summary>
public static class MockHttpMessageHandlerFactory
{
    /// <summary>
    /// Creates a mock HTTP message handler that returns a successful Gemini API response
    /// </summary>
    public static Mock<HttpMessageHandler> CreateSuccessfulGeminiResponse(
        string? responseContent = null,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(
                    responseContent ?? TestDataFactory.SampleApiResponses.SuccessResponse,
                    System.Text.Encoding.UTF8,
                    "application/json")
            });

        return mockHandler;
    }

    /// <summary>
    /// Creates a mock HTTP message handler that returns an error response
    /// </summary>
    public static Mock<HttpMessageHandler> CreateErrorGeminiResponse(
        HttpStatusCode statusCode,
        string? errorContent = null)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        
        var content = statusCode switch
        {
            HttpStatusCode.BadRequest => errorContent ?? TestDataFactory.SampleApiResponses.ErrorResponse400,
            HttpStatusCode.TooManyRequests => errorContent ?? TestDataFactory.SampleApiResponses.ErrorResponse429,
            HttpStatusCode.InternalServerError => errorContent ?? TestDataFactory.SampleApiResponses.ErrorResponse500,
            _ => errorContent ?? TestDataFactory.SampleApiResponses.ErrorResponse500
        };

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(
                    content,
                    System.Text.Encoding.UTF8,
                    "application/json")
            });

        return mockHandler;
    }

    /// <summary>
    /// Creates a mock HTTP message handler that simulates a network timeout
    /// </summary>
    public static Mock<HttpMessageHandler> CreateTimeoutResponse()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        return mockHandler;
    }

    /// <summary>
    /// Creates a mock HTTP message handler that returns a malformed response
    /// </summary>
    public static Mock<HttpMessageHandler> CreateMalformedResponse()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    TestDataFactory.SampleApiResponses.MalformedResponse,
                    System.Text.Encoding.UTF8,
                    "application/json")
            });

        return mockHandler;
    }

    /// <summary>
    /// Creates a mock HTTP message handler with rate limit headers
    /// </summary>
    public static Mock<HttpMessageHandler> CreateResponseWithRateLimitHeaders(
        int remainingRequests,
        int limitRequests,
        DateTimeOffset? resetTime = null)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                TestDataFactory.SampleApiResponses.SuccessResponse,
                System.Text.Encoding.UTF8,
                "application/json")
        };

        response.Headers.Add("X-RateLimit-Limit", limitRequests.ToString());
        response.Headers.Add("X-RateLimit-Remaining", remainingRequests.ToString());
        response.Headers.Add("X-RateLimit-Reset", 
            (resetTime ?? DateTimeOffset.UtcNow.AddMinutes(1)).ToUnixTimeSeconds().ToString());

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return mockHandler;
    }
}
