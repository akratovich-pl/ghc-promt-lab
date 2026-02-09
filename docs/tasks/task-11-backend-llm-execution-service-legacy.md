# Task: Implement LLM Execution Service

## Context
Refactoring `PromptExecutionService` using Pipeline pattern. This is the second of three pipeline stages, handling provider interaction and LLM invocation.

## Objective
Extract LLM execution logic from `PromptExecutionService` into a dedicated `ILlmExecutionService` that routes requests to appropriate providers and handles responses.

## Current State
- After Task 1: `PromptExecutionService` will have 4 dependencies and orchestrate 5 execution steps
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
Current implementation (2 providers):
- Extract model prefix (e.g., "gemini", "gpt")
- Route to appropriate provider
- For now, single `ILlmProvider` implementation handles routing internally
- **Future**: Replace with provider registry/factory pattern when adding dynamic configuration

### Integration Points
Update `PromptExecutionService` (after Task 1 completion):
- Replace step 5 with single call to `ILlmExecutionService`
- Reduce dependencies from 4 to 3 (remove: `ILlmProvider`)
- Add dependency on `ILlmExecutionService`
- Pass correlation ID from existing context

### Registration
Update `Registration.cs`:
- Add `ILlmExecutionService` → `LlmExecutionService` to DI container
- Scope: `AddScoped` (matches existing service lifetimes)

## Constraints
- **No new features** - pure refactoring, preserve existing behavior
- **No breaking changes** - maintain same public API on `IPromptExecutionService`
- **Preserve logging** - maintain same log messages and correlation IDs
- **Provider routing** - keep existing logic, don't introduce new routing mechanism yet
- **Defer enhancements** - add comments/placeholders for retry and caching, but don't implement

## Success Criteria
- [ ] Build succeeds without warnings
- [ ] All existing tests pass without modification
- [ ] `PromptExecutionService.ExecutePromptAsync` reduced to ~50-60 lines
- [ ] New service handles provider errors correctly
- [ ] Logging maintains correlation ID across provider calls
- [ ] No behavior changes observable from API consumers
- [ ] Code includes TODO comments for future retry policy and caching hooks

## Testing Strategy
- Unit test `LlmExecutionService` in isolation with mocked `ILlmProvider`
- Test provider error scenarios (timeouts, API errors, rate limits)
- Verify existing integration tests for `PromptExecutionService` still pass
- No need to modify existing controller tests

## Dependencies
- Requires Task 1 completion (`PreparedPromptRequest` DTO must exist)
- All other required services already exist

## Follow-up Tasks
After completion, this enables:
- Task 3: Implement `IPromptPersistenceService` (persistence pipeline stage)

## Future Enhancements (Not in Scope)
Document extension points for:
- **Retry Policy**: Automatic retry on transient provider failures
- **Response Caching**: Cache responses for identical requests
- **Provider Registry**: Dynamic provider selection based on configuration
- **Circuit Breaker**: Fail-fast when provider is consistently unavailable

## Notes
- Current providers: Gemini and GPT families
- Provider selection logic exists in `ILlmProvider` implementation
- Correlation ID propagation is critical for distributed tracing
- Execution timestamp can help with performance monitoring

## Estimated Impact
- Lines removed from `PromptExecutionService`: ~15-20
- New service complexity: ~60-80 lines
- Dependencies reduced: 4 → 3
- Clear separation between orchestration and execution concerns
