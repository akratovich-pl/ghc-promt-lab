# Task 02: Implement Google Gemini Provider Configuration

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 2 hours  
**GitHub Issue**: [#4](https://github.com/akratovich-pl/ghc-promt-lab/issues/4)

## Objective
Implement configuration management for Google Gemini API provider, including secure API key storage and environment-based configuration.

## Requirements

### 1. Update `appsettings.json`
Add Google Gemini configuration section:
```json
"LlmProviders": {
  "GoogleGemini": {
    "Enabled": true,
    "BaseUrl": "https://generativelanguage.googleapis.com/v1beta",
    "DefaultModel": "gemini-pro",
    "MaxTokens": 8192,
    "Temperature": 0.7
  }
}
```

### 2. Create Configuration Classes
**Location**: `Infrastructure/Configuration/`
- `LlmProvidersOptions` class with nested `GoogleGeminiOptions`
- Properties: `Enabled`, `BaseUrl`, `DefaultModel`, `MaxTokens`, `Temperature`
- Add validation attributes (Required, Url, Range)

### 3. Implement `ILlmProviderConfig` in Infrastructure
- `LlmProviderConfigService` class
- Read API key from environment variable `GOOGLE_GEMINI_API_KEY`
- Implement methods to retrieve configuration values
- Add logging for configuration loading

### 4. Register Configuration in `Program.cs`
- Bind configuration section to options class
- Register `ILlmProviderConfig` as singleton
- Add validation on startup

## Acceptance Criteria
- [x] Configuration classes properly bound from appsettings.json
- [x] API key retrieved from environment variable (never hardcoded)
- [x] Configuration validation works on startup
- [x] Logging added for configuration loading
- [x] Unit tests for configuration service
- [x] README updated with environment variable setup instructions

## Technical Constraints
- API keys MUST be stored in environment variables, never in config files
- Use IOptions pattern for configuration
- Add validation attributes to all configuration classes
- Follow ASP.NET Core configuration best practices

## Related Tasks
- Depends on: Task 01 (ILlmProviderConfig interface)
- Enables: Task 03 (Google Gemini API Client)
