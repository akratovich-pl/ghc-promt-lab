using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSwaggerGen();

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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add Rate Limiting Middleware (before CORS and Authorization)
app.UseMiddleware<RateLimitMiddleware>();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();

// Make the Program class accessible to integration tests
public partial class Program { }
