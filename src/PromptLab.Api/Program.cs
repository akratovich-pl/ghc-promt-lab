using Microsoft.EntityFrameworkCore;
using PromptLab.Api.Models;
using PromptLab.Infrastructure.Configuration;
using PromptLab.Infrastructure.Data;
using Serilog;
using PromptLab.Api.Services;
using PromptLab.Infrastructure.Services.LlmProviders;
using Microsoft.OpenApi.Models;
using System.Reflection;
using PromptLab.Core.Configuration;
using PromptLab.Core.Services.Interfaces;
using PromptLab.Infrastructure.Services;
using PromptLab.Api.Middleware;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting PromptLab API");
    
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddSingleton<IStartupTimeService, StartupTimeService>();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
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

    // Register Rate Limit Service
    builder.Services.AddScoped<IRateLimitService, InMemoryRateLimitService>();

    // Add HttpClient
    builder.Services.AddHttpClient();

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddSingleton<IStartupTimeService, StartupTimeService>();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configure LLM Provider settings
    var geminiConfig = new GoogleGeminiConfig();
    builder.Configuration.GetSection(GoogleGeminiConfig.ConfigSectionName).Bind(geminiConfig);
    builder.Services.AddSingleton(geminiConfig);

    // Register HTTP client for LLM providers
    builder.Services.AddHttpClient();

    // Register LLM provider
    builder.Services.AddSingleton<PromptLab.Core.Services.Interfaces.ILlmProvider, GoogleGeminiProvider>();
    
    // Register application services (using mock implementations for now)
    builder.Services.AddScoped<PromptLab.Core.Application.Services.IProviderService, MockProviderService>();
    builder.Services.AddScoped<PromptLab.Core.Application.Services.IPromptExecutionService, MockPromptExecutionService>();
    
    // Register rate limiting service
    builder.Services.AddScoped<IRateLimitService, InMemoryRateLimitService>();

    // Add memory cache for rate limiting
    builder.Services.AddMemoryCache();

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

    var app = builder.Build();

    // Add request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
        };
    });

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("PromptLab API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the Program class accessible to integration tests
public partial class Program { }
