# Task 12: Implement Prompt Persistence Service (Pipeline Stage 3/3)

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 3 hours  
**GitHub Issue**: [#39](https://github.com/akratovich-pl/ghc-promt-lab/issues/39)

## Context
Refactoring `PromptExecutionService` using Pipeline pattern. This is the third and final pipeline stage, handling conversation management and database operations.

## Objective
Extract persistence logic from `PromptExecutionService` into a dedicated `IPromptPersistenceService` that manages conversation history and stores prompt results.

## Current State
- After Task 11: `PromptExecutionService` has 3 dependencies and orchestrates 3 execution steps
- Steps 6-8 handle persistence:
  - Ensure conversation exists (create if needed)
  - Save prompt execution to database
  - Update conversation timestamp

## Requirements

### Service Interface
Create `IPromptPersistenceService` in `Core/Services/` that:
- Accepts: original request parameters, `LlmExecutionResult` (from stage 2), and correlation ID
- Returns: `PromptPersistenceResult` DTO with saved prompt details
- Single async method: `PersistAsync(ExecutePromptRequest request, LlmExecutionResult executionResult, string correlationId)`

### Data Transfer Object
Create `PromptPersistenceResult` in `Core/DTOs/` with:
- Saved prompt entity (including ID, timestamps)
- Conversation ID (existing or newly created)
- Persistence metadata (database operation details)

### Implementation Location
Create `PromptPersistenceService` in `Infrastructure/Services/`:
- Inject `IPromptRepository` for database operations
- Orchestrate conversation creation and prompt saving
- Maintain existing logging patterns with correlation ID
- Preserve error handling behavior
- Ensure transactional consistency (conversation + prompt creation)

### Conversation Management
Handle conversation lifecycle:
- If `conversationId` is null: create new conversation with generated ID
- If `conversationId` provided: verify existence, use existing
- Set conversation `UpdatedAt` timestamp after prompt save
- Associate prompt with conversation via foreign key

### Integration Points
Update `PromptExecutionService` (after Task 11 completion):
- Replace steps 6-8 with single call to `IPromptPersistenceService`
- Reduce dependencies from 3 to 2 (remove: `IPromptRepository`)
- Add dependency on `IPromptPersistenceService`
- Simplify to pure orchestration (~30-40 lines)

### Registration
Update `Registration.cs`:
- Add `IPromptPersistenceService` → `PromptPersistenceService` to DI container
- Scope: `AddScoped`

## Acceptance Criteria
- [x] Build succeeds without warnings
- [x] All existing tests pass without modification
- [x] `PromptExecutionService.ExecutePromptAsync` reduced to ~30-40 lines (pure orchestration)
- [x] New service handles null conversationId correctly (creates new conversation)
- [x] Conversation timestamp updated after each prompt save
- [x] Logging maintains correlation ID across database operations
- [x] No behavior changes observable from API consumers
- [x] Transaction rollback works correctly on save failures

## Technical Constraints
- No new features - pure refactoring
- No breaking changes
- Preserve logging
- Keep history behavior
- Transaction safety
- Maintain testability

## Testing Strategy
- Unit test `PromptPersistenceService` in isolation with mocked `IPromptRepository`
- Test conversation creation scenarios (new vs existing)
- Test transaction rollback on database errors
- Verify existing integration tests for `PromptExecutionService` still pass

## Database Operations
Service must handle:
- `EnsureConversationExistsAsync`: Create or retrieve conversation
- `SaveAsync`: Persist prompt with all metadata
- `UpdateConversationTimestampAsync`: Update conversation `UpdatedAt` field
- Error handling for constraint violations

## Follow-up Tasks
After completion, the pipeline refactoring is complete. Future enhancements can be planned:
- Provider registry for dynamic provider configuration
- Caching layer for response optimization
- Retry policies for resilience
- Circuit breaker patterns

## Estimated Impact
- Lines removed from `PromptExecutionService`: ~40-50
- New service complexity: ~80-100 lines
- Dependencies reduced: 3 → 2
- Final `PromptExecutionService` becomes pure orchestrator (~30-40 lines total)
- Complete separation of concerns: Preparation → Execution → Persistence

## Related Tasks
- Depends on: Task 11 (LlmExecutionResult DTO)
- Completes: Pipeline Pattern Refactoring
