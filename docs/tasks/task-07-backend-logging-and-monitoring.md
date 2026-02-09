# Task 07: Add Logging and Monitoring

**Status**: ✅ Done  
**Priority**: Low  
**Estimated Effort**: 2 hours  
**GitHub Issue**: [#11](https://github.com/akratovich-pl/ghc-promt-lab/issues/11)

## Objective
Implement comprehensive logging and monitoring infrastructure to track LLM operations, costs, performance, and errors.

## Requirements

### 1. Configure Structured Logging with Serilog
- Install Serilog packages (Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File)
- Configure in `Program.cs`
- Log to console (development) and file (production)
- Use structured logging format (JSON)

### 2. Add Logging to Key Components

**PromptExecutionService:**
- Log prompt execution start (with correlation ID)
- Log provider selection
- Log token counts and costs
- Log execution duration
- Log errors with full context

**GoogleGeminiProvider:**
- Log API request (without sensitive data)
- Log API response metadata (tokens, latency)
- Log retry attempts
- Log rate limit hits
- Log all errors

**Controllers:**
- Log incoming requests (HTTP method, path, query params)
- Log validation errors
- Log rate limit rejections
- Log response status codes

### 3. Create Custom Log Event IDs
Define in `LogEvents.cs`:
- PromptExecutionStarted = 1000
- PromptExecutionCompleted = 1001
- PromptExecutionFailed = 1002
- ApiRequestSent = 2000
- ApiResponseReceived = 2001
- RateLimitExceeded = 3000
- ConfigurationLoaded = 4000

### 4. Add Performance Metrics
Track and log:
- Average prompt execution time
- Token usage per hour/day
- Total cost per hour/day
- API error rate
- Rate limit hit rate
- Cache hit rate (if implemented)

### 5. Add Health Check Endpoints
Create `/health` endpoint with:
- Application status (up/down)
- Database connectivity
- LLM provider availability
- Cache availability
- Last successful API call timestamp

### 6. Configure Log Levels by Environment
- Development: Debug level, console output
- Production: Information level, file output
- Include request correlation IDs

## Acceptance Criteria
- [x] Serilog configured and working
- [x] All key operations logged with structured data
- [x] No sensitive data in logs (API keys, user data)
- [x] Log correlation IDs present in all entries
- [x] Performance metrics calculated and logged
- [x] Health check endpoint responds correctly
- [x] Log files rotate daily (max 30 days retention)
- [x] Console logs readable in development
- [x] Production logs in JSON format

## Technical Constraints
- Use Serilog for structured logging
- Never log API keys or sensitive user data
- Use correlation IDs for request tracing
- Log levels configurable via appsettings.json
- File logs with daily rolling (retention: 30 days)

## Implementation Summary
See [TASK_8_IMPLEMENTATION_SUMMARY.md](TASK_8_IMPLEMENTATION_SUMMARY.md) for detailed implementation notes.

## Future Enhancements
- Application Insights integration
- Custom metrics dashboard
- Alert rules for errors/rate limits
- Distributed tracing with OpenTelemetry

## Related Tasks
- Enhances: Tasks 03, 05, 08
