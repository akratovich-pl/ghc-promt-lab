using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PromptLab.Core.Application.Services;
using System.Reflection;
using PromptLab.Core.Configuration;
using PromptLab.Core.Services.Interfaces;
using PromptLab.Core.Interfaces;
using PromptLab.Infrastructure.Configuration;
using PromptLab.Infrastructure.Data;
using PromptLab.Infrastructure.Services;
using PromptLab.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
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
builder.Services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();
// Add LLM Provider Configuration
builder.Services.Configure<LlmProvidersOptions>(
    builder.Configuration.GetSection(LlmProvidersOptions.SectionName));
builder.Services.AddSingleton<ILlmProviderConfig, LlmProviderConfigService>();

// Validate configuration on startup
builder.Services.AddOptions<LlmProvidersOptions>()
    .Bind(builder.Configuration.GetSection(LlmProvidersOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<IPromptExecutionService, PromptExecutionService>();
builder.Services.AddScoped<ILlmProvider, StubLlmProvider>(); // TODO: Replace with actual GoogleGemini provider
builder.Services.AddScoped<IRateLimitService, StubRateLimitService>(); // TODO: Replace with actual rate limit service
builder.Services.AddScoped<IPromptExecutionService, MockPromptExecutionService>();
builder.Services.AddScoped<IProviderService, MockProviderService>();

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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PromptLab API v1");
        options.RoutePrefix = "swagger";
    });
}

// Use ProblemDetails middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

// Add Rate Limiting Middleware (before CORS and Authorization)
app.UseMiddleware<RateLimitMiddleware>();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.Run();

// Make the Program class accessible to integration tests
public partial class Program { }
