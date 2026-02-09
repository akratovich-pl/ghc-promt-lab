# Frontend API Integration Guide

## Overview

This document describes the frontend integration with the backend API for the PromptLab application. 
The integration includes real-time token calculation, prompt execution, and provider management.

## Architecture

### State Management (Pinia Stores)

#### Provider Store (`stores/providerStore.ts`)
Manages LLM provider selection and availability.

**State:**
- `providers`: List of available providers
- `selectedProvider`: Currently selected provider name
- `selectedModel`: Currently selected model name
- `models`: Detailed model information
- `loading`: Loading state
- `error`: Error messages

**Actions:**
- `fetchProviders()`: Load providers from `/api/providers`
- `fetchModels(providerName)`: Load models for a specific provider
- `setSelectedProvider(name)`: Change selected provider
- `setSelectedModel(name)`: Change selected model

#### Prompt Store (`stores/promptStore.ts`)
Manages prompt execution and token estimation.

**State:**
- `currentPrompt`: User's current prompt text
- `systemPrompt`: Optional system prompt
- `response`: AI response data
- `tokenEstimate`: Token count and cost estimation
- `loading`: Overall loading state
- `executing`: Prompt execution in progress
- `estimating`: Token estimation in progress
- `error`: Error messages
- `conversationId`: Optional conversation context

**Actions:**
- `executePrompt(prompt, model?, systemPrompt?, maxTokens?, temperature?)`: Execute prompt
- `estimateTokens(prompt, model?)`: Estimate token count (debounced)
- `clearResponse()`: Clear response data
- `clearTokenEstimate()`: Clear token estimate
- `setCurrentPrompt(prompt)`: Update prompt text
- `setSystemPrompt(prompt)`: Update system prompt
- `setConversationId(id)`: Set conversation context

### API Service (`services/api.ts`)

Centralized API client using Axios with:
- Base URL configuration: `/api`
- Request/response interceptors for logging
- Error handling
- 30-second timeout

**Endpoints:**
- `GET /api/providers` - List available providers
- `GET /api/providers/{name}/models` - Get models for provider
- `POST /api/prompts/execute` - Execute prompt
- `POST /api/prompts/estimate` - Estimate tokens

### Components

#### ModelSelection.vue
Provider and model selection interface.

**Features:**
- Loads providers on mount
- Displays availability status
- Auto-selects first available provider
- Shows loading and error states
- Real-time status indicator

#### InputBlock.vue
Prompt input interface with token estimation.

**Features:**
- System prompt input (optional)
- Main prompt textarea
- Debounced token calculation (500ms using VueUse)
- Real-time token/cost display
- Execute and Clear buttons
- Loading states during execution
- Error display
- GSAP animations on interaction

**Token Estimation:**
- Triggers automatically on prompt text change
- Debounced by 500ms to avoid excessive API calls
- Re-calculates when model changes
- Displays token count and estimated cost

#### OutputBlock.vue
AI response display with metrics.

**Features:**
- Loading spinner during execution
- Animated metrics cards (tokens, cost, latency)
- Syntax-highlighted response content
- Copy to clipboard functionality
- Empty state when no response
- GSAP entrance animations

## API Integration Details

### Request/Response Flow

1. **Provider Selection:**
   ```
   User loads page → fetchProviders() → GET /api/providers → Display providers
   User selects provider → Auto-select first model
   ```

2. **Token Estimation (Debounced):**
   ```
   User types prompt → Wait 500ms → estimateTokens() 
   → POST /api/prompts/estimate → Display estimate
   ```

3. **Prompt Execution:**
   ```
   User clicks Execute → executePrompt() → POST /api/prompts/execute 
   → Display response with metrics → Animate results
   ```

### Error Handling

All API calls include comprehensive error handling:

- **Network Errors**: Caught and displayed to user
- **HTTP Errors**: Extracts error details from response
- **Validation Errors**: Displays server validation messages
- **Rate Limiting**: Handled with user-friendly messages

Example error structure:
```typescript
{
  status: 400,
  title: "Invalid Request",
  detail: "Prompt is required"
}
```

### Loading States

Multiple loading indicators for better UX:

- **Provider Loading**: Spinner in ModelSelection
- **Executing**: Button spinner + disabled state
- **Estimating**: Small spinner next to token estimate
- **Global Loading**: Overlay in OutputBlock

## Animations

GSAP animations enhance user experience:

1. **Page Load**: Staggered fade-in of components
2. **Logo Click**: Bounce and rotate animation
3. **Prompt Execute**: Scale pulse on InputBlock
4. **Response Arrival**: Slide-up and fade-in
5. **Metrics Cards**: Staggered entrance animation

## TypeScript Types

All API models are typed in `types/api.ts`:

- `ProviderInfoResponse`
- `ModelInfoResponse`
- `ExecutePromptRequest`
- `ExecutePromptResponse`
- `EstimateTokensRequest`
- `EstimateTokensResponse`
- `ApiError`

## Configuration

### Vite Proxy
Configured in `vite.config.ts` to proxy `/api` requests to `http://localhost:5000`

### API Timeouts
- Default: 30 seconds for all requests
- Can be overridden per request if needed

## Development Workflow

1. **Start Backend:**
   ```bash
   cd src/PromptLab.Api
   dotnet run
   ```

2. **Start Frontend:**
   ```bash
   cd client
   npm run dev
   ```

3. **Access Application:**
   - Frontend: http://localhost:5173
   - API: http://localhost:5000/api

## Testing Checklist

- [ ] Providers load on page mount
- [ ] Model selection updates when provider changes
- [ ] Token estimation triggers on prompt input (debounced)
- [ ] Token estimation updates when model changes
- [ ] Execute button disabled when no prompt or model
- [ ] Loading spinner shows during execution
- [ ] Response displays with all metrics
- [ ] Animations trigger correctly
- [ ] Error messages display properly
- [ ] Copy to clipboard works
- [ ] Clear button resets all states

## Future Enhancements

- [ ] Conversation history
- [ ] Context file upload integration
- [ ] Response comparison for multiple models
- [ ] Export functionality
- [ ] Streaming responses
- [ ] Retry logic for failed requests
- [ ] Offline mode detection
- [ ] Response caching

## Troubleshooting

### Backend Connection Issues
- Verify backend is running on port 5000
- Check Vite proxy configuration
- Inspect browser console for CORS errors

### Token Estimation Not Working
- Check debounce timing (500ms)
- Verify model is selected
- Check browser console for API errors

### Components Not Rendering
- Verify all dependencies installed: `npm install`
- Check TypeScript compilation: `npm run build`
- Review browser console for errors
