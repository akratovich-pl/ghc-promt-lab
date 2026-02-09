# Task 11: Implement LLM Execution Service (Pipeline Stage 2/3)

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 2 hours  
**GitHub Issue**: [#38](https://github.com/akratovich-pl/ghc-promt-lab/issues/38)

## Context
Refactoring `PromptExecutionService` using Pipeline pattern. This is the second of three pipeline stages, handling provider interaction and LLM invocation.

## Objective
Extract LLM execution logic from `PromptExecutionService` into a dedicated `ILlmExecutionService` that routes requests to appropriate providers and handles responses.

## Current State
- After Task 10: `PromptExecutionService` has 4 dependencies and orchestrates 5 execution steps
- Step 5 handles LLM execution:
  - Provider selection based on model name
  - Request invocation via `ILlmProvider`
  - Response handling and error propagation

## Requirements

### Service Interface
Create `ILlmExecutionService` in `Core/Services/` that:
- Accepts: `PreparedPromptRequest` (from stage 1) and correlation ID
- Returns: `LlmExecutionResult` DTO containing provider response
- Single async method: `ExecuteAsync(PreparedPromptRequest request, string correlationId)`

### Data Transfer Object
Create `LlmExecutionResult` in `Core/DTOs/` with:
- Provider response (`LlmResponse`)
- Execution metadata (provider name, actual model used, execution timestamp)
- Optional context file ID (pass-through from preparation stage)

### Implementation Location
Create `LlmExecutionService` in `Infrastructure/Services/`:
- Inject `ILlmProvider` (currently single provider, plan for multiple)
- Route to provider based on model name in `PreparedPromptRequest`
- Maintain existing logging patterns with correlation ID
- Preserve error handling behavior
- Add extensibility points for future enhancements (retry, caching) but **do not implement them now**

### Provider Routing Strategy
Current implementation:
- Extract model prefix (e.g., "gemini", "gpt")
- Route to appropriate provider
- Single `ILlmProvider` implementation handles routing internally
- **Future**: Replace with provider registry/factory pattern

### Integration Points
Update `PromptExecutionService` (after Task 10 completion):
- Replace step 5 with single call to `ILlmExecutionService`
- Reduce dependencies from 4 to 3 (remove: `ILlmProvider`)
- Add dependency on `ILlmExecutionService`
- Pass correlation ID from existing context

### Registration
Update `Registration.cs`:
- Add `ILlmExecutionService` → `LlmExecutionService` to DI container
- Scope: `AddScoped`

## Acceptance Criteria
- [x] Build succeeds without warnings
- [x] All existing tests pass without modification
- [x] `PromptExecutionService.ExecutePromptAsync` reduced to ~50-60 lines
- [x] New service handles provider errors correctly
- [x] Logging maintains correlation ID across provider calls
- [x] No behavior changes observable from API consumers
- [x] Code includes TODO comments for future retry policy and caching hooks

## Technical Constraints
- No new features - pure refactoring
- No breaking changes
- Preserve logging
- Provider routing - keep existing logic
- Defer enhancements - add comments/placeholders only

## Testing Strategy
- Unit test `LlmExecutionService` in isolation with mocked `ILlmProvider`
- Test provider error scenarios (timeouts, API errors, rate limits)
- Verify existing integration tests for `PromptExecutionService` still pass

## Future Enhancements (Not in Scope)
Document extension points for:
- **Retry Policy**: Automatic retry on transient provider failures
- **Response Caching**: Cache responses for identical requests
- **Provider Registry**: Dynamic provider selection based on configuration
- **Circuit Breaker**: Fail-fast when provider is consistently unavailable

## Estimated Impact
- Lines removed from `PromptExecutionService`: ~15-20
- New service complexity: ~60-80 lines
- Dependencies reduced: 4 → 3
- Clear separation between orchestration and execution concerns

## Related Tasks
- Depends on: Task 10 (PreparedPromptRequest DTO)
- Enables: Task 12 (Prompt Persistence Service)
