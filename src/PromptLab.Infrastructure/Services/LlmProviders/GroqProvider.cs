using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Services.Interfaces;
using PromptLab.Infrastructure.Configuration;
using PromptLab.Infrastructure.Services.LlmProviders.Models;

namespace PromptLab.Infrastructure.Services.LlmProviders;

/// <summary>
/// Groq API provider implementation
/// </summary>
public class GroqProvider : ILlmProvider
{
    private readonly GroqConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GroqProvider> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly JsonSerializerOptions _jsonOptions;

    public string ProviderName => "Groq";
    public AiProvider Provider => AiProvider.Groq;

    public GroqProvider(
        GroqConfig config,
        IHttpClientFactory httpClientFactory,
        ILogger<GroqProvider> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests || 
                          r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                          r.StatusCode >= HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(
                _config.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Request failed with {StatusCode}. Waiting {Delay}s before retry #{RetryCount}",
                        outcome.Result?.StatusCode ?? (HttpStatusCode)0,
                        timespan.TotalSeconds,
                        retryCount);
                });
    }

    public async Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var stopwatch = Stopwatch.StartNew();
        var model = request.Model ?? _config.Model;

        try
        {
            _logger.LogInformation("Generating content with model {Model}", model);

            var groqRequest = BuildGroqRequest(request);
            var httpClient = CreateHttpClient();
            var url = $"{_config.BaseUrl}/{_config.ApiVersion}/chat/completions";

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var jsonContent = JsonSerializer.Serialize(groqRequest, _jsonOptions);
                _logger.LogDebug("Request payload: {Payload}", jsonContent);

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                requestMessage.Headers.Add("Authorization", $"Bearer {_config.ApiKey}");
                
                return await httpClient.SendAsync(requestMessage, cancellationToken);
            });

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "API request failed with status {StatusCode}: {Error}",
                    response.StatusCode,
                    errorContent);

                return new LlmResponse
                {
                    Content = string.Empty,
                    Model = model,
                    Success = false,
                    ErrorMessage = $"API returned {response.StatusCode}: {errorContent}",
                    LatencyMs = stopwatch.ElapsedMilliseconds
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Response payload: {Payload}", responseJson);

            var groqResponse = JsonSerializer.Deserialize<GroqChatResponse>(responseJson, _jsonOptions);

            if (groqResponse?.Choices == null || groqResponse.Choices.Count == 0)
            {
                _logger.LogWarning("No choices in response");
                return new LlmResponse
                {
                    Content = string.Empty,
                    Model = model,
                    Success = false,
                    ErrorMessage = "No content generated",
                    LatencyMs = stopwatch.ElapsedMilliseconds
                };
            }

            var choice = groqResponse.Choices[0];
            var content = choice.Message?.Content ?? string.Empty;
            var usage = groqResponse.Usage;

            var promptTokens = usage?.PromptTokens ?? 0;
            var completionTokens = usage?.CompletionTokens ?? 0;
            var cost = CalculateCost(promptTokens, completionTokens);

            _logger.LogInformation(
                "Content generated successfully. Tokens: {PromptTokens}/{CompletionTokens}, Cost: ${Cost:F6}, Latency: {LatencyMs}ms",
                promptTokens,
                completionTokens,
                cost,
                stopwatch.ElapsedMilliseconds);

            return new LlmResponse
            {
                Content = content,
                Model = model,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                Cost = cost,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                FinishReason = choice.FinishReason,
                Success = true
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Network error during API request");

            return new LlmResponse
            {
                Content = string.Empty,
                Model = model,
                Success = false,
                ErrorMessage = $"Network error: {ex.Message}",
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "JSON parsing error");

            return new LlmResponse
            {
                Content = string.Empty,
                Model = model,
                Success = false,
                ErrorMessage = $"Failed to parse response: {ex.Message}",
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unexpected error during content generation");

            return new LlmResponse
            {
                Content = string.Empty,
                Model = model,
                Success = false,
                ErrorMessage = $"Unexpected error: {ex.Message}",
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public Task<int> EstimateTokensAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));

        // Groq doesn't have a token counting endpoint, so we use a simple estimation
        // Roughly 4 characters per token
        var estimatedTokens = prompt.Length / 4;
        _logger.LogInformation("Estimated {TokenCount} tokens for prompt length {Length}", estimatedTokens, prompt.Length);
        return Task.FromResult(estimatedTokens);
    }

    public async Task<bool> IsAvailable()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_config.ApiKey))
            {
                _logger.LogWarning("API key not configured");
                return false;
            }

            // Simple availability check with a minimal request
            var httpClient = CreateHttpClient();
            var url = $"{_config.BaseUrl}/{_config.ApiVersion}/models";
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("Authorization", $"Bearer {_config.ApiKey}");
            
            var response = await httpClient.SendAsync(requestMessage);
            
            var isAvailable = response.IsSuccessStatusCode;
            _logger.LogInformation("Provider availability: {IsAvailable}", isAvailable);
            
            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking provider availability");
            return false;
        }
    }

    private GroqChatRequest BuildGroqRequest(LlmRequest request)
    {
        var messages = new List<GroqMessage>();

        // Add system message if provided
        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            messages.Add(new GroqMessage
            {
                Role = "system",
                Content = request.SystemMessage
            });
        }

        // Add user prompt
        messages.Add(new GroqMessage
        {
            Role = "user",
            Content = request.Prompt
        });

        var groqRequest = new GroqChatRequest
        {
            Model = request.Model ?? _config.Model,
            Messages = messages,
            Temperature = request.Temperature.HasValue ? (double?)request.Temperature.Value : null,
            MaxTokens = request.MaxTokens
        };

        return groqRequest;
    }

    private HttpClient CreateHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        return httpClient;
    }

    private decimal CalculateCost(int promptTokens, int completionTokens)
    {
        var promptCost = (promptTokens / 1000m) * _config.InputTokenCostPer1K;
        var completionCost = (completionTokens / 1000m) * _config.OutputTokenCostPer1K;
        return promptCost + completionCost;
    }
}
