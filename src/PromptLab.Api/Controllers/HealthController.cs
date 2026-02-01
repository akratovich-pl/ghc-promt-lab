using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromptLab.Infrastructure.Data;
using PromptLab.Core.Logging;
using PromptLab.Api.Services;

namespace PromptLab.Api.Controllers;

/// <summary>
/// Controller for application health checks and status monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;
    private readonly IStartupTimeService _startupTimeService;

    public HealthController(
        ApplicationDbContext dbContext, 
        ILogger<HealthController> logger,
        IStartupTimeService startupTimeService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _startupTimeService = startupTimeService;
    }

    /// <summary>
    /// Comprehensive health check endpoint
    /// </summary>
    /// <returns>Health status with detailed component information</returns>
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        _logger.LogInformation(LogEvents.HealthCheckStarted, "Health check requested");

        var healthStatus = new
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Uptime = DateTime.UtcNow - _startupTimeService.StartupTime,
            Components = new
            {
                Application = await CheckApplicationHealth(),
                Database = await CheckDatabaseHealth(),
                LlmProvider = CheckLlmProviderHealth(),
                Cache = CheckCacheHealth()
            },
            LastSuccessfulCheck = _startupTimeService.LastSuccessfulHealthCheck
        };

        _startupTimeService.LastSuccessfulHealthCheck = DateTime.UtcNow;
        _logger.LogInformation(LogEvents.HealthCheckCompleted, "Health check completed successfully");

        return Ok(healthStatus);
    }

    /// <summary>
    /// Simple liveness probe endpoint
    /// </summary>
    [HttpGet("alive")]
    public IActionResult Alive()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe endpoint
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        var dbHealthy = await CheckDatabaseHealth();
        
        if (!dbHealthy.Healthy)
        {
            return StatusCode(503, new { status = "not ready", reason = "database unavailable" });
        }

        return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
    }

    private async Task<ComponentHealth> CheckApplicationHealth()
    {
        try
        {
            return new ComponentHealth
            {
                Name = "Application",
                Healthy = true,
                Status = "up",
                Details = new Dictionary<string, object>
                {
                    { "startupTime", _startupTimeService.StartupTime },
                    { "uptime", DateTime.UtcNow - _startupTimeService.StartupTime },
                    { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(LogEvents.HealthCheckFailed, ex, "Application health check failed");
            return new ComponentHealth { Name = "Application", Healthy = false, Status = "error", Error = ex.Message };
        }
    }

    private async Task<ComponentHealth> CheckDatabaseHealth()
    {
        try
        {
            _logger.LogDebug(LogEvents.DatabaseConnectionOpened, "Checking database connectivity");
            
            // Simple connectivity check
            var canConnect = await _dbContext.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                _logger.LogWarning(LogEvents.DatabaseConnectionFailed, "Database connectivity check failed");
                return new ComponentHealth { Name = "Database", Healthy = false, Status = "unavailable" };
            }

            return new ComponentHealth
            {
                Name = "Database",
                Healthy = true,
                Status = "connected",
                Details = new Dictionary<string, object>
                {
                    { "provider", "SQLite" },
                    { "connectionString", _dbContext.Database.GetConnectionString()?.Split(';')[0] ?? "unknown" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(LogEvents.DatabaseConnectionFailed, ex, "Database health check failed");
            return new ComponentHealth { Name = "Database", Healthy = false, Status = "error", Error = ex.Message };
        }
    }

    private ComponentHealth CheckLlmProviderHealth()
    {
        try
        {
            // Placeholder for when LLM providers are implemented
            // For now, return a "not configured" status
            return new ComponentHealth
            {
                Name = "LLM Provider",
                Healthy = true,
                Status = "not configured",
                Details = new Dictionary<string, object>
                {
                    { "message", "LLM provider integration pending" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(LogEvents.HealthCheckFailed, ex, "LLM provider health check failed");
            return new ComponentHealth { Name = "LLM Provider", Healthy = false, Status = "error", Error = ex.Message };
        }
    }

    private ComponentHealth CheckCacheHealth()
    {
        try
        {
            // Placeholder for when caching is implemented
            return new ComponentHealth
            {
                Name = "Cache",
                Healthy = true,
                Status = "not configured",
                Details = new Dictionary<string, object>
                {
                    { "message", "Cache not yet implemented" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(LogEvents.CacheError, ex, "Cache health check failed");
            return new ComponentHealth { Name = "Cache", Healthy = false, Status = "error", Error = ex.Message };
        }
    }
}

public class ComponentHealth
{
    public string Name { get; set; } = string.Empty;
    public bool Healthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}
