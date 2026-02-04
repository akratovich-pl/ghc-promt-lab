using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PromptLab.Api.Services;
using PromptLab.Core.Builders;
using PromptLab.Core.Repositories;
using PromptLab.Core.Services;
using PromptLab.Core.Providers;
using PromptLab.Core.RateLimiter;
using PromptLab.Core.Configuration;
using PromptLab.Core.Validators;
using PromptLab.Infrastructure.Builders;
using PromptLab.Infrastructure.Configuration;
using PromptLab.Infrastructure.Data;
using PromptLab.Infrastructure.Repositories;
using PromptLab.Infrastructure.Services;
using PromptLab.Infrastructure.Providers;
using PromptLab.Infrastructure.RateLimiter;
using PromptLab.Infrastructure.Validators;
using System.Reflection;

namespace PromptLab.Api;

/// <summary>
/// Service registration configuration
/// </summary>
public static class Registration
{
    /// <summary>
    /// Configures all application services
    /// </summary>
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add core services
        builder.Services.AddSingleton<IStartupTimeService, StartupTimeService>();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Configure Swagger
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "PromptLab API",
                Description = "API for LLM prompt execution and provider management",
                Contact = new OpenApiContact
                {
                    Name = "PromptLab",
                    Url = new Uri("https://github.com/akratovich-pl/ghc-promt-lab")
                }
            });

            // Include XML comments
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        // Add ProblemDetails support
        builder.Services.AddProblemDetails();

        // Add Memory Cache for rate limiting
        builder.Services.AddMemoryCache();

        // Configure Rate Limiting Options
        builder.Services.Configure<RateLimitingOptions>(
            builder.Configuration.GetSection(RateLimitingOptions.SectionName));

        // Add HttpClient
        builder.Services.AddHttpClient();

        // Add DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Configure LLM Provider settings from unified Providers configuration
        var geminiConfig = new GoogleGeminiConfig();
        var geminiSection = builder.Configuration.GetSection("Providers:Google");
        geminiSection.Bind(geminiConfig);
        geminiConfig.Model = geminiSection.GetValue<string>("DefaultModel") ?? "gemini-pro";
        // Override API key from environment variable
        geminiConfig.ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? string.Empty;
        // Set default cost from the first model in the Models array (or use configured values as fallback)
        var geminiModels = geminiSection.GetSection("Models").Get<List<Core.Configuration.ModelSettings>>();
        if (geminiModels != null && geminiModels.Count > 0)
        {
            var defaultModel = geminiModels[0]; // Use first model as default for cost calculation
            geminiConfig.InputTokenCostPer1K = defaultModel.InputCostPer1kTokens;
            geminiConfig.OutputTokenCostPer1K = defaultModel.OutputCostPer1kTokens;
        }
        builder.Services.AddSingleton(geminiConfig);

        var groqConfig = new GroqConfig();
        var groqSection = builder.Configuration.GetSection("Providers:Groq");
        groqSection.Bind(groqConfig);
        groqConfig.Model = groqSection.GetValue<string>("DefaultModel") ?? "llama-3.3-70b-versatile";
        // Override API key from environment variable
        groqConfig.ApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? string.Empty;
        // Set default cost from the first model in the Models array (or use configured values as fallback)
        var groqModels = groqSection.GetSection("Models").Get<List<Core.Configuration.ModelSettings>>();
        if (groqModels != null && groqModels.Count > 0)
        {
            var defaultModel = groqModels[0]; // Use first model as default for cost calculation
            groqConfig.InputTokenCostPer1K = defaultModel.InputCostPer1kTokens;
            groqConfig.OutputTokenCostPer1K = defaultModel.OutputCostPer1kTokens;
        }
        builder.Services.AddSingleton(groqConfig);

        // Register LLM providers (Infrastructure)
        builder.Services.AddSingleton<ILlmProvider, GoogleGeminiProvider>();
        builder.Services.AddSingleton<ILlmProvider, GroqProvider>();

        // Register application services (Orchestrators)
        builder.Services.AddScoped<IProviderService, ProviderService>();
        builder.Services.AddScoped<IPromptExecutionService, PromptExecutionService>();

        // Register pipeline services (Request preparation)
        builder.Services.AddScoped<IRequestPreparationService, RequestPreparationService>();

        // Register domain services (Business logic)
        builder.Services.AddScoped<IConversationHistoryService, ConversationHistoryService>();
        
        // Register rate limiting (Used by middleware)
        builder.Services.AddScoped<IRateLimiter, InMemoryRateLimiter>();

        // Register repositories (Data access)
        builder.Services.AddScoped<IPromptRepository, PromptRepository>();

        // Register validators (Cross-cutting concerns)
        builder.Services.AddScoped<IPromptValidator, PromptValidator>();

        // Register builders (Helpers/Factories)
        builder.Services.AddScoped<ILlmRequestBuilder, LlmRequestBuilder>();
        builder.Services.AddScoped<IPromptEnricher, PromptEnricher>();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }
}
