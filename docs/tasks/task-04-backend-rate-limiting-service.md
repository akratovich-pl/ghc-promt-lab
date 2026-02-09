# Task 04: Implement Rate Limiting Service

**Status**: ✅ Done  
**Priority**: Medium  
**Estimated Effort**: 3 hours  
**GitHub Issue**: [#7](https://github.com/akratovich-pl/ghc-promt-lab/issues/7)

## Objective
Implement an in-memory rate limiting service to prevent API quota exhaustion and protect against abuse during Phase 1 development.

## Requirements

### 1. Create `IRateLimitService` Interface
**Location**: `Core/Services/Interfaces/`
- `CheckRateLimitAsync()` method - returns bool if request allowed
- `RecordRequestAsync()` method - tracks request timestamp
- `GetRemainingRequestsAsync()` method - returns available requests
- Support for different limit types (per-user, per-provider, global)

### 2. Implement `InMemoryRateLimitService`
**Location**: `Infrastructure/Services/`
- Use `MemoryCache` for storing request counts
- Sliding window rate limiting algorithm
- Configurable limits per time window (requests per minute/hour)
- Thread-safe implementation with concurrent collections

### 3. Create Rate Limit Configuration
Add to `appsettings.json`:
```json
"RateLimiting": {
  "RequestsPerMinute": 60,
  "RequestsPerHour": 1000,
  "Enabled": true
}
```

### 4. Create Custom Middleware for Rate Limiting
- `RateLimitMiddleware` to intercept API requests
- Return 429 Too Many Requests with Retry-After header
- Add rate limit headers to all responses (X-RateLimit-Limit, X-RateLimit-Remaining)

### 5. Register Service and Middleware in `Program.cs`
- Register `IRateLimitService` as singleton
- Add middleware to pipeline before controllers

## Acceptance Criteria
- [x] Rate limiting works correctly with sliding window algorithm
- [x] Thread-safe implementation (no race conditions)
- [x] Configurable limits from appsettings.json
- [x] Returns proper HTTP 429 status with headers
- [x] Rate limit bypassed for health check endpoints
- [x] Unit tests for rate limit logic
- [x] Integration tests for middleware behavior
- [x] Documentation for configuring rate limits

## Technical Constraints
- Use in-memory cache (MemoryCache) for Phase 1
- Thread-safe implementation required
- Must not block async operations
- Cache expiration aligned with time windows
- Prepare for Redis migration in Phase 2

## Future Enhancements
- Migrate to distributed cache (Redis) for multi-instance deployments
- Per-user rate limiting with authentication
- Different limits per API key tier

## Related Tasks
- Used by: Task 06 (Prompt Execution Service)
