# Migration to User Secrets

## Overview

We've migrated from `.env` files to **User Secrets** for local development and **Environment Variables** for production. This provides better security and follows .NET best practices.

## What Changed

### ✅ Removed
- `.env` file support
- `DotNetEnv` package dependency
- Environment variable loading logic in Program.cs

### ✅ Added
- User Secrets configuration (Development)
- `.env.example` with instructions
- This migration guide

## Setup for Development

### 1. Initialize User Secrets (Already Done)

The project already has User Secrets initialized with ID: `fb7838fa-1c4a-40f1-8091-27ae5311ff09`

### 2. Set Your API Keys

Run these commands from the repository root:

```bash
# Set Google API Key
dotnet user-secrets set "Providers:Google:ApiKey" "YOUR_GOOGLE_API_KEY" --project src/PromptLab.Api

# Set Groq API Key
dotnet user-secrets set "Providers:Groq:ApiKey" "YOUR_GROQ_API_KEY" --project src/PromptLab.Api
```

### 3. Verify Your Secrets

```bash
# List all secrets
dotnet user-secrets list --project src/PromptLab.Api
```

### 4. Get API Keys

- **Google Gemini**: https://makersuite.google.com/app/apikey
- **Groq**: https://console.groq.com/keys

## Where Are Secrets Stored?

User Secrets are stored in your user profile directory, **outside the project**:

- **Windows**: `%APPDATA%\Microsoft\UserSecrets\fb7838fa-1c4a-40f1-8091-27ae5311ff09\secrets.json`
- **Linux/macOS**: `~/.microsoft/usersecrets/fb7838fa-1c4a-40f1-8091-27ae5311ff09/secrets.json`

This means:
- ✅ Secrets cannot be accidentally committed to Git
- ✅ Each developer has their own API keys
- ✅ No risk of exposing secrets in the repository

## Production Deployment

### GitHub Actions Secrets

1. Go to your repository on GitHub
2. Navigate to: **Settings → Secrets and variables → Actions**
3. Add the following secrets:

```
Name: PROVIDERS__GOOGLE__APIKEY
Value: <your-google-api-key>

Name: PROVIDERS__GROQ__APIKEY
Value: <your-groq-api-key>
```

### Using in GitHub Actions

```yaml
- name: Run Application
  env:
    Providers__Google__ApiKey: ${{ secrets.PROVIDERS__GOOGLE__APIKEY }}
    Providers__Groq__ApiKey: ${{ secrets.PROVIDERS__GROQ__APIKEY }}
  run: dotnet run --project src/PromptLab.Api
```

### Other Hosting Platforms

Set environment variables with double underscores:

```bash
Providers__Google__ApiKey=your-google-api-key
Providers__Groq__ApiKey=your-groq-api-key
```

## Troubleshooting

### Secrets not loading?

1. Verify secrets are set:
   ```bash
   dotnet user-secrets list --project src/PromptLab.Api
   ```

2. Check you're in Development environment:
   - User Secrets only load when `ASPNETCORE_ENVIRONMENT=Development`

3. Rebuild the project:
   ```bash
   dotnet build
   ```

### Need to remove a secret?

```bash
dotnet user-secrets remove "Providers:Google:ApiKey" --project src/PromptLab.Api
```

### Need to clear all secrets?

```bash
dotnet user-secrets clear --project src/PromptLab.Api
```

## Benefits

✅ **Secure**: Secrets stored outside project, OS-protected  
✅ **Team-friendly**: Each developer has their own keys  
✅ **Standard**: Built-in .NET feature, no extra packages  
✅ **CI/CD Ready**: Works seamlessly with GitHub Actions  
✅ **Flexible**: Easy to add new secrets as needed

## Migration Notes

If you previously had a `.env` file:

1. Your keys have already been migrated to User Secrets (if this was done for you)
2. The `.env` file is ignored by Git and can be deleted
3. Keep `.env.example` as a reference for team members

## Questions?

See official documentation:
- [Safe storage of app secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
