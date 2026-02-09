# Task 03: Implement Google Gemini API Client

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 4-5 hours  
**GitHub Issue**: [#5](https://github.com/akratovich-pl/ghc-promt-lab/issues/5)

## Objective
Create a fully functional Google Gemini API client that implements the ILlmProvider interface with proper error handling, retry logic, and comprehensive logging.

## Requirements

### 1. Create `GoogleGeminiProvider` Class
**Location**: `Infrastructure/Services/LlmProviders/`
- Implement `ILlmProvider` interface
- Constructor with `ILlmProviderConfig`, `IHttpClientFactory`, `ILogger`
- Use HttpClient for API calls

### 2. Implement `GenerateAsync()` Method
- Build Google Gemini API request payload
- Make POST request to `/models/{model}:generateContent`
- Parse response and map to `LlmResponse` DTO
- Calculate cost based on token usage
- Track latency with Stopwatch
- Handle streaming vs. non-streaming responses

### 3. Implement `EstimateTokensAsync()` Method
- Use `/models/{model}:countTokens` endpoint
- Return token count without making generation request
- Cache results for identical prompts (optional optimization)

### 4. Implement `IsAvailable()` Method
- Health check: verify API key is configured
- Optionally ping a lightweight endpoint to verify connectivity
- Return boolean status

### 5. Error Handling
- Try-catch with specific exception types (HttpRequestException, JsonException)
- Implement exponential backoff for retries (use Polly library)
- Handle rate limiting (429 status codes)
- Handle quota exceeded errors
- Log all errors with context

### 6. API Request/Response Models
Create internal DTOs for Google Gemini API format:
- `GeminiGenerateRequest` with Parts[], GenerationConfig
- `GeminiGenerateResponse` with Candidates[], UsageMetadata
- Proper JSON serialization attributes

## Acceptance Criteria
- [x] All ILlmProvider methods implemented correctly
- [x] HTTP requests properly formatted for Google Gemini API
- [x] Response parsing handles all success scenarios
- [x] Error handling covers all failure scenarios
- [x] Retry logic with exponential backoff (3 retries max)
- [x] Comprehensive logging (request, response, errors, latency)
- [x] Token count and cost calculation accurate
- [x] Unit tests with mocked HttpClient (90%+ coverage)
- [x] Integration test with real API documented

## Technical Constraints
- Use IHttpClientFactory for HttpClient instances
- Use System.Text.Json for serialization
- Implement retry logic with Polly library
- All HTTP calls must be async
- Timeout: 30 seconds for API calls

## Reference
- [Google Gemini API Documentation](https://ai.google.dev/docs)
- [REST API Reference](https://ai.google.dev/api/rest)

## Related Tasks
- Depends on: Task 01, Task 02
- Enables: Task 06 (Prompt Execution Service)
