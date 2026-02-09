# Task 19: Backend API Integration and Data Flow

**Status**: ✅ Done  
**Priority**: High  
**Estimated Effort**: 3-4 hours  
**GitHub Issue**: [#24](https://github.com/akratovich-pl/ghc-promt-lab/issues/24)

## Description
Integrate animated frontend with backend API, implement prompt execution flow, real-time token calculation, and connect all components with live data from the backend.

## Requirements

### 1. Update API Service
Update `services/api.ts` with endpoints:
- `GET /api/providers` - List available LLM providers
- `POST /api/prompts/execute` - Execute prompt
- `POST /api/prompts/calculate-tokens` - Token estimation

### 2. Wire Up Components
- Load providers into model selection page
- Execute prompt from InputBlock
- Display results in OutputBlock
- Update metrics in real-time

### 3. Implement Error Handling
- Loading states show during API calls
- Errors display user-friendly messages
- Add debounced token calculation (500ms)

### 4. Update Pinia Stores
- Update stores with API responses
- Maintain data flow through stores
- Handle async state management

## Acceptance Criteria
- [x] Model selection loads providers from backend
- [x] Prompt execution calls backend API
- [x] Token calculation updates on input change (debounced)
- [x] Response displays in OutputBlock with metrics
- [x] Loading states show during API calls
- [x] Errors display user-friendly messages
- [x] All data flows through Pinia stores
- [x] Connection animations trigger on API state changes

## Technical Details
- Axios for HTTP requests
- Debouncing for token calculation
- Error handling and loading states
- Reactive data flow through Pinia

## Dependencies
- Depends on: Task 18 (Components)
- Depends on: Task 08 (API Controllers - Backend)
- Depends on: Task 05 (Prompt Execution Service - Backend)

## Related Tasks
- Completes: Core Frontend-Backend Integration
- Enhanced by: Task 20 (Markdown Rendering)
- Enhanced by: Task 21 (Interactive Tooltips)
