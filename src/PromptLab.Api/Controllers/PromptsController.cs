using Microsoft.AspNetCore.Mvc;
using PromptLab.Api.Extensions;
using PromptLab.Api.Models;
using PromptLab.Core.Services;

namespace PromptLab.Api.Controllers;

/// <summary>
/// API endpoints for prompt execution and management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PromptsController : ControllerBase
{
    private readonly IPromptExecutionService _promptService;
    private readonly ILogger<PromptsController> _logger;

    public PromptsController(
        IPromptExecutionService promptService,
        ILogger<PromptsController> logger)
    {
        _promptService = promptService;
        _logger = logger;
    }

    /// <summary>
    /// Execute a prompt and get AI response
    /// </summary>
    /// <param name="request">The prompt execution request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution result with response content, tokens, and cost</returns>
    /// <response code="200">Prompt executed successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="429">Rate limit exceeded</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(ExecutePromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExecutePromptResponse>> ExecutePrompt(
        [FromBody] ExecutePromptRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing prompt with provider: {Provider}, model: {Model}", 
            request.Provider, request.Model ?? "default");

        var result = await _promptService.ExecutePromptAsync(
            request.Provider,
            request.Prompt,
            request.SystemPrompt,
            request.ConversationId,
            request.ContextFileIds,
            request.Model,
            request.MaxTokens,
            request.Temperature,
            cancellationToken);

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Estimate tokens and cost for a prompt without executing it
    /// </summary>
    /// <param name="request">The token estimation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Estimated token count and cost</returns>
    /// <response code="200">Tokens estimated successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("estimate")]
    [ProducesResponseType(typeof(EstimateTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EstimateTokensResponse>> EstimateTokens(
        [FromBody] EstimateTokensRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Estimating tokens for prompt with model: {Model}", request.Model ?? "default");

        var estimate = await _promptService.EstimateTokensAsync(
            request.Prompt,
            request.Model,
            cancellationToken);

        return Ok(estimate.ToResponse());
    }

    /// <summary>
    /// Get a prompt by its ID with response details
    /// </summary>
    /// <param name="id">The prompt ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed prompt information including responses</returns>
    /// <response code="200">Prompt found</response>
    /// <response code="404">Prompt not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromptDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PromptDetailResponse>> GetPromptById(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting prompt by ID: {PromptId}", id);

        var result = await _promptService.GetPromptByIdAsync(id, cancellationToken);

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Prompt with ID {id} not found"
            });
        }

        // Note: This is a simplified mapping. In a real implementation,
        // we would fetch the full prompt details from the database
        return Ok(result.ToDetailResponse(Guid.Empty));
    }

    /// <summary>
    /// Get all prompts in a conversation
    /// </summary>
    /// <param name="conversationId">The conversation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of prompts with their responses</returns>
    /// <response code="200">Prompts retrieved successfully</response>
    /// <response code="404">Conversation not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("conversation/{conversationId:guid}")]
    [ProducesResponseType(typeof(List<PromptDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<PromptDetailResponse>>> GetPromptsByConversation(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting prompts for conversation: {ConversationId}", conversationId);

        var results = await _promptService.GetPromptsByConversationIdAsync(
            conversationId,
            cancellationToken);

        if (results == null || results.Count == 0)
        {
            return Ok(new List<PromptDetailResponse>());
        }

        return Ok(results.ToDetailResponses(conversationId));
    }
}
