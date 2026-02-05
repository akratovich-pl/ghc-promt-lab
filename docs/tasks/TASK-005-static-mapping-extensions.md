# Task: Implement Static Mapping Extensions

## Context
`PromptsController` optimization Phase 2. After exception filter implementation, the controller still contains ~40-60 lines of manual DTO mapping code across 4 endpoints. Each method manually constructs response objects with property-by-property assignments.

## Objective
Create static extension methods for mapping between service layer DTOs and API response models to eliminate repetitive mapping code and reduce controller to pure orchestration logic.

## Current State
- `PromptsController`: 4 endpoints with inline mapping logic
- Manual response construction in each method:
  - `ExecutePrompt`: Maps `PromptExecutionResult` → `ExecutePromptResponse`
  - `EstimateTokens`: Maps `TokenEstimate` → `EstimateTokensResponse`
  - `GetPromptById`: Maps `PromptExecutionResult` → `PromptDetailResponse` (complex)
  - `GetPromptsByConversation`: Maps `List<PromptExecutionResult>` → `List<PromptDetailResponse>`
- Duplicate property assignments across methods
- No centralized mapping logic

## Requirements

### Extension Methods Location
Create `PromptMappingExtensions` in `Api/Extensions/`:
- Static class with extension methods
- Organized by source type (e.g., `PromptExecutionResult` extensions)
- Clear, descriptive method names (`ToResponse`, `ToDetailResponse`, etc.)
- XML documentation for each mapping method

### Required Mappings

#### 1. ExecutePromptResponse Mapping
```csharp
public static ExecutePromptResponse ToResponse(this PromptExecutionResult result)
```
Maps: `PromptExecutionResult` → `ExecutePromptResponse`
Properties: Id, Content, InputTokens, OutputTokens, Cost, LatencyMs, Model, CreatedAt

#### 2. EstimateTokensResponse Mapping
```csharp
public static EstimateTokensResponse ToResponse(this TokenEstimate estimate)
```
Maps: `TokenEstimate` → `EstimateTokensResponse`
Properties: TokenCount, EstimatedCost, Model

#### 3. PromptDetailResponse Mapping
```csharp
public static PromptDetailResponse ToDetailResponse(
    this PromptExecutionResult result, 
    Guid conversationId)
```
Maps: `PromptExecutionResult` → `PromptDetailResponse`
Properties: Id, ConversationId, UserPrompt, Context, ContextFileId, EstimatedTokens, ActualTokens, CreatedAt, Responses

#### 4. ResponseDetail Mapping (Helper)
```csharp
public static ResponseDetail ToResponseDetail(this PromptExecutionResult result)
```
Maps: `PromptExecutionResult` → `ResponseDetail` (nested object)
Properties: Id, Provider, Model, Content, Tokens, Cost, LatencyMs, CreatedAt

#### 5. Collection Mapping (Optional Helper)
```csharp
public static List<PromptDetailResponse> ToDetailResponses(
    this IEnumerable<PromptExecutionResult> results, 
    Guid conversationId)
```
Convenience method for mapping collections

### Controller Refactoring
Update `PromptsController` to use extension methods:
- Replace all manual mapping with extension method calls
- Remove inline object initializers for responses
- Reduce each method body to: service call → map → return
- Target: 3-5 lines per endpoint (excluding attributes/docs)

### File Organization
```
Api/
  Extensions/
    PromptMappingExtensions.cs  (NEW)
    RateLimitingExtensions.cs   (existing)
```

## Constraints
- **No dependencies** - pure C#, no NuGet packages (no AutoMapper)
- **No breaking changes** - response structure must remain identical
- **Type safety** - all mappings compile-time checked
- **Null handling** - properly handle null/empty values where needed
- **Performance** - direct property assignment, no reflection

## Success Criteria
- [ ] Build succeeds without warnings
- [ ] All existing tests pass without modification
- [ ] `PromptsController` reduced from ~180-200 lines (post-filter) to ~120-140 lines
- [ ] No manual response construction remains in controller methods
- [ ] All mappings are in `PromptMappingExtensions` class
- [ ] Extension methods have XML documentation
- [ ] Response structure identical to current implementation
- [ ] Integration tests verify API responses unchanged

## Testing Strategy
- Create unit tests for `PromptMappingExtensions`:
  - Test each mapping method with valid inputs
  - Test null handling (conversationId, contextFileIds, etc.)
  - Test collection mapping (empty list, single item, multiple items)
  - Verify all properties mapped correctly
- Verify existing controller integration tests pass unchanged
- No need to modify API contract tests

## Dependencies
- Requires Task 004 completion (Exception Filter) to be implemented first
- Otherwise, no external dependencies

## Implementation Notes

### Extension Method Example
```csharp
namespace PromptLab.Api.Extensions;

public static class PromptMappingExtensions
{
    /// <summary>
    /// Maps PromptExecutionResult to ExecutePromptResponse
    /// </summary>
    public static ExecutePromptResponse ToResponse(this PromptExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        
        return new ExecutePromptResponse
        {
            Id = result.PromptId,
            Content = result.Content,
            InputTokens = result.InputTokens,
            OutputTokens = result.OutputTokens,
            Cost = result.Cost,
            LatencyMs = result.LatencyMs,
            Model = result.Model,
            CreatedAt = result.CreatedAt
        };
    }
}
```

### Controller Usage
```csharp
[HttpPost("execute")]
public async Task<ActionResult<ExecutePromptResponse>> ExecutePrompt(
    [FromBody] ExecutePromptRequest request,
    CancellationToken cancellationToken)
{
    var result = await _promptService.ExecutePromptAsync(...);
    return Ok(result.ToResponse());
}
```

### Naming Conventions
- Use `ToResponse()` for simple mappings
- Use `ToDetailResponse()` for complex/nested mappings
- Use `ToResponseDetail()` for nested object mappings
- Include parameter context when mapping requires additional data (e.g., conversationId)

### Null Handling
- Add `ArgumentNullException.ThrowIfNull(result)` for required parameters
- Handle optional fields gracefully (null-coalescing, default values)
- Preserve existing null-handling behavior from controller

## Estimated Impact
- Lines removed from controller: ~40-60 (manual mapping code)
- New code added: ~80-120 (extension methods + tests)
- Net controller size: ~120-140 lines (60% reduction from original ~307)
- Maintainability: Centralized mapping logic, single responsibility
- Future endpoints: Reusable mappings, consistent patterns

## Benefits
- **DRY Principle**: Mapping logic defined once, reused everywhere
- **Readability**: Controller methods become simple and focused
- **Maintainability**: Changes to response structure in one place
- **Type Safety**: Compile-time checking, refactoring support
- **Performance**: Zero overhead, direct property assignment
- **Testability**: Mapping logic testable independently
- **No Dependencies**: Pure C#, no third-party libraries

## Future Extensibility
When to consider alternative approaches:
- **If API grows to 20+ endpoints**: Consider AutoMapper for convention-based mapping
- **If DTOs exceed 20+ properties**: Convention mapping reduces boilerplate
- **If complex nested mappings proliferate**: Mapping frameworks handle better
- **If bidirectional mapping needed**: AutoMapper profiles more elegant

For now, static extensions are optimal for project scale and team preferences.
