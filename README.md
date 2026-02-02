# ghc-promt-lab
AI interaction visualization tool - Full-stack application for training and learning with AI prompt analysis, token metrics, and response comparison

## Architecture

### Backend
- **Framework**: .NET 8 Web API
- **Database**: Entity Framework Core with SQLite (dev) / PostgreSQL (prod)
- **Architecture**: Clean Architecture (Core → Infrastructure → API)

### Frontend
- **Framework**: Vue 3 with TypeScript
- **Build Tool**: Vite
- **State Management**: Pinia
- **Routing**: Vue Router
- **HTTP Client**: Axios

## Project Structure

```
/
├── src/
│   ├── PromptLab.Api/          # Web API project
│   ├── PromptLab.Core/         # Domain entities & interfaces
│   ├── PromptLab.Infrastructure/  # Data access & services
│   └── PromptLab.Tests/        # Unit & integration tests
├── client/                     # Vue 3 frontend
│   ├── src/
│   │   ├── components/         # Vue components
│   │   ├── views/              # Page components
│   │   ├── services/           # API services
│   │   ├── stores/             # Pinia stores
│   │   └── router/             # Vue Router config
│   └── package.json
├── docs/                       # Documentation
└── PromptLab.sln              # Solution file
```

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Visual Studio Code](https://code.visualstudio.com/) (recommended)

### Backend Setup

1. Navigate to the API project:
```bash
cd src/PromptLab.Api
```

2. Set up environment variables for LLM providers:
```bash
# For Google Gemini API (required for AI features)
export GOOGLE_GEMINI_API_KEY="your-api-key-here"

# On Windows (PowerShell)
$env:GOOGLE_GEMINI_API_KEY="your-api-key-here"

# On Windows (Command Prompt)
set GOOGLE_GEMINI_API_KEY=your-api-key-here
```

**Note**: Never commit API keys to the repository. They must be stored as environment variables.

3. Restore dependencies:
```bash
dotnet restore
```

4. Build the project:
```bash
dotnet build
```

5. Run the API:
```bash
dotnet run
```

The API will be available at `http://localhost:5000`

### Frontend Setup

1. Navigate to the client folder:
```bash
cd client
```

2. Install dependencies:
```bash
npm install
```

3. Run the development server:
```bash
npm run dev
```

The frontend will be available at `http://localhost:5173`

## Build Commands

### Backend
```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run in watch mode
dotnet watch run --project src/PromptLab.Api
```

### Frontend
```bash
# Development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Development

### LLM Provider Configuration

The application uses environment variables for API keys and appsettings.json for provider configuration.

#### Environment Variables
- `GOOGLE_GEMINI_API_KEY` - API key for Google Gemini (required)

#### Configuration File (appsettings.json)
The LLM provider settings are configured in `src/PromptLab.Api/appsettings.json`:

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

### Database Migrations (when ready)
### Database Migrations

The database has been initialized with the following entities:
- Conversations
- Prompts
- Responses
- ContextFiles

**Initial Setup (already done):**
```bash
cd src/PromptLab.Api
dotnet ef database update
```

**Creating New Migrations:**
```bash
cd src/PromptLab.Api
dotnet ef migrations add <MigrationName> --project ../PromptLab.Infrastructure --output-dir Data/Migrations
dotnet ef database update
```

**Reverting Migrations:**
```bash
cd src/PromptLab.Api
# Remove last migration
dotnet ef migrations remove --project ../PromptLab.Infrastructure

# Revert to specific migration
dotnet ef database update <MigrationName>
```

## Features (Planned)
- ✅ Project structure setup
- ✅ Google Gemini API integration
- ✅ Rate limiting service (in-memory, sliding window)
- 🔄 AI prompt execution
- 🔄 Token counting and metrics
- 🔄 Response comparison
- 🔄 Context file upload
- 🔄 Conversation history
- 🔄 Export functionality

## Configuration

### Google Gemini API Setup

To use the Google Gemini API provider, you need to configure your API key in `appsettings.json`:

```json
{
  "LlmProviders": {
    "GoogleGemini": {
      "ApiKey": "YOUR_GOOGLE_API_KEY_HERE",
      "BaseUrl": "https://generativelanguage.googleapis.com",
      "Model": "gemini-pro",
      "ApiVersion": "v1",
      "MaxRetries": 3,
      "TimeoutSeconds": 30,
      "InputTokenCostPer1K": 0.00025,
      "OutputTokenCostPer1K": 0.0005
    }
  }
}
```

Get your API key from [Google AI Studio](https://makersuite.google.com/app/apikey).

### Manual Integration Testing

To manually test the Google Gemini provider:

1. Set your API key in `appsettings.json` or `appsettings.Development.json`
2. Use the `ILlmProvider` service in your code:

```csharp
// Inject ILlmProvider in your controller/service
public class ExampleController : ControllerBase
{
    private readonly ILlmProvider _llmProvider;

    public ExampleController(ILlmProvider llmProvider)
    {
        _llmProvider = llmProvider;
    }

    public async Task<IActionResult> Generate()
    {
        var request = new LlmRequest
        {
            Prompt = "Hello, what can you do?",
            Temperature = 0.7
        };

        var response = await _llmProvider.GenerateAsync(request);
        return Ok(response);
    }
}
```

3. Run the API and make a request to your endpoint
## Documentation

- [Rate Limiting Configuration](docs/rate-limiting.md) - Configure and use the API rate limiting feature

## License
MIT
