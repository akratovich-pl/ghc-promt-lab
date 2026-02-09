# Architecture Visualization

## Layered Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          PRESENTATION LAYER                              │
│                         PromptLab.Api                                    │
│                                                                          │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐    │
│  │ PromptsController│  │ ProvidersCtrl    │  │ HealthController │    │
│  └────────┬─────────┘  └────────┬─────────┘  └──────────────────┘    │
│           │                     │                                       │
└───────────┼─────────────────────┼───────────────────────────────────────┘
            │                     │
            ▼                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        APPLICATION LAYER                                 │
│                   (Orchestration & Use Cases)                            │
│                                                                          │
│  ┌────────────────────────────────────────────────────────────┐        │
│  │           PromptExecutionService                            │        │
│  │   - ExecutePromptAsync()                                   │        │
│  │   - EstimateTokensAsync()                                  │        │
│  │   - GetPromptByIdAsync()                                   │        │
│  └──┬────┬────┬────┬────┬────┬────┬──────────────────────────┘        │
│     │    │    │    │    │    │    │                                    │
└─────┼────┼────┼────┼────┼────┼────┼────────────────────────────────────┘
      │    │    │    │    │    │    │
      │    │    │    │    │    │    └──────────────────┐
      │    │    │    │    │    │                       │
┌─────┼────┼────┼────┼────┼────┼───────────────────────┼────────────────┐
│     │    │    │    │    │    │   DOMAIN LAYER        │                │
│     │    │    │    │    │    │   (Business Logic)    │                │
│     │    │    │    │    │    │                       │                │
│     │    │    │    │    │    │  ┌────────────────────▼──────────────┐ │
│     │    │    │    │    │    │  │ ConversationHistoryService        │ │
│     │    │    │    │    │    │  │ - LoadConversationHistoryAsync()  │ │
│     │    │    │    │    │    │  │ - EnsureConversationAsync()       │ │
│     │    │    │    │    │    │  │ - UpdateConversationTimestamp()   │ │
│     │    │    │    │    │    │  └───────────────────────────────────┘ │
│     │    │    │    │    │    │                                        │
│     │    │    │    │    │    │  ┌────────────────────────────────────┐│
│     │    │    │    │    │    └─▶│ IRateLimitService                  ││
│     │    │    │    │    │       │ - CheckRateLimitAsync()            ││
│     │    │    │    │    │       │ - RecordRequestAsync()             ││
│     │    │    │    │    │       └────────────────────────────────────┘│
└─────┼────┼────┼────┼────┼────────────────────────────────────────────┘
      │    │    │    │    │
┌─────┼────┼────┼────┼────┼──────────────────────────────────────────────┐
│     │    │    │    │    │     REPOSITORY LAYER                         │
│     │    │    │    │    │     (Data Access Abstraction)                │
│     │    │    │    │    │                                              │
│     │    │    │    │    │    ┌──────────────────────────────────────┐ │
│     │    │    │    │    └───▶│ IPromptRepository                    │ │
│     │    │    │    │         │ - SavePromptExecutionAsync()         │ │
│     │    │    │    │         │ - GetPromptByIdAsync()               │ │
│     │    │    │    │         │ - GetPromptsByConversationIdAsync()  │ │
│     │    │    │    │         └────────┬─────────────────────────────┘ │
│     │    │    │    │                  │                               │
└─────┼────┼────┼────┼──────────────────┼───────────────────────────────┘
      │    │    │    │                  │
      │    │    │    │                  ▼
┌─────┼────┼────┼────┼───────────────────────────────────────────────────┐
│     │    │    │    │         INFRASTRUCTURE LAYER                      │
│     │    │    │    │         (External Integrations & Database)        │
│     │    │    │    │                                                   │
│     │    │    │    │    ┌────────────────────────────────────────┐    │
│     │    │    │    └───▶│ ILlmProvider                          │    │
│     │    │    │         │ - GoogleGeminiProvider                 │    │
│     │    │    │         │ - GroqProvider                         │    │
│     │    │    │         └────────────────────────────────────────┘    │
│     │    │    │                                                        │
│     │    │    │         ┌────────────────────────────────────────┐    │
│     │    │    │         │ ApplicationDbContext (EF Core)         │    │
│     │    │    │         │ - Prompts, Responses, Conversations    │    │
│     │    │    │         └────────────────────────────────────────┘    │
└─────┼────┼────┼────────────────────────────────────────────────────────┘
      │    │    │
┌─────┼────┼────┼────────────────────────────────────────────────────────┐
│     │    │    │         CROSS-CUTTING CONCERNS                         │
│     │    │    │                                                        │
│     │    │    │    ┌───────────────────────────────────────┐          │
│     │    └────┼───▶│ Validators                           │          │
│     │         │    │ - IPromptValidator                   │          │
│     │         │    │   * ValidatePromptRequest()          │          │
│     │         │    └───────────────────────────────────────┘          │
│     │         │                                                        │
│     │         │    ┌───────────────────────────────────────┐          │
│     └─────────┼───▶│ Builders                             │          │
│               │    │ - ILlmRequestBuilder                 │          │
│               │    │   * BuildRequest()                   │          │
│               │    │                                       │          │
│               └───▶│ - IPromptEnricher                    │          │
│                    │   * EnrichPromptWithContextAsync()   │          │
│                    └───────────────────────────────────────┘          │
└─────────────────────────────────────────────────────────────────────────┘
```

## Component Interactions Flow

### Prompt Execution Flow

```
1. Controller receives HTTP request
         ↓
2. PromptExecutionService.ExecutePromptAsync() called
         ↓
3. ┌─ IPromptValidator validates input
   ├─ IRateLimitService checks rate limits
   ├─ IConversationHistoryService loads history
   ├─ IPromptEnricher enriches prompt with context
   ├─ ILlmRequestBuilder builds LLM request
   ├─ ILlmProvider calls external LLM API
   ├─ IConversationHistoryService ensures conversation exists
   ├─ IPromptRepository saves to database
   ├─ IConversationHistoryService updates timestamp
   └─ IRateLimitService records usage
         ↓
4. Result returned to Controller
         ↓
5. HTTP response sent to client
```

## Dependency Graph

```
┌─────────────────────────────────────────────────────┐
│         PromptExecutionService                      │
│         (depends on 7 interfaces)                   │
└─┬─────┬─────┬─────┬─────┬─────┬─────┬─────────────┘
  │     │     │     │     │     │     │
  ▼     ▼     ▼     ▼     ▼     ▼     ▼
┌──┐  ┌──┐  ┌──┐  ┌──┐  ┌──┐  ┌──┐  ┌──┐
│V │  │D │  │D │  │B │  │B │  │R │  │I │
│a │  │o │  │o │  │u │  │u │  │e │  │n │
│l │  │m │  │m │  │i │  │i │  │p │  │f │
│i │  │a │  │a │  │l │  │l │  │o │  │r │
│d │  │i │  │i │  │d │  │d │  │s │  │a │
│a │  │n │  │n │  │e │  │e │  │i │  │s │
│t │  │  │  │  │  │r │  │r │  │t │  │t │
│o │  │  │  │  │  │1 │  │2 │  │o │  │r │
│r │  │S1│  │S2│  │  │  │  │  │r │  │u │
│  │  │  │  │  │  │  │  │  │  │y │  │c │
└──┘  └──┘  └──┘  └──┘  └──┘  └──┘  └──┘

V = Validator
D = Domain Service
B = Builder
R = Repository
I = Infrastructure Service
```

## Namespace Organization

```
PromptLab.Core/
├── Builders/              ← Object construction
├── Domain/                ← Entities, Enums, Value Objects
├── DTOs/                  ← Data Transfer Objects
├── Repositories/          ← Data access interfaces
├── Services/Interfaces/   ← Service contracts
└── Validators/            ← Input validation

PromptLab.Infrastructure/
├── Builders/              ← Builder implementations
├── Configuration/         ← Config classes
├── Data/                  ← EF Core, Migrations
├── Repositories/          ← Repository implementations
├── Services/              ← Service implementations
│   └── LlmProviders/      ← External LLM integrations
└── Validators/            ← Validator implementations

PromptLab.Api/
├── Controllers/           ← HTTP endpoints
├── Middleware/            ← Request pipeline
├── Models/                ← API models
└── Services/              ← API-specific services
```

## Key Design Decisions

### 1. Why Scoped vs Singleton?
- **Singleton**: LLM Providers (stateless, expensive to create)
- **Scoped**: Most services (tied to request lifecycle, use DbContext)
- **Transient**: Rarely used (creates new instance each time)

### 2. Why Separate Builders from Services?
- Services contain business logic and state
- Builders are stateless factories
- Clear separation of concerns

### 3. Why Validators in Cross-Cutting?
- Used by multiple layers
- Pure validation logic without business rules
- Reusable across different contexts

### 4. Why Domain Services?
- Business logic that doesn't fit in entities
- Coordinates multiple entities
- Orchestrates domain operations

### 5. Why Application Services?
- Entry points for use cases
- Coordinates across layers
- Transaction boundaries
- Error handling and logging
