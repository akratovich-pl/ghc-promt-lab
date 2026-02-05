# Task: Implement Request Preparation Service

## Context
Refactoring `PromptExecutionService` using Pipeline pattern to achieve better separation of concerns. This is the first of three pipeline stages.

## Objective
Extract request preparation logic (validation, enrichment, history loading, request building) from `PromptExecutionService` into a dedicated `IRequestPreparationService`.

## Current State
- `PromptExecutionService` has 6 dependencies and orchestrates 8 execution steps
- Steps 1-4 handle request preparation:
  1. Input validation
  2. Conversation history loading
  3. Context enrichment
  4. LLM request building

## Requirements

### Service Interface
Create `IRequestPreparationService` in `Core/Services/` that:
- Accepts: raw user input parameters (prompt, systemPrompt, conversationId, contextFileIds, model, maxTokens, temperature, userId)
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
- Inject and orchestrate existing services: `IPromptValidator`, `IConversationHistoryService`, `IPromptEnricher`, `ILlmRequestBuilder`
- Maintain existing logging patterns
- Preserve error handling behavior

### Integration Points
Update `PromptExecutionService`:
- Replace steps 1-4 with single call to `IRequestPreparationService`
- Reduce dependencies from 6 to 4 (remove: validator, enricher, history service, request builder)
- Add dependency on `IRequestPreparationService`
- Simplify method body significantly

### Registration
Update `Registration.cs`:
- Add `IRequestPreparationService` → `RequestPreparationService` to DI container
- Scope: `AddScoped` (matches existing service lifetimes)

## Constraints
- **No new features** - pure refactoring, preserve existing behavior
- **No breaking changes** - maintain same public API on `IPromptExecutionService`
- **Preserve logging** - maintain same log messages and correlation IDs
- **Keep validation** - existing validation rules must remain unchanged
- **Maintain testability** - new service should be unit testable in isolation

## Success Criteria
- [ ] Build succeeds without warnings
- [ ] All existing tests pass without modification
- [ ] `PromptExecutionService.ExecutePromptAsync` reduced to ~70-80 lines
- [ ] New service properly handles all edge cases (null conversationId, empty contextFileIds, etc.)
- [ ] Logging maintains same information and format
- [ ] No behavior changes observable from API consumers

## Testing Strategy
- Unit test `RequestPreparationService` in isolation with mocked dependencies
- Verify existing integration tests for `PromptExecutionService` still pass
- No need to modify existing controller tests

## Dependencies
None - all required services already exist

## Follow-up Tasks
After completion, this enables:
- Task 2: Implement `ILlmExecutionService` (execution pipeline stage)
- Task 3: Implement `IPromptPersistenceService` (persistence pipeline stage)

## Notes
- User ID is currently hardcoded as "system" - maintain this for now
- Default model is "gemini-1.5-flash" - preserve this default
- Consider future extensibility for provider selection strategy (not implemented now)

## Estimated Impact
- Lines removed from `PromptExecutionService`: ~40
- New service complexity: ~80-100 lines
- Dependencies reduced: 6 → 4
- Improved separation of concerns and testability
