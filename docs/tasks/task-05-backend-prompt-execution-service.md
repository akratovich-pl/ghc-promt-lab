# Task 05: Create Prompt Execution Service

**Status**: ✅ Done (Refactored in Tasks 10-12)  
**Priority**: High  
**Estimated Effort**: 3-4 hours  
**GitHub Issue**: [#6](https://github.com/akratovich-pl/ghc-promt-lab/issues/6)

## Objective
Create the business logic layer that orchestrates prompt execution, including provider selection, context file handling, conversation management, and response persistence.

## Requirements

### 1. Create `IPromptExecutionService` Interface
**Location**: `Core/Services/Interfaces/`
- `ExecutePromptAsync()` method - main execution orchestration
- `GetProviderStatusAsync()` method - check all provider availability
- `ValidatePromptAsync()` method - validate request before execution

### 2. Implement `PromptExecutionService`
**Location**: `Infrastructure/Services/`

**Constructor dependencies:**
- `ILlmProvider` (Google Gemini provider)
- `IRateLimitService`
- `ApplicationDbContext` (for persistence)
- `ILogger`

### 3. Implement `ExecutePromptAsync()` Method
**Workflow:**
1. Validate input (prompt length, model availability)
2. Check rate limits (via IRateLimitService)
3. Load conversation if provided (from database)
4. Load context files if provided (from database)
5. Build complete prompt with context
6. Call provider's GenerateAsync()
7. Save Prompt entity to database
8. Save Response entity to database
9. Record rate limit usage
10. Return response with metadata

### 4. Implement Error Handling and Fallbacks
- Validate prompt not empty/too long
- Handle provider unavailability
- Handle rate limit exceeded (return 429)
- Handle database errors
- Log all steps with correlation IDs
- Return meaningful error messages to API

### 5. Implement Context File Handling
- Load file content from database by IDs
- Append to system prompt or user prompt
- Track which files were used in response

### 6. Implement Conversation Context
- Load previous prompts/responses for conversation
- Build conversation history for provider
- Maintain token budget across conversation

## Acceptance Criteria
- [x] Complete orchestration flow implemented
- [x] All dependencies properly injected
- [x] Database persistence working (Prompt, Response entities)
- [x] Context files correctly included in prompts
- [x] Conversation history properly managed
- [x] Rate limiting checked before execution
- [x] Comprehensive error handling with logging
- [x] Unit tests with mocked dependencies (80%+ coverage)
- [x] Integration tests with in-memory database

## Technical Constraints
- All database operations must be async
- Use transactions for multi-step database operations
- Calculate and store cost/tokens for each response
- Track execution latency
- Use cancellation tokens for long-running operations
- Follow single responsibility principle

## Note
This service was later refactored into three pipeline stages (Tasks 10-12) for better separation of concerns.

## Related Tasks
- Depends on: Task 01, Task 03, Task 04
- Enables: Task 08 (API Controllers)
- Refactored into: Tasks 10-12 (Pipeline Pattern)
