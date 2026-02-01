using Microsoft.EntityFrameworkCore;
using PromptLab.Core.Domain.Interfaces;
using PromptLab.Infrastructure.Configuration;
using PromptLab.Infrastructure.Data;
using PromptLab.Infrastructure.Services.LlmProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient
builder.Services.AddHttpClient();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure LLM Provider settings
var geminiConfig = new GoogleGeminiConfig();
builder.Configuration.GetSection(GoogleGeminiConfig.ConfigSectionName).Bind(geminiConfig);
builder.Services.AddSingleton(geminiConfig);

// Register LLM providers
builder.Services.AddSingleton<ILlmProvider, GoogleGeminiProvider>();

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
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();
