# Layered Architecture Refactoring

This document describes the layered architecture implemented for the PromptLab application following SOLID principles.

## Architecture Overview

The application is now organized into clear layers with well-defined responsibilities:

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  - Orchestrates use cases and workflows                     │
│  - PromptExecutionService (Coordinates prompt execution)   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    Domain Layer                              │
│  - Business logic and domain services                        │
│  - ConversationHistoryService (Manages conversations)       │
│  - IRateLimitService (Rate limiting logic)                  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                  Repository Layer                            │
│  - Data access abstraction                                   │
│  - IPromptRepository (Prompt/Response persistence)          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                Infrastructure Layer                          │
│  - External system integrations                              │
│  - ILlmProvider (Google Gemini, Groq)                       │
└─────────────────────────────────────────────────────────────┘

Cross-Cutting Concerns:
├── Validators (Input validation)
│   └── IPromptValidator
├── Builders (Object construction)
│   ├── ILlmRequestBuilder
│   └── IPromptEnricher
└── Utilities (Helpers, formatters, etc.)
```

## Project Structure

### PromptLab.Core (Abstractions)

```
Core/
├── Builders/
│   ├── ILlmRequestBuilder.cs        # Builds LLM requests
│   └── IPromptEnricher.cs           # Enriches prompts with context
├── Repositories/
│   └── IPromptRepository.cs         # Data access abstraction
├── Services/
│   └── Interfaces/
│       ├── IPromptExecutionService.cs      # Application service
│       ├── IConversationHistoryService.cs  # Domain service
│       ├── ILlmProvider.cs                 # Infrastructure service
│       └── IRateLimitService.cs            # Domain service
├── Validators/
│   └── IPromptValidator.cs          # Input validation
├── Domain/
│   ├── Entities/                    # Domain entities
│   └── Enums/                       # Domain enumerations
└── DTOs/                             # Data transfer objects
```

### PromptLab.Infrastructure (Implementations)

```
Infrastructure/
├── Builders/
│   ├── LlmRequestBuilder.cs         # LLM request construction
│   └── PromptEnricher.cs            # Context file processing
├── Repositories/
│   └── PromptRepository.cs          # EF Core data access
├── Services/
│   ├── PromptExecutionService.cs    # Main orchestrator
│   ├── ConversationHistoryService.cs # Conversation management
│   ├── InMemoryRateLimitService.cs  # Rate limiting
│   └── LlmProviders/
│       ├── GoogleGeminiProvider.cs  # Google Gemini integration
│       └── GroqProvider.cs          # Groq integration
├── Validators/
│   └── PromptValidator.cs           # Input validation logic
├── Data/
│   └── ApplicationDbContext.cs      # EF Core context
└── Configuration/
    └── *.cs                          # Configuration classes
```

## Component Responsibilities

### Application Services (Orchestrators)
- **PromptExecutionService**: Orchestrates the entire prompt execution workflow
- Coordinates validation, conversation loading, prompt enrichment, LLM calls, and persistence
- Depends on: Validators, Domain Services, Repositories, Builders, Infrastructure Services

### Domain Services (Business Logic)
- **ConversationHistoryService**: Manages conversation lifecycle and history
  - Creates new conversations
  - Loads conversation history
  - Updates conversation timestamps
- **IRateLimitService**: Enforces rate limiting rules

### Repositories (Data Access)
- **PromptRepository**: Handles all database operations for prompts and responses
  - Saves prompt execution results
  - Retrieves prompts by ID or conversation
  - Manages database transactions

### Infrastructure Services (External Integrations)
- **ILlmProvider**: Abstracts LLM provider implementations
  - GoogleGeminiProvider
  - GroqProvider

### Validators (Cross-Cutting)
- **PromptValidator**: Validates input parameters
  - Maximum prompt length
  - Maximum context file count

### Builders (Helpers/Factories)
- **LlmRequestBuilder**: Constructs LLM request objects
- **PromptEnricher**: Enriches prompts with context file content

## Benefits of This Architecture

### 1. Single Responsibility Principle (SRP)
- Each class has one clear, well-defined responsibility
- Easy to understand what each component does

### 2. Separation of Concerns
- Clear boundaries between layers
- Business logic separated from infrastructure
- Data access isolated in repositories

### 3. Dependency Inversion Principle (DIP)
- All dependencies are on abstractions (interfaces)
- High-level modules don't depend on low-level modules

### 4. Testability
- Each component can be unit tested independently
- Easy to mock dependencies
- Clear input/output contracts

### 5. Maintainability
- Changes to one component don't ripple through the system
- New features can be added without modifying existing code
- Easy to locate where specific functionality lives

### 6. Flexibility
- Easy to swap implementations (e.g., different LLM providers)
- Can add new validators, builders, or services without modification
- Supports extension without modification (Open/Closed Principle)

## Dependency Injection Configuration

Services are registered in layers in `Registration.cs`:

```csharp
// Infrastructure Services (External integrations)
builder.Services.AddSingleton<ILlmProvider, GoogleGeminiProvider>();
builder.Services.AddSingleton<ILlmProvider, GroqProvider>();

// Application Services (Orchestrators)
builder.Services.AddScoped<IPromptExecutionService, PromptExecutionService>();

// Domain Services (Business logic)
builder.Services.AddScoped<IConversationHistoryService, ConversationHistoryService>();
builder.Services.AddScoped<IRateLimitService, InMemoryRateLimitService>();

// Repositories (Data access)
builder.Services.AddScoped<IPromptRepository, PromptRepository>();

// Validators (Cross-cutting)
builder.Services.AddScoped<IPromptValidator, PromptValidator>();

// Builders (Helpers)
builder.Services.AddScoped<ILlmRequestBuilder, LlmRequestBuilder>();
builder.Services.AddScoped<IPromptEnricher, PromptEnricher>();
```

## Design Patterns Used

1. **Repository Pattern**: Abstracts data access (IPromptRepository)
2. **Builder Pattern**: Constructs complex objects (ILlmRequestBuilder)
3. **Strategy Pattern**: Pluggable LLM providers (ILlmProvider)
4. **Validator Pattern**: Input validation (IPromptValidator)
5. **Dependency Injection**: Loose coupling throughout

## When to Add New Components

### Add a new Validator when:
- You need to validate different types of input
- Cross-cutting validation rules are needed

### Add a new Builder when:
- You need to construct complex objects
- Object creation logic becomes non-trivial

### Add a new Repository when:
- You need data access for a new aggregate root
- Different data sources need abstraction

### Add a new Domain Service when:
- You have business logic that doesn't fit in entities
- You need to coordinate multiple entities

### Add a new Infrastructure Service when:
- You need to integrate with external systems
- You need to abstract infrastructure concerns

## Migration Notes

### What Changed:
1. **IContextFileProcessor** → **IPromptEnricher**
   - Better name reflecting its purpose
   - Moved from `Services/Interfaces` to `Builders`

2. **Folder restructuring**:
   - Validators: `Services/Validation` → `Validators`
   - Repositories: `Services/Persistence` → `Repositories`
   - Builders: `Services/LlmRequestBuilder` → `Builders`
   - Context processing: `Services/Context` → `Builders`

3. **Namespace updates**:
   - `PromptLab.Core.Services.Interfaces.*` → `PromptLab.Core.{Validators|Repositories|Builders}`
   - `PromptLab.Infrastructure.Services.*` → `PromptLab.Infrastructure.{Validators|Repositories|Builders}`

### Breaking Changes:
- None for external consumers (API contracts unchanged)
- Internal restructuring only affects dependency injection

## Future Improvements

Consider these patterns as the application grows:

1. **CQRS (Command Query Responsibility Segregation)**
   - Separate read and write operations
   - Use MediatR for command/query handlers

2. **Domain Events**
   - Decouple domain logic with events
   - Enable reactive programming

3. **Specification Pattern**
   - Encapsulate query logic
   - Reusable filtering criteria

4. **Unit of Work Pattern**
   - Coordinate multiple repository operations
   - Better transaction management

---

**Date**: February 4, 2026  
**Refactoring**: Layered Architecture with SOLID Principles
