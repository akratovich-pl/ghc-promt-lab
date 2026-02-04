using Microsoft.Extensions.Logging;
using PromptLab.Core.Builders;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services;
using PromptLab.Core.Validators;

namespace PromptLab.Infrastructure.Services;

/// <summary>
/// Implementation of request preparation service that orchestrates validation, 
/// enrichment, history loading, and request building
/// </summary>
public class RequestPreparationService : IRequestPreparationService
{
    private readonly IPromptValidator _promptValidator;
    private readonly IConversationHistoryService _conversationHistoryService;
    private readonly IPromptEnricher _promptEnricher;
    private readonly ILlmRequestBuilder _llmRequestBuilder;
    private readonly ILogger<RequestPreparationService> _logger;

    public RequestPreparationService(
        IPromptValidator promptValidator,
        IConversationHistoryService conversationHistoryService,
        IPromptEnricher promptEnricher,
        ILlmRequestBuilder llmRequestBuilder,
        ILogger<RequestPreparationService> logger)
    {
        _promptValidator = promptValidator ?? throw new ArgumentNullException(nameof(promptValidator));
        _conversationHistoryService = conversationHistoryService ?? throw new ArgumentNullException(nameof(conversationHistoryService));
        _promptEnricher = promptEnricher ?? throw new ArgumentNullException(nameof(promptEnricher));
        _llmRequestBuilder = llmRequestBuilder ?? throw new ArgumentNullException(nameof(llmRequestBuilder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PreparedPromptRequest> PrepareAsync(
        string prompt,
        string? systemPrompt,
        Guid? conversationId,
        List<Guid>? contextFileIds,
        string? model,
        int? maxTokens,
        double? temperature,
        string userId,
        CancellationToken cancellationToken)
    {
        var requestModel = model ?? "gemini-1.5-flash";

        // 1. Validate input
        _promptValidator.ValidatePromptRequest(prompt, contextFileIds);

        // 2. Load conversation history if provided
        var conversationHistory = conversationId.HasValue
            ? await _conversationHistoryService.LoadConversationHistoryAsync(conversationId.Value, cancellationToken)
            : new List<ConversationMessage>();

        _logger.LogInformation("Loaded {Count} previous messages for request preparation",
            conversationHistory.Count);

        // 3. Enrich prompt with context files
        var (enrichedPrompt, contextFileId) = await _promptEnricher.EnrichPromptWithContextAsync(
            prompt, contextFileIds, cancellationToken);

        // 4. Build LLM request
        var llmRequest = _llmRequestBuilder.BuildRequest(
            enrichedPrompt,
            systemPrompt,
            requestModel,
            conversationHistory,
            maxTokens,
            temperature);

        return new PreparedPromptRequest
        {
            LlmRequest = llmRequest,
            ContextFileId = contextFileId,
            ConversationHistory = conversationHistory,
            UserId = userId,
            Model = requestModel
        };
    }
}
