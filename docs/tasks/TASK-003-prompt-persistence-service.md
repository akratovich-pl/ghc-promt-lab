# Task: Implement Prompt Persistence Service

## Context
Refactoring `PromptExecutionService` using Pipeline pattern. This is the third and final pipeline stage, handling conversation management and database operations.

## Objective
Extract persistence logic from `PromptExecutionService` into a dedicated `IPromptPersistenceService` that manages conversation history and stores prompt results.

## Current State
- After Task 2: `PromptExecutionService` will have 3 dependencies and orchestrate 3 execution steps
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
Update `PromptExecutionService` (after Task 2 completion):
- Replace steps 6-8 with single call to `IPromptPersistenceService`
- Reduce dependencies from 3 to 2 (remove: `IPromptRepository`)
- Add dependency on `IPromptPersistenceService`
- Simplify to pure orchestration (~30-40 lines)

### Registration
Update `Registration.cs`:
- Add `IPromptPersistenceService` → `PromptPersistenceService` to DI container
- Scope: `AddScoped` (matches existing service lifetimes)

## Constraints
- **No new features** - pure refactoring, preserve existing behavior
- **No breaking changes** - maintain same public API on `IPromptExecutionService`
- **Preserve logging** - maintain same log messages and correlation IDs
- **Keep history behavior** - conversation history remains integral feature
- **Transaction safety** - ensure atomic operations for conversation + prompt creation
- **Maintain testability** - new service should be unit testable in isolation

## Success Criteria
- [ ] Build succeeds without warnings
- [ ] All existing tests pass without modification
- [ ] `PromptExecutionService.ExecutePromptAsync` reduced to ~30-40 lines (pure orchestration)
- [ ] New service handles null conversationId correctly (creates new conversation)
- [ ] Conversation timestamp updated after each prompt save
- [ ] Logging maintains correlation ID across database operations
- [ ] No behavior changes observable from API consumers
- [ ] Transaction rollback works correctly on save failures

## Testing Strategy
- Unit test `PromptPersistenceService` in isolation with mocked `IPromptRepository`
- Test conversation creation scenarios (new vs existing)
- Test transaction rollback on database errors
- Verify existing integration tests for `PromptExecutionService` still pass
- No need to modify existing controller tests

## Dependencies
- Requires Task 2 completion (`LlmExecutionResult` DTO must exist)
- All other required services already exist

## Follow-up Tasks
After completion, the pipeline refactoring is complete. Future enhancements can be planned:
- Provider registry for dynamic provider configuration
- Caching layer for response optimization
- Retry policies for resilience
- Circuit breaker patterns

## Database Operations
Service must handle:
- `EnsureConversationExistsAsync`: Create or retrieve conversation
- `SaveAsync`: Persist prompt with all metadata
- `UpdateConversationTimestampAsync`: Update conversation `UpdatedAt` field
- Error handling for constraint violations (duplicate IDs, foreign key failures)

## Notes
- User ID is currently hardcoded as "system" - maintain this for now
- Conversation history is loaded in stage 1 but created/updated in stage 3
- Context file ID flows through all stages for traceability
- Correlation ID critical for debugging across pipeline stages

## Estimated Impact
- Lines removed from `PromptExecutionService`: ~40-50
- New service complexity: ~80-100 lines
- Dependencies reduced: 3 → 2
- Final `PromptExecutionService` becomes pure orchestrator (~30-40 lines total)
- Complete separation of concerns: Preparation → Execution → Persistence
