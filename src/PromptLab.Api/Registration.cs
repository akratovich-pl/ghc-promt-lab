using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PromptLab.Api.Services;
using PromptLab.Core.Services.Interfaces;
using PromptLab.Core.Configuration;
using PromptLab.Infrastructure.Configuration;
using PromptLab.Infrastructure.Data;
using PromptLab.Infrastructure.Services;
using PromptLab.Infrastructure.Services.LlmProviders;
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

        // Configure LLM Provider settings
        var geminiConfig = new GoogleGeminiConfig();
        builder.Configuration.GetSection(GoogleGeminiConfig.ConfigSectionName).Bind(geminiConfig);
        // Override API key from environment variable
        geminiConfig.ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? string.Empty;
        builder.Services.AddSingleton(geminiConfig);

        var groqConfig = new GroqConfig();
        builder.Configuration.GetSection(GroqConfig.ConfigSectionName).Bind(groqConfig);
        // Override API key from environment variable
        groqConfig.ApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? string.Empty;
        builder.Services.AddSingleton(groqConfig);

        // Register LLM providers
        builder.Services.AddSingleton<ILlmProvider, GoogleGeminiProvider>();
        builder.Services.AddSingleton<ILlmProvider, GroqProvider>();

        // Register application services
        builder.Services.AddScoped<IProviderService, ConfigurationProviderService>();
        builder.Services.AddScoped<IPromptExecutionService, PromptExecutionService>();

        // Register rate limiting service
        builder.Services.AddScoped<IRateLimitService, InMemoryRateLimitService>();

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
