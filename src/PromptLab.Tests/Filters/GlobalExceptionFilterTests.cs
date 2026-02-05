using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using PromptLab.Api.Filters;

namespace PromptLab.Tests.Filters;

public class GlobalExceptionFilterTests
{
    private readonly Mock<ILogger<GlobalExceptionFilter>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly GlobalExceptionFilter _filter;

    public GlobalExceptionFilterTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionFilter>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _filter = new GlobalExceptionFilter(_mockLogger.Object, _mockEnvironment.Object);
    }

    private ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        return new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }

    [Fact]
    public void OnException_ArgumentException_Returns400BadRequest()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        var context = CreateExceptionContext(exception);

        // Act
        _filter.OnException(context);

        // Assert
        Assert.True(context.ExceptionHandled);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(400, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Invalid Request", problemDetails.Title);
        Assert.Equal("Invalid argument", problemDetails.Detail);
    }

    [Fact]
    public void OnException_ArgumentNullException_Returns400BadRequest()
    {
        // Arrange
        var exception = new ArgumentNullException("paramName", "Parameter cannot be null");
        var context = CreateExceptionContext(exception);

        // Act
        _filter.OnException(context);

        // Assert
        Assert.True(context.ExceptionHandled);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(400, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Invalid Request", problemDetails.Title);
    }

    [Fact]
    public void OnException_InvalidOperationException_Returns400BadRequest()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation");
        var context = CreateExceptionContext(exception);

        // Act
        _filter.OnException(context);

        // Assert
        Assert.True(context.ExceptionHandled);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(400, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Invalid Operation", problemDetails.Title);
        Assert.Equal("Invalid operation", problemDetails.Detail);
    }

    [Fact]
    public void OnException_KeyNotFoundException_Returns404NotFound()
    {
        // Arrange
        var exception = new KeyNotFoundException("Resource not found");
        var context = CreateExceptionContext(exception);

        // Act
        _filter.OnException(context);

        // Assert
        Assert.True(context.ExceptionHandled);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(404, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Not Found", problemDetails.Title);
        Assert.Equal("Resource not found", problemDetails.Detail);
    }

    [Fact]
    public void OnException_UnauthorizedAccessException_Returns403Forbidden()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");
        var context = CreateExceptionContext(exception);

        // Act
        _filter.OnException(context);

        // Assert
        Assert.True(context.ExceptionHandled);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(403, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(403, problemDetails.Status);
        Assert.Equal("Forbidden", problemDetails.Title);
        Assert.Equal("Access denied", problemDetails.Detail);
    }

    [Fact]
    public void OnException_GenericException_Returns500InternalServerError()
    {
        // Arrange
        var exception = new Exception("Unexpected error");
        var context = CreateExceptionContext(exception);
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        _filter.OnException(context);

        // Assert
        Assert.True(context.ExceptionHandled);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(500, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(500, problemDetails.Status);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.Equal("An error occurred while processing your request", problemDetails.Detail);
    }

    [Fact]
    public void OnException_GenericException_InDevelopment_IncludesDetailedError()
    {
        // Arrange
        var exception = new Exception("Unexpected error");
        var context = CreateExceptionContext(exception);
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        _filter.OnException(context);

        // Assert
        Assert.True(context.ExceptionHandled);
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(500, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(500, problemDetails.Status);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.Equal("Unexpected error", problemDetails.Detail);
        Assert.True(problemDetails.Extensions.ContainsKey("stackTrace"));
    }

    [Fact]
    public void OnException_IncludesTraceId()
    {
        // Arrange
        var exception = new ArgumentException("Test");
        var context = CreateExceptionContext(exception);
        var traceId = "test-trace-id";
        context.HttpContext.TraceIdentifier = traceId;

        // Act
        _filter.OnException(context);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
        Assert.Equal(traceId, problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public void OnException_ClientError_LogsWarning()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        var context = CreateExceptionContext(exception);

        // Act
        _filter.OnException(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnException_ServerError_LogsError()
    {
        // Arrange
        var exception = new Exception("Server error");
        var context = CreateExceptionContext(exception);
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        _filter.OnException(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void OnException_WithInnerException_InDevelopment_IncludesInnerExceptionDetails()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var exception = new Exception("Outer error", innerException);
        var context = CreateExceptionContext(exception);
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        _filter.OnException(context);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.True(problemDetails.Extensions.ContainsKey("innerException"));
    }
}
