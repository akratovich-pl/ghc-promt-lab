# Task 08: Implement API Controllers

**Status**: ✅ Done (Optimized in Tasks 13-14)  
**Priority**: High  
**Estimated Effort**: 3 hours  
**GitHub Issue**: [#9](https://github.com/akratovich-pl/ghc-promt-lab/issues/9)

## Objective
Create REST API controllers that expose LLM functionality with proper OpenAPI documentation, validation, and error handling.

## Requirements

### 1. Create `PromptsController`
**Location**: `Api/Controllers/`

**Endpoints:**
- `POST /api/prompts/execute` - Execute a prompt
- `POST /api/prompts/estimate` - Estimate tokens for a prompt
- `GET /api/prompts/{id}` - Get prompt by ID with response
- `GET /api/prompts/conversation/{conversationId}` - Get all prompts in conversation

### 2. Create Request/Response Models
**Location**: `Api/Models/`
- `ExecutePromptRequest` (Prompt, SystemPrompt, ConversationId?, ContextFileIds?, Model?, MaxTokens?, Temperature?)
- `ExecutePromptResponse` (Id, Content, InputTokens, OutputTokens, Cost, LatencyMs, Model, CreatedAt)
- `EstimateTokensRequest` (Prompt, Model?)
- `EstimateTokensResponse` (TokenCount, EstimatedCost)
- `PromptDetailResponse` (Prompt data with Response)

### 3. Implement Controller Methods
- Use `IPromptExecutionService` for business logic
- Add model validation with FluentValidation or DataAnnotations
- Return appropriate HTTP status codes (200, 400, 429, 500)
- Add cancellation token support
- Include OpenAPI documentation attributes

### 4. Add Request Validation
- Prompt required, max length 50,000 characters
- Model name must be valid
- MaxTokens between 1 and 8192
- Temperature between 0.0 and 2.0
- ContextFileIds must exist in database

### 5. Create `ProvidersController`
**Location**: `Api/Controllers/`

**Endpoints:**
- `GET /api/providers` - List all available providers
- `GET /api/providers/{name}/status` - Get provider health status
- `GET /api/providers/{name}/models` - List available models

### 6. Add OpenAPI Documentation
- XML comments on all endpoints
- Example request/response bodies
- Describe all status codes
- Add tags for grouping in Swagger UI

## Acceptance Criteria
- [x] All endpoints implemented and functional
- [x] Request validation working correctly
- [x] Proper HTTP status codes returned
- [x] OpenAPI/Swagger documentation complete
- [x] Error responses use ProblemDetails format
- [x] CORS configured for frontend access
- [x] Rate limiting applied to all endpoints
- [x] Unit tests for controller actions
- [x] Integration tests with TestServer
- [x] Postman/Swagger tested manually

## Technical Constraints
- Use ASP.NET Core controller-based APIs
- Follow REST conventions
- Use async/await for all I/O operations
- Return ProblemDetails for errors (RFC 7807)
- Enable XML documentation output
- Add [ApiController] attribute for automatic validation

## Note
This controller was later optimized with global exception filter and static mapping extensions (Tasks 13-14).

## Related Tasks
- Depends on: Task 05 (IPromptExecutionService)
- Depends on: Task 04 (Rate limiting middleware)
- Optimized in: Tasks 13-14
