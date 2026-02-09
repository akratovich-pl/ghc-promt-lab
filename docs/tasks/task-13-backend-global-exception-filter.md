# Task 13: Implement Global Exception Filter (Controller Refactoring Phase 1)

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 2 hours  
**GitHub Issue**: [#42](https://github.com/akratovich-pl/ghc-promt-lab/issues/42)

## Context
`PromptsController` had ~307 lines with significant duplication. Each of the 4 endpoints contained nearly identical try-catch blocks (~25 lines each), resulting in ~100 lines of repetitive error handling code.

## Objective
Implement global exception handling using ASP.NET Core exception filter to centralize error responses and reduce controller bloat by 80-100 lines.

## Current State
- `PromptsController`: 4 endpoints, ~307 total lines
- Each endpoint has manual try-catch blocks for:
  - `ArgumentException` → 400 Bad Request
  - `InvalidOperationException` → 400 Bad Request
  - Generic `Exception` → 500 Internal Server Error
- Manual `ProblemDetails` construction in every catch block
- Duplicate logging patterns across all methods

## Requirements

### Exception Filter Implementation
Create `GlobalExceptionFilter` in `Api/Filters/`:
- Implement `IExceptionFilter` interface
- Map exception types to HTTP status codes and ProblemDetails
- Preserve original exception messages and logging context
- Inject `ILogger` for centralized error logging

### Exception Mapping Strategy
Handle common exceptions:
- `ArgumentException` / `ArgumentNullException` → 400 Bad Request
- `InvalidOperationException` → 400 Bad Request
- `KeyNotFoundException` → 404 Not Found
- `UnauthorizedAccessException` → 403 Forbidden (future-ready)
- Generic `Exception` → 500 Internal Server Error

### ProblemDetails Construction
Standardize error responses:
- Consistent structure across all error types
- Include correlation ID from HTTP context (if available)
- Sanitize internal error details for 500 errors
- Preserve detailed messages for 4xx errors

### Controller Refactoring
Update `PromptsController`:
- Remove all try-catch blocks from endpoints
- Remove ProblemDetails construction code
- Keep business logic and service calls
- Reduce each method to 5-10 lines (core logic only)

### Registration
Update `Program.cs`:
- Register exception filter globally: `services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>())`

## Acceptance Criteria
- [x] Build succeeds without warnings
- [x] All existing tests pass without modification
- [x] `PromptsController` reduced from ~307 lines to ~180-200 lines
- [x] No try-catch blocks remain in controller methods
- [x] Same HTTP status codes returned for same error conditions
- [x] Error response structure matches current `ProblemDetails` format
- [x] Integration tests verify error handling behavior unchanged

## Technical Constraints
- No breaking changes - maintain same HTTP status codes and response structure
- Preserve logging - same log levels and information
- Keep error messages - don't change exception messages
- Environment-aware - detailed errors in Development, sanitized in Production
- Backward compatibility

## Testing Strategy
- Create unit tests for `GlobalExceptionFilter`:
  - Test exception-to-status-code mapping
  - Test ProblemDetails construction for each exception type
  - Test logging behavior (verify correct log levels)
- Verify existing controller integration tests pass unchanged
- Add test for unexpected exception type (should return 500)
- Test environment-specific behavior

## Environment-Specific Details
- **Development**: Include stack traces and inner exceptions
- **Production**: Sanitize error details, log full exception internally
- Use `IWebHostEnvironment` to detect environment

## Estimated Impact
- Lines removed from controller: ~100-120
- New code added: ~60-80 (filter class + tests)
- Net reduction: ~40+ lines
- Maintainability: Significantly improved

## Benefits
- **DRY Principle**: Error handling defined once, applied everywhere
- **Consistency**: All endpoints return uniform error responses
- **Maintainability**: Single place to update error handling logic
- **Future-proof**: New endpoints automatically get proper error handling
- **Testability**: Error handling logic testable in isolation

## Related Tasks
- Part of: Controller Optimization
- Enables: Task 14 (Static Mapping Extensions)
