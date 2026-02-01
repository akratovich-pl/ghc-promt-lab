namespace PromptLab.Api.Models;

/// <summary>
/// Health check response model
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
