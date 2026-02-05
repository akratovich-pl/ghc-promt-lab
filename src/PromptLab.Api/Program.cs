using Serilog;
using PromptLab.Api;
using PromptLab.Api.Extensions;
using DotNetEnv;
using PromptLab.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// Load environment variables from .env file if it exists
// Try multiple potential paths for .env file
var currentDir = Directory.GetCurrentDirectory();
var baseDir = AppContext.BaseDirectory;

var envPaths = new[]
{
    Path.Combine(currentDir, ".env"),                                           // Current directory
    Path.Combine(currentDir, "..", "..", ".env"),                              // Up 2 levels from project
    Path.Combine(currentDir, "..", "..", "..", "..", "..", ".env"),           // From bin/Debug/net10.0
    Path.Combine(baseDir, ".env"),                                            // Base directory
    Path.Combine(baseDir, "..", "..", "..", "..", "..", ".env"),             // From base up to solution root
    Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", ".env")) // Extra level for safety
};

var envLoaded = false;
var envPath = string.Empty;
foreach (var path in envPaths)
{
    var fullPath = Path.GetFullPath(path);
    if (File.Exists(fullPath))
    {
        Env.Load(fullPath);
        envLoaded = true;
        envPath = fullPath;
        Console.WriteLine($"✓ Loaded .env file from: {fullPath}");
        Console.WriteLine($"  Google API Key: {Environment.GetEnvironmentVariable("Providers__Google__ApiKey")?[..20]}...");
        Console.WriteLine($"  Groq API Key: {Environment.GetEnvironmentVariable("Providers__Groq__ApiKey")?[..20]}...");
        break;
    }
}

// If .env not found, that's okay - we'll use environment variables directly
if (!envLoaded)
{
    Console.WriteLine("⚠ No .env file found. Using system environment variables.");
    // In test/production environments, environment variables may be set directly
}

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

    // Configure all services
    Registration.ConfigureServices(builder);

    var app = builder.Build();

    // Ensure database is created and migrations are applied
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
            Log.Information("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the database");
            throw;
        }
    }

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
    
    // Apply rate limiting middleware before authorization
    app.UseRateLimiting();
    
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
