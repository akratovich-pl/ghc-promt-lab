# Migration Guide: Layered Architecture Refactoring

## Overview

This guide helps you understand the changes made during the layered architecture refactoring and how to work with the new structure.

## What Changed

### 1. Renamed Components

| Old Name | New Name | Reason |
|----------|----------|--------|
| `IContextFileProcessor` | `IPromptEnricher` | Better reflects its purpose of enriching prompts |
| `ProcessContextFilesAsync()` | `EnrichPromptWithContextAsync()` | More descriptive method name |

### 2. Moved Interfaces

| Old Location | New Location | Layer |
|--------------|--------------|-------|
| `Core/Services/Interfaces/IPromptValidator.cs` | `Core/Validators/IPromptValidator.cs` | Validator |
| `Core/Services/Interfaces/IPromptRepository.cs` | `Core/Repositories/IPromptRepository.cs` | Repository |
| `Core/Services/Interfaces/ILlmRequestBuilder.cs` | `Core/Builders/ILlmRequestBuilder.cs` | Builder |
| `Core/Services/Interfaces/IContextFileProcessor.cs` | `Core/Builders/IPromptEnricher.cs` | Builder |

### 3. Moved Implementations

| Old Location | New Location |
|--------------|--------------|
| `Infrastructure/Services/Validation/PromptValidator.cs` | `Infrastructure/Validators/PromptValidator.cs` |
| `Infrastructure/Services/Persistence/PromptRepository.cs` | `Infrastructure/Repositories/PromptRepository.cs` |
| `Infrastructure/Services/LlmRequestBuilder/LlmRequestBuilder.cs` | `Infrastructure/Builders/LlmRequestBuilder.cs` |
| `Infrastructure/Services/Context/ContextFileProcessor.cs` | `Infrastructure/Builders/PromptEnricher.cs` |
| `Infrastructure/Services/Conversation/ConversationHistoryService.cs` | `Infrastructure/Services/ConversationHistoryService.cs` |

### 4. Namespace Changes

**Old namespaces:**
```csharp
using PromptLab.Core.Services.Interfaces;
using PromptLab.Infrastructure.Services.Validation;
using PromptLab.Infrastructure.Services.Persistence;
using PromptLab.Infrastructure.Services.Context;
using PromptLab.Infrastructure.Services.LlmRequestBuilder;
using PromptLab.Infrastructure.Services.Conversation;
```

**New namespaces:**
```csharp
using PromptLab.Core.Validators;
using PromptLab.Core.Repositories;
using PromptLab.Core.Builders;
using PromptLab.Core.Services.Interfaces;
using PromptLab.Infrastructure.Validators;
using PromptLab.Infrastructure.Repositories;
using PromptLab.Infrastructure.Builders;
using PromptLab.Infrastructure.Services;
```

## How to Update Your Code

### If You're Using Dependency Injection

✅ **No changes needed!** The DI container still resolves the same interfaces.

```csharp
// This still works exactly the same
public MyController(IPromptExecutionService promptService)
{
    _promptService = promptService;
}
```

### If You're Creating New Services

**Old way:**
```csharp
// DON'T DO THIS ANYMORE
using PromptLab.Core.Services.Interfaces;

public class MyService
{
    private readonly IContextFileProcessor _processor;
    
    public MyService(IContextFileProcessor processor)
    {
        _processor = processor;
    }
}
```

**New way:**
```csharp
// DO THIS INSTEAD
using PromptLab.Core.Builders;

public class MyService
{
    private readonly IPromptEnricher _enricher;
    
    public MyService(IPromptEnricher enricher)
    {
        _enricher = enricher;
    }
}
```

### If You're Writing Tests

**Old way:**
```csharp
var mockProcessor = new Mock<IContextFileProcessor>();
mockProcessor
    .Setup(x => x.ProcessContextFilesAsync(It.IsAny<string>(), It.IsAny<List<Guid>?>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(("enriched prompt", Guid.NewGuid()));
```

**New way:**
```csharp
var mockEnricher = new Mock<IPromptEnricher>();
mockEnricher
    .Setup(x => x.EnrichPromptWithContextAsync(It.IsAny<string>(), It.IsAny<List<Guid>?>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(("enriched prompt", Guid.NewGuid()));
```

## Understanding the New Structure

### Where to Put New Code

#### Adding a new validator?
```
Core/Validators/IMyValidator.cs
Infrastructure/Validators/MyValidator.cs
```

#### Adding a new repository?
```
Core/Repositories/IMyRepository.cs
Infrastructure/Repositories/MyRepository.cs
```

#### Adding a new builder?
```
Core/Builders/IMyBuilder.cs
Infrastructure/Builders/MyBuilder.cs
```

#### Adding a new domain service?
```
Core/Services/Interfaces/IMyDomainService.cs
Infrastructure/Services/MyDomainService.cs
```

#### Adding a new application service?
```
Core/Services/Interfaces/IMyApplicationService.cs
Infrastructure/Services/MyApplicationService.cs
```

### Layer Decision Tree

```
Is it validating input?
  └─> Yes: Put in Validators/

Is it accessing the database?
  └─> Yes: Put in Repositories/

Is it building/constructing objects?
  └─> Yes: Put in Builders/

Is it coordinating a use case?
  └─> Yes: Put in Services/ (Application Service)

Is it business logic?
  └─> Yes: Put in Services/ (Domain Service)

Is it integrating with external systems?
  └─> Yes: Put in Services/ or Infrastructure/
```

## Registering New Components

Add to `Registration.cs` in the appropriate section:

```csharp
public static void ConfigureServices(WebApplicationBuilder builder)
{
    // ... existing code ...

    // Validators (Cross-cutting)
    builder.Services.AddScoped<IPromptValidator, PromptValidator>();
    builder.Services.AddScoped<IMyValidator, MyValidator>(); // ← Add here

    // Builders (Helpers)
    builder.Services.AddScoped<ILlmRequestBuilder, LlmRequestBuilder>();
    builder.Services.AddScoped<IPromptEnricher, PromptEnricher>();
    builder.Services.AddScoped<IMyBuilder, MyBuilder>(); // ← Add here

    // Repositories (Data access)
    builder.Services.AddScoped<IPromptRepository, PromptRepository>();
    builder.Services.AddScoped<IMyRepository, MyRepository>(); // ← Add here

    // Domain Services (Business logic)
    builder.Services.AddScoped<IConversationHistoryService, ConversationHistoryService>();
    builder.Services.AddScoped<IMyDomainService, MyDomainService>(); // ← Add here

    // Application Services (Orchestrators)
    builder.Services.AddScoped<IPromptExecutionService, PromptExecutionService>();
    builder.Services.AddScoped<IMyApplicationService, MyApplicationService>(); // ← Add here
}
```

## Common Pitfalls

### ❌ Don't mix layers

```csharp
// BAD: Repository directly in controller
public class MyController : ControllerBase
{
    private readonly IPromptRepository _repository;
    
    public MyController(IPromptRepository repository)
    {
        _repository = repository; // Skip application service!
    }
}
```

```csharp
// GOOD: Controller uses application service
public class MyController : ControllerBase
{
    private readonly IPromptExecutionService _service;
    
    public MyController(IPromptExecutionService service)
    {
        _service = service; // Proper layering!
    }
}
```

### ❌ Don't put business logic in builders

```csharp
// BAD: Business logic in builder
public class MyBuilder : IMyBuilder
{
    public MyObject Build(Data data)
    {
        // Complex business rules here
        if (IsSpecialCase(data))
        {
            // ... lots of logic ...
        }
        return new MyObject();
    }
}
```

```csharp
// GOOD: Simple construction only
public class MyBuilder : IMyBuilder
{
    public MyObject Build(Data data)
    {
        return new MyObject
        {
            Property1 = data.Value1,
            Property2 = data.Value2
        };
    }
}
```

### ❌ Don't make validators stateful

```csharp
// BAD: Validator with state
public class MyValidator : IMyValidator
{
    private int _validationCount; // State!
    
    public void Validate(string input)
    {
        _validationCount++; // Bad!
    }
}
```

```csharp
// GOOD: Stateless validator
public class MyValidator : IMyValidator
{
    public void Validate(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Invalid input");
        }
    }
}
```

## Testing the New Architecture

### Unit Test Example

```csharp
public class PromptExecutionServiceTests
{
    [Fact]
    public async Task ExecutePrompt_ValidInput_Success()
    {
        // Arrange
        var mockValidator = new Mock<IPromptValidator>();
        var mockRateLimit = new Mock<IRateLimitService>();
        var mockConversation = new Mock<IConversationHistoryService>();
        var mockEnricher = new Mock<IPromptEnricher>();
        var mockRepository = new Mock<IPromptRepository>();
        var mockBuilder = new Mock<ILlmRequestBuilder>();
        var mockProvider = new Mock<ILlmProvider>();
        var mockLogger = new Mock<ILogger<PromptExecutionService>>();

        mockRateLimit.Setup(x => x.CheckRateLimitAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        mockEnricher.Setup(x => x.EnrichPromptWithContextAsync(It.IsAny<string>(), It.IsAny<List<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("enriched", null));

        // ... setup other mocks ...

        var service = new PromptExecutionService(
            mockProvider.Object,
            mockRateLimit.Object,
            mockValidator.Object,
            mockConversation.Object,
            mockEnricher.Object,
            mockRepository.Object,
            mockBuilder.Object,
            mockLogger.Object
        );

        // Act
        var result = await service.ExecutePromptAsync(
            "test prompt",
            null,
            null,
            null,
            "gemini-1.5-flash",
            null,
            null,
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(result);
        mockValidator.Verify(x => x.ValidatePromptRequest(It.IsAny<string>(), It.IsAny<List<Guid>?>()), Times.Once);
    }
}
```

## FAQ

### Q: Why so many interfaces?
**A:** Each interface has a single responsibility. This makes testing easier and follows the Interface Segregation Principle.

### Q: Do I always need to create an interface?
**A:** For services, repositories, and anything you want to mock in tests: Yes. For simple DTOs or entities: No.

### Q: What if I need a service to call another service?
**A:** That's fine! Application services can depend on domain services. Domain services can depend on repositories. Just don't create circular dependencies.

### Q: Can a validator use a repository?
**A:** Generally no. Validators should be lightweight and fast. If you need database validation, consider doing it in a domain service instead.

### Q: Where do I put helper/utility methods?
**A:** If they're stateless and reusable: Put them in a static utility class. If they need dependencies: Create a builder or helper service.

## Need Help?

- Review the [LAYERED_ARCHITECTURE.md](./LAYERED_ARCHITECTURE.md) for detailed architecture info
- Check [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) for visual guides
- Look at existing implementations as examples
- Ask the team for guidance on complex scenarios

---

**Last Updated**: February 4, 2026  
**Refactoring Version**: 1.0
