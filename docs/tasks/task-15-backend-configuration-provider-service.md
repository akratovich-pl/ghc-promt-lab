# Task 15: Implement Configuration-Based Provider Service

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 4 hours  
**GitHub Issue**: [#32](https://github.com/akratovich-pl/ghc-promt-lab/issues/32)

## Objective
Replace `MockProviderService` with `ConfigurationProviderService` that reads provider configurations from `appsettings.json` and API keys from User Secrets.

## Requirements

### 1. User Secrets Configuration
- Migrate from `.env` file to User Secrets for security
- Configure API keys using `dotnet user-secrets`:
  - `Providers:Google:ApiKey`
  - `Providers:Groq:ApiKey`
- Remove `.env` file dependency

### 2. Configuration Models
Create configuration classes:
- `ProviderSettings` - provider metadata (name, base URL, models collection)
- `ModelSettings` - model specifications (name, display name, max tokens, pricing)

### 3. Configuration Data
Populate `appsettings.json` with Google and Groq provider configurations:
- Provider metadata (names, base URLs)
- Model specifications (gemini-1.5-flash, gemini-1.5-pro, llama models)
- Pricing information (input/output costs per 1k tokens)
- Token limits

### 4. Service Implementation
Create `ConfigurationProviderService` implementing `IProviderService`:
- `GetProvidersAsync()` - return list of configured providers
- `GetProviderStatusAsync()` - check real-time provider health via `ILlmProvider.IsAvailable()`
- `GetProviderModelsAsync()` - return models for specified provider

**Health check logic:**
- If API key missing → unavailable with appropriate message
- If configured → call provider's health endpoint
- Return accurate status with timestamp

### 5. Integration
- Update `LlmProviderConfig` to read API keys from IConfiguration (User Secrets)
- Register `ConfigurationProviderService` in DI container (replace `MockProviderService`)
- Delete `MockProviderService.cs`

### 6. Validation
- Verify `/api/providers` endpoints return correct data
- Test graceful handling when API keys are missing
- Confirm health checks reflect actual provider availability

## Acceptance Criteria
- [x] User Secrets configured for API keys
- [x] Configuration models created and bound
- [x] appsettings.json populated with provider metadata
- [x] ConfigurationProviderService implemented
- [x] All provider endpoints working correctly
- [x] Health checks accurate
- [x] MockProviderService removed
- [x] Security improved (API keys not in code or .env)
- [x] Documentation updated

## Scope
Focus on **Google and Groq** providers only. Other providers (OpenAI, Anthropic) will be added in future tasks.

## Technical Constraints
- Use User Secrets for development
- Use environment variables or Key Vault for production
- No hardcoded API keys
- Configuration validation on startup
- Health checks must reflect real provider availability

## Security Notes
- API keys never committed to repository
- User Secrets used for local development
- Environment variables for production deployment
- Configuration section documented in README

## Related Tasks
- Improves: Task 02 (Configuration)
- Enhances: Task 03 (Provider Implementation)
