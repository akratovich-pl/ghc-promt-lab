# PromptLab – AI Interaction Visualization Tool

> **GitHub Copilot Course – Practical Assignment**  
> Developed as part of the AI Adoption Program  
> 
> **98% of this project's code was implemented using GitHub Copilot**  
> 
> Full development history and AI conversation logs: [docs/ai-conversations/](docs/ai-conversations/README.md)
> 
> Demo Walkthrough - [Visual demonstration](docs/demo-walkthrough.md)  with screenshots and feature descriptions 

---

## 📌 Quick Navigation

- [Overview](#overview)
- [Mission and Purpose](#mission-and-purpose)
- [Key Features Implemented](#key-features-implemented)
- [Current Stage](#current-stage)
- [Planned Improvements / Future Roadmap](#planned-improvements--future-roadmap)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Usage Example](#usage-example)
- [Database Migrations](#database-migrations)
- [Documentation](#documentation)

---

## Overview

**PromptLab** is a full-stack web application designed for training and learning how to effectively interact with Large Language Models (LLMs). It provides a comprehensive environment for prompt engineering, AI response analysis, token consumption tracking, and multi-provider comparison. The application enables users to experiment with different AI providers, analyze their responses, and understand the cost implications of various prompting strategies.

This project was developed during the **GitHub Copilot Course** as a practical assignment to demonstrate modern software development practices with AI-assisted coding.

## Mission and Purpose

The primary mission of PromptLab is to democratize AI learning by providing an accessible platform for:

- **Prompt Engineering Training**: Help developers and AI enthusiasts learn effective prompt writing techniques
- **Cost Analysis**: Provide visibility into token consumption and associated costs across different LLM providers
- **Response Comparison**: Enable side-by-side evaluation of responses from multiple AI providers
- **Conversation Management**: Track and organize prompt-response history for learning and reference
- **Educational Value**: Serve as a hands-on tool for understanding how LLMs process and respond to different types of inputs

This tool addresses the gap between theoretical AI knowledge and practical application by offering real-time feedback on prompt effectiveness, cost efficiency, and response quality.

## Key Features Implemented

### Frontend Features
- **Interactive Prompt Interface**: Clean, intuitive UI for composing and submitting prompts
- **Response Visualization**: Markdown rendering support for formatted AI responses
- **Multi-Provider Selection**: Easy switching between different LLM providers (Google Gemini, OpenAI, Anthropic)
- **Token Metrics Display**: Real-time visualization of input/output tokens and cost estimation
- **Conversation History**: Browse and manage previous prompts and responses
- **Tooltip System**: Comprehensive glossary and help system for AI/ML terminology
- **Responsive Design**: Modern, light-themed UI built with TailwindCSS
- **State Management**: Efficient state handling with Pinia for smooth user experience

### Backend Features
- **RESTful API**: Well-structured .NET 10 Web API with clean architecture
- **Multi-Provider Support**: Abstracted LLM provider interface supporting:
  - Google Gemini
  - Groq models
- **Rate Limiting**: Sliding window rate limiter to prevent API abuse
- **Token Counting**: Accurate token calculation for cost estimation
- **Database Persistence**: Entity Framework Core with SQLite (dev)
- **Request/Response Pipeline**: Modular pipeline pattern for extensible request processing
- **Security**: Environment-based API key management and user secrets support
- **Logging**: Comprehensive structured logging with Serilog
- **Error Handling**: Global exception filter with detailed error responses

### Core Architecture
- **Clean Architecture**: Separation of concerns across Core, Infrastructure, and API layers
- **Domain Entities**: Conversations, Prompts, Responses, and ContextFiles
- **Dependency Injection**: Service-based architecture with Microsoft DI container
- **Configuration Management**: Flexible configuration provider system
- **Extensibility**: Plugin-based design for adding new LLM providers

## Current Stage

**Status**: ✅ **MVP (Minimum Viable Product)**

The application is currently in the MVP stage with core functionality implemented and operational. It is suitable for:
- Educational purposes and training sessions
- Internal demonstrations and proof-of-concept presentations
- Individual developer use for prompt experimentation
- Learning and exploring LLM capabilities

The system is stable for development and testing environments. Production deployment would require additional hardening (see Planned Improvements).

## Planned Improvements / Future Roadmap

### Short-term Goals
- **User Authentication**: Implement user registration and login system
- **Response Comparison View**: Side-by-side comparison of responses from multiple providers
- **Export Functionality**: Export conversations to PDF, Markdown, or JSON formats
- **Advanced Token Analysis**: Detailed breakdown of token types and optimization suggestions
- **Cost Tracking Dashboard**: Aggregate cost analytics across conversations and time periods

### Medium-term Goals
- **Batch Processing**: Submit multiple prompts in parallel for efficiency testing
- **Prompt Templates**: Pre-built templates for common use cases (code review, translation, summarization)
- **Collaborative Features**: Share conversations and prompts with team members
- **Performance Monitoring**: Real-time monitoring of API response times and reliability

### Long-term Vision
- **Prompt Versioning**: Track and compare different versions of the same prompt
- **Analytics and Insights**: ML-powered suggestions for prompt improvements
- **Multi-language Support**: Internationalization for global users

## Tech Stack

### Frontend
- **Framework**: Vue 3 with Composition API
- **Language**: TypeScript
- **Build Tool**: Vite
- **UI Framework**: TailwindCSS
- **State Management**: Pinia
- **Routing**: Vue Router
- **HTTP Client**: Axios
- **Markdown Rendering**: markdown-it
- **Icons**: Heroicons

### Backend
- **Framework**: .NET 10 Web API
- **Language**: C# 15
- **Architecture**: Clean Architecture (Core → Infrastructure → API)
- **Database**: 
  - Development: SQLite
  - Production: PostgreSQL (planned)
- **ORM**: Entity Framework Core 8
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI (planned)
- **Testing**: xUnit, Moq

### LLM Integrations
- **Google Gemini API**: Public LLM models
- **Groq API**: Public LLM models

### DevOps / Tooling
- **Version Control**: Git with GitHub
- **Package Management**: NuGet (backend), npm (frontend)
- **Development Environment**: Visual Studio Code
- **Task Management**: VS Code tasks for build/run/watch
- **Documentation**: Markdown-based docs with AI conversation logs


## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- API keys for Google Gemini and/or Groq

### Quick Start

1. **Clone Repository**
   ```bash
   git clone https://github.com/akratovich-pl/ghc-promt-lab.git
   cd ghc-promt-lab
   ```

2. **Configure User Secrets** (Backend)
   ```bash
   cd src/PromptLab.Api
   dotnet user-secrets set "Providers:Google:ApiKey" "YOUR_GOOGLE_API_KEY"
   dotnet user-secrets set "Providers:Groq:ApiKey" "YOUR_GROQ_API_KEY"
   ```

3. **Run Backend**
   ```bash
   dotnet run --project src/PromptLab.Api
   # Backend starts on http://localhost:5251
   ```

4. **Run Frontend**
   ```bash
   cd client
   npm install
   npm run dev
   # Frontend starts on http://localhost:5173
   ```

5. **Access Application**
   - Open browser: http://localhost:5173
   - Select a model and start prompting!

## Usage Example

### Basic Workflow

1. **Start the Application**
   - Ensure both backend (port 5251) and frontend (port 5173) are running
   - Navigate to `http://localhost:5173` in your browser

2. **Submit Your First Prompt**
   ```
   Navigate to the main prompt interface
   → Select an LLM provider (e.g., Google Gemini)
   → Enter your prompt: "Explain the concept of Clean Architecture in software development"
   → Click "Submit"
   ```

3. **Analyze the Response**
   - View the AI-generated response with markdown formatting
   - Check token metrics: input tokens, output tokens, and estimated cost
   - Review response time and other metadata

4. **Build a Conversation**
   - Continue the conversation by submitting follow-up prompts
   - Access conversation history to review previous exchanges
   - Export the conversation for documentation or sharing

### Example Use Cases

**For Learning Prompt Engineering:**
```markdown
Prompt: "Write a Python function to calculate Fibonacci numbers"
→ Analyze how different models respond to the same technical request
→ Compare token usage across providers
```

**For Cost Optimization:**
```markdown
Test different prompt formulations:
- Verbose: "Can you please write a comprehensive function..."
- Concise: "Write a Python Fibonacci function"
→ Compare token consumption and response quality
```

## Database Migrations

The database has been initialized with the following entities:
- **Conversations**: Container for related prompts and responses
- **Prompts**: User-submitted prompts with metadata
- **Responses**: AI-generated responses with token metrics
- **ContextFiles**: Uploaded files providing additional context


## Documentation

### Project Documentation
- **[Demo Walkthrough](docs/demo-walkthrough.md)** - Visual demonstration with screenshots and feature descriptions
- **[Project Recap](docs/ai-conversations/project-recap.md)** - Complete project journey, milestones, and achievements
- **[Lessons Learned](docs/ai-conversations/lessons-learned.md)** - Practical insights from AI-assisted development
- **[Task Documentation](docs/tasks/README.md)** - Comprehensive task descriptions and implementation phases
- **[AI Conversation Logs](docs/ai-conversations/README.md)** - Complete development history with all AI sessions
- **[User Secrets Migration](docs/development/USER_SECRETS_MIGRATION.md)** - Guide for secure API key management

### Additional Resources
- See [Demo Walkthrough](docs/demo-walkthrough.md) for visual guide with screenshots
- Review [Getting Started](#getting-started) section above for quick setup
- Check [User Secrets Migration](docs/USER_SECRETS_MIGRATION.md) for secure API key configuration

---

**Project Contact**: [Andrey Kratovich]  
**Course**: GitHub Copilot – AI Adoption Program  
**Year**: 2026
