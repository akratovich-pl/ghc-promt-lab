namespace PromptLab.Api.Services;

/// <summary>
/// Service to track application startup time
/// </summary>
public interface IStartupTimeService
{
    DateTime StartupTime { get; }
    DateTime? LastSuccessfulHealthCheck { get; set; }
}

public class StartupTimeService : IStartupTimeService
{
    public DateTime StartupTime { get; }
    public DateTime? LastSuccessfulHealthCheck { get; set; }

    public StartupTimeService()
    {
        StartupTime = DateTime.UtcNow;
    }
}
