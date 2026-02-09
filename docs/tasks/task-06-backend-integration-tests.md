# Task 06: Add Integration Tests

**Status**: ✅ Done  
**Priority**: Medium  
**Estimated Effort**: 4 hours  
**GitHub Issue**: [#10](https://github.com/akratovich-pl/ghc-promt-lab/issues/10)

## Objective
Create comprehensive integration tests to verify the entire LLM integration pipeline works correctly with mocked external dependencies.

## Requirements

### 1. Create Test Infrastructure
- New test class `LlmIntegrationTests` in `PromptLab.Tests/Integration/`
- Use `WebApplicationFactory` for integration testing
- Configure in-memory database for tests
- Mock HttpClient responses for Google Gemini API
- Setup test fixtures with sample data

### 2. Test Scenarios for Prompt Execution

**Happy Path Tests:**
- Execute simple prompt and verify response saved to database
- Execute prompt with conversation context
- Execute prompt with context files
- Verify token counting and cost calculation
- Verify latency tracking

**Error Handling Tests:**
- Handle Google API returning error response (400, 500)
- Handle network timeout
- Handle invalid API key
- Handle rate limit exceeded (429)
- Handle malformed API responses

**Edge Cases:**
- Empty prompt (validation error)
- Extremely long prompt (validation error)
- Invalid model name
- Missing required configuration
- Database connection failure

### 3. Test Rate Limiting Integration
- Verify rate limit headers in responses
- Test rate limit enforcement (429 after threshold)
- Verify rate limit reset after time window
- Test rate limit bypass for health checks

### 4. Test API Endpoints
Test all controller endpoints:
- POST `/api/prompts/execute` with various inputs
- POST `/api/prompts/estimate`
- GET `/api/prompts/{id}`
- GET `/api/providers`
- GET `/api/providers/google-gemini/status`

### 5. Test Database Persistence
- Verify Prompt entity saved correctly
- Verify Response entity saved with relationships
- Verify conversation context maintained
- Verify context file associations

### 6. Create Helper Methods
- `CreateTestPromptRequest()` - factory for test data
- `MockSuccessfulGeminiResponse()` - mock API response
- `MockErrorGeminiResponse()` - mock error scenarios
- `SeedTestData()` - populate test database

## Acceptance Criteria
- [x] All happy path scenarios covered
- [x] All error scenarios covered
- [x] All API endpoints have integration tests
- [x] Rate limiting tested end-to-end
- [x] Database persistence verified
- [x] Tests use mocked external dependencies (no real API calls)
- [x] Tests are isolated and can run in parallel
- [x] Clear test naming (Given_When_Then pattern)
- [x] All tests pass consistently
- [x] Test coverage >80% for integration layer

## Technical Constraints
- Use xUnit for test framework
- Use WebApplicationFactory for in-memory hosting
- Use in-memory SQLite for database tests
- Mock HttpClient using HttpMessageHandler
- Tests must run independently (no shared state)

## Related Tasks
- Depends on: All Tasks 1-5 completed
