# Task 01: Create LLM Provider Abstraction Layer

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 2-3 hours  
**GitHub Issue**: [#3](https://github.com/akratovich-pl/ghc-promt-lab/issues/3)

## Objective
Define common interfaces and data models for LLM provider integration that allows adding multiple providers in the future without changing existing code.

## Requirements

### 1. Create `ILlmProvider` Interface
**Location**: `Core/Services/Interfaces/`
- Define `GenerateAsync()` method signature
- Define `EstimateTokensAsync()` method signature
- Define `IsAvailable()` method for health checks
- Add `ProviderName` property

### 2. Create Request/Response DTOs
**Location**: `Core/DTOs/`
- `LlmRequest` record with Prompt, SystemPrompt, Model, MaxTokens, Temperature
- `LlmResponse` record with Content, InputTokens, OutputTokens, Cost, LatencyMs, Model
- `TokenEstimateRequest` and `TokenEstimateResponse` records

### 3. Create Provider Configuration Interface
**Interface**: `ILlmProviderConfig`
- Method to retrieve API key for specific provider
- Method to get base URL for provider
- Method to check if provider is enabled

## Acceptance Criteria
- [x] All interfaces defined in Core layer (no dependencies on Infrastructure)
- [x] DTOs are immutable records with proper validation attributes
- [x] XML documentation comments on all public interfaces
- [x] No implementation details in abstractions
- [x] Code follows C# naming conventions

## Technical Constraints
- Use records for DTOs (immutability)
- Use async/await for all I/O operations
- No external dependencies in Core layer
- Follow repository pattern principles

## Related Tasks
- Foundation for Task 02 (Google Gemini Configuration)
- Foundation for Task 03 (Google Gemini API Client)
