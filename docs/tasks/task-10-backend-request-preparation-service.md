# Task 10: Implement Request Preparation Service (Pipeline Stage 1/3)

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 3 hours  
**GitHub Issue**: [#36](https://github.com/akratovich-pl/ghc-promt-lab/issues/36)

## Context
Refactoring `PromptExecutionService` using Pipeline pattern to achieve better separation of concerns. This is the first of three pipeline stages.

## Objective
Extract request preparation logic (validation, enrichment, history loading, request building) from `PromptExecutionService` into a dedicated `IRequestPreparationService`.

## Current State
- `PromptExecutionService` had 6 dependencies and orchestrated 8 execution steps
- Steps 1-4 handled request preparation:
  1. Input validation
  2. Conversation history loading
  3. Context enrichment
  4. LLM request building

## Requirements

### Service Interface
Create `IRequestPreparationService` in `Core/Services/` that:
- Accepts: raw user input parameters
- Returns: `PreparedPromptRequest` DTO containing all data needed for LLM execution
- Single async method: `PrepareAsync(...)`

### Data Transfer Object
Create `PreparedPromptRequest` in `Core/DTOs/` with:
- Built `LlmRequest` ready for provider consumption
- Context file ID (if enrichment occurred)
- Loaded conversation history
- User ID and model name for traceability

### Implementation Location
Create `RequestPreparationService` in `Infrastructure/Services/`:
- Inject and orchestrate: `IPromptValidator`, `IConversationHistoryService`, `IPromptEnricher`, `ILlmRequestBuilder`
- Maintain existing logging patterns
- Preserve error handling behavior

### Integration Points
Update `PromptExecutionService`:
- Replace steps 1-4 with single call to `IRequestPreparationService`
- Reduce dependencies from 6 to 4
- Add dependency on `IRequestPreparationService`

### Registration
Update `Registration.cs`:
- Add `IRequestPreparationService` → `RequestPreparationService` to DI container
- Scope: `AddScoped`

## Acceptance Criteria
- [x] Build succeeds without warnings
- [x] All existing tests pass without modification
- [x] `PromptExecutionService.ExecutePromptAsync` reduced to ~70-80 lines
- [x] New service properly handles all edge cases
- [x] Logging maintains same information and format
- [x] No behavior changes observable from API consumers

## Technical Constraints
- No new features - pure refactoring
- No breaking changes
- Preserve logging
- Keep validation rules unchanged
- Maintain testability

## Testing Strategy
- Unit test `RequestPreparationService` in isolation with mocked dependencies
- Verify existing integration tests for `PromptExecutionService` still pass

## Related Tasks
- Part of: Pipeline Pattern Refactoring
- Enables: Task 11 (LLM Execution Service)
- Enables: Task 12 (Prompt Persistence Service)

## Estimated Impact
- Lines removed from `PromptExecutionService`: ~40
- New service complexity: ~80-100 lines
- Dependencies reduced: 6 → 4
