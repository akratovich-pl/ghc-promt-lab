# Task: Implement Global Exception Filter

## Context
`PromptsController` has ~307 lines with significant duplication. Each of the 4 endpoints contains nearly identical try-catch blocks (~25 lines each), resulting in ~100 lines of repetitive error handling code.

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
- Inject `ILogger<GlobalExceptionFilter>` for centralized error logging

### Exception Mapping Strategy
Handle common exceptions:
- `ArgumentException` / `ArgumentNullException` → 400 Bad Request with validation details
- `InvalidOperationException` → 400 Bad Request with operation context
- `KeyNotFoundException` → 404 Not Found
- `UnauthorizedAccessException` → 403 Forbidden (future-ready)
- Generic `Exception` → 500 Internal Server Error with sanitized message

### ProblemDetails Construction
Standardize error responses:
- Consistent structure across all error types
- Include correlation ID from HTTP context (if available)
- Sanitize internal error details for 500 errors (don't leak sensitive info)
- Preserve detailed messages for 4xx errors (client-actionable)

### Controller Refactoring
Update `PromptsController`:
- Remove all try-catch blocks from endpoints
- Remove ProblemDetails construction code
- Keep business logic and service calls
- Reduce each method to 5-10 lines (core logic only)

### Registration
Update `Program.cs`:
- Register exception filter globally: `services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>())`
- Alternative: Use `[ExceptionFilter]` attribute if scoped registration preferred
- Ensure proper DI scope for logger injection

## Constraints
- **No breaking changes** - maintain same HTTP status codes and response structure for existing clients
- **Preserve logging** - maintain same log levels and information
- **Keep error messages** - don't change exception messages exposed to clients
- **Environment-aware** - show detailed errors in Development, sanitized in Production
- **Backward compatibility** - existing API contracts remain unchanged

## Success Criteria
- [ ] Build succeeds without warnings
- [ ] All existing tests pass without modification
- [ ] `PromptsController` reduced from ~307 lines to ~180-200 lines
- [ ] No try-catch blocks remain in controller methods
- [ ] Same HTTP status codes returned for same error conditions
- [ ] Error response structure matches current `ProblemDetails` format
- [ ] Integration tests verify error handling behavior unchanged

## Testing Strategy
- Create unit tests for `GlobalExceptionFilter`:
  - Test exception-to-status-code mapping
  - Test ProblemDetails construction for each exception type
  - Test logging behavior (verify correct log levels)
- Verify existing controller integration tests pass unchanged
- Add test for unexpected exception type (should return 500)
- Test environment-specific behavior (Development vs Production error details)

## Dependencies
None - uses built-in ASP.NET Core features

## Follow-up Tasks
After completion, enables:
- **Phase 2**: Add AutoMapper/Mapster for DTO mapping (additional ~40 lines reduction)
- **Future**: Result Pattern adoption for exception-free flow
- **Future**: Custom exception types for richer error context (e.g., `ValidationException`, `ResourceNotFoundException`)

## Implementation Notes

### Exception Filter Example Structure
```csharp
public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (statusCode, title) = MapException(context.Exception);
        context.Result = new ObjectResult(new ProblemDetails { ... });
        context.ExceptionHandled = true;
    }
}
```

### Environment-Specific Details
- **Development**: Include stack traces and inner exceptions
- **Production**: Sanitize error details, log full exception internally
- Use `IWebHostEnvironment` to detect environment

### Logging Considerations
- Log all exceptions with correlation ID
- Use appropriate log levels:
  - 4xx errors: `LogWarning` (client errors)
  - 5xx errors: `LogError` (server errors)
- Include request path, method, and user context in logs

## Estimated Impact
- Lines removed from controller: ~100-120
- New code added: ~60-80 (filter class + tests)
- Net reduction: ~40+ lines
- Maintainability: Significantly improved (single source of truth for error handling)
- Future endpoints: Inherit error handling automatically, no boilerplate needed

## Benefits
- **DRY Principle**: Error handling defined once, applied everywhere
- **Consistency**: All endpoints return uniform error responses
- **Maintainability**: Single place to update error handling logic
- **Future-proof**: New endpoints automatically get proper error handling
- **Testability**: Error handling logic testable in isolation
