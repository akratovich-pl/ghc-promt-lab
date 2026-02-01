using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PromptLab.Api.Middleware;
using PromptLab.Core.Configuration;
using PromptLab.Core.Services.Interfaces;

namespace PromptLab.Tests.Middleware;

public class RateLimitMiddlewareTests
{
    private readonly Mock<IRateLimitService> _rateLimitService;
    private readonly Mock<ILogger<RateLimitMiddleware>> _logger;
    private readonly RateLimitingOptions _options;

    public RateLimitMiddlewareTests()
    {
        _rateLimitService = new Mock<IRateLimitService>();
        _logger = new Mock<ILogger<RateLimitMiddleware>>();
        _options = new RateLimitingOptions
        {
            RequestsPerMinute = 60,
            RequestsPerHour = 1000,
            Enabled = true
        };
    }

    [Fact]
    public async Task InvokeAsync_WhenRateLimitDisabled_CallsNext()
    {
        // Arrange
        var disabledOptions = new RateLimitingOptions { Enabled = false };
        var middleware = new RateLimitMiddleware(
            next: (innerHttpContext) => Task.CompletedTask,
            logger: _logger.Object,
            options: Options.Create(disabledOptions));

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, _rateLimitService.Object);

        // Assert
        _rateLimitService.Verify(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_HealthCheckEndpoint_BypassesRateLimit()
    {
        // Arrange
        var middleware = new RateLimitMiddleware(
            next: (innerHttpContext) => Task.CompletedTask,
            logger: _logger.Object,
            options: Options.Create(_options));

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/health";

        // Act
        await middleware.InvokeAsync(context, _rateLimitService.Object);

        // Assert
        _rateLimitService.Verify(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _rateLimitService.Verify(x => x.RecordRequestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithinLimit_AddsHeadersAndCallsNext()
    {
        // Arrange
        _rateLimitService.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _rateLimitService.Setup(x => x.GetRemainingRequestsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(50);

        var nextCalled = false;
        var middleware = new RateLimitMiddleware(
            next: (innerHttpContext) => { nextCalled = true; return Task.CompletedTask; },
            logger: _logger.Object,
            options: Options.Create(_options));

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, _rateLimitService.Object);

        // Assert
        Assert.True(nextCalled);
        Assert.True(context.Response.Headers.ContainsKey("X-RateLimit-Limit-Minute"));
        Assert.True(context.Response.Headers.ContainsKey("X-RateLimit-Limit-Hour"));
        Assert.True(context.Response.Headers.ContainsKey("X-RateLimit-Remaining"));
        Assert.Equal("60", context.Response.Headers["X-RateLimit-Limit-Minute"].ToString());
        Assert.Equal("1000", context.Response.Headers["X-RateLimit-Limit-Hour"].ToString());
        Assert.Equal("50", context.Response.Headers["X-RateLimit-Remaining"].ToString());

        _rateLimitService.Verify(x => x.RecordRequestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ExceedsLimit_Returns429()
    {
        // Arrange
        _rateLimitService.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _rateLimitService.Setup(x => x.GetRemainingRequestsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var nextCalled = false;
        var middleware = new RateLimitMiddleware(
            next: (innerHttpContext) => { nextCalled = true; return Task.CompletedTask; },
            logger: _logger.Object,
            options: Options.Create(_options));

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, _rateLimitService.Object);

        // Assert
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.True(context.Response.Headers.ContainsKey("Retry-After"));
        Assert.Equal("60", context.Response.Headers["Retry-After"].ToString());

        _rateLimitService.Verify(x => x.RecordRequestAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_UsesXForwardedForHeader_WhenPresent()
    {
        // Arrange
        _rateLimitService.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _rateLimitService.Setup(x => x.GetRemainingRequestsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(50);

        var middleware = new RateLimitMiddleware(
            next: (innerHttpContext) => Task.CompletedTask,
            logger: _logger.Object,
            options: Options.Create(_options));

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.1";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, _rateLimitService.Object);

        // Assert
        _rateLimitService.Verify(x => x.CheckRateLimitAsync("client:192.168.1.1", It.IsAny<CancellationToken>()), Times.Once);
        _rateLimitService.Verify(x => x.RecordRequestAsync("client:192.168.1.1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
