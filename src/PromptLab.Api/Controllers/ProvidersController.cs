using Microsoft.AspNetCore.Mvc;
using PromptLab.Api.Models;
using PromptLab.Core.Application.Services;

namespace PromptLab.Api.Controllers;

/// <summary>
/// API endpoints for AI provider information and management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;
    private readonly ILogger<ProvidersController> _logger;

    public ProvidersController(
        IProviderService providerService,
        ILogger<ProvidersController> logger)
    {
        _providerService = providerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available AI providers
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available providers with their supported models</returns>
    /// <response code="200">Providers retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProviderInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ProviderInfoResponse>>> GetProviders(
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting all providers");

            var providers = await _providerService.GetProvidersAsync(cancellationToken);

            var responses = providers.Select(p => new ProviderInfoResponse
            {
                Name = p.Name,
                IsAvailable = p.IsAvailable,
                SupportedModels = p.SupportedModels
            }).ToList();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting providers");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An error occurred while processing your request"
            });
        }
    }

    /// <summary>
    /// Get health status of a specific provider
    /// </summary>
    /// <param name="name">Provider name (e.g., 'Google', 'OpenAI', 'Anthropic')</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Provider health status</returns>
    /// <response code="200">Status retrieved successfully</response>
    /// <response code="404">Provider not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{name}/status")]
    [ProducesResponseType(typeof(ProviderStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProviderStatusResponse>> GetProviderStatus(
        string name,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting status for provider: {ProviderName}", name);

            var status = await _providerService.GetProviderStatusAsync(name, cancellationToken);

            var response = new ProviderStatusResponse
            {
                Name = status.Name,
                IsHealthy = status.IsHealthy,
                ErrorMessage = status.ErrorMessage,
                LastChecked = status.LastChecked
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Provider not found: {ProviderName}", name);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Provider Not Found",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider status: {ProviderName}", name);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An error occurred while processing your request"
            });
        }
    }

    /// <summary>
    /// Get all available models for a specific provider
    /// </summary>
    /// <param name="name">Provider name (e.g., 'Google', 'OpenAI', 'Anthropic')</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of models with pricing information</returns>
    /// <response code="200">Models retrieved successfully</response>
    /// <response code="404">Provider not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{name}/models")]
    [ProducesResponseType(typeof(List<ModelInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ModelInfoResponse>>> GetProviderModels(
        string name,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting models for provider: {ProviderName}", name);

            var models = await _providerService.GetProviderModelsAsync(name, cancellationToken);

            var responses = models.Select(m => new ModelInfoResponse
            {
                Name = m.Name,
                DisplayName = m.DisplayName,
                Provider = m.Provider.ToString(),
                MaxTokens = m.MaxTokens,
                InputCostPer1kTokens = m.InputCostPer1kTokens,
                OutputCostPer1kTokens = m.OutputCostPer1kTokens
            }).ToList();

            return Ok(responses);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Provider not found: {ProviderName}", name);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Provider Not Found",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider models: {ProviderName}", name);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An error occurred while processing your request"
            });
        }
    }
}
