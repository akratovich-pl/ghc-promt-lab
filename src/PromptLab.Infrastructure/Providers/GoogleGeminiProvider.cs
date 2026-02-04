using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using PromptLab.Core.Domain.Enums;
using PromptLab.Core.DTOs;
using PromptLab.Core.Providers;
using PromptLab.Infrastructure.Configuration;
using PromptLab.Infrastructure.Providers.Models;

namespace PromptLab.Infrastructure.Providers;

/// <summary>
/// Google Gemini API provider implementation
/// </summary>
public class GoogleGeminiProvider : ILlmProvider
{
    private readonly GoogleGeminiConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleGeminiProvider> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly JsonSerializerOptions _jsonOptions;

    public string ProviderName => "Google Gemini";
    public AiProvider Provider => AiProvider.Google;

    public GoogleGeminiProvider(
        GoogleGeminiConfig config,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleGeminiProvider> logger)
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

            var geminiRequest = BuildGeminiRequest(request);
            var httpClient = CreateHttpClient();
            var url = $"{_config.BaseUrl}/{_config.ApiVersion}/models/{model}:generateContent?key={_config.ApiKey}";

            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var jsonContent = JsonSerializer.Serialize(geminiRequest, _jsonOptions);
                _logger.LogDebug("Request payload: {Payload}", jsonContent);

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                return await httpClient.PostAsync(url, content, cancellationToken);
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

            var geminiResponse = JsonSerializer.Deserialize<GeminiGenerateResponse>(responseJson, _jsonOptions);

            if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Count == 0)
            {
                _logger.LogWarning("No candidates in response");
                return new LlmResponse
                {
                    Content = string.Empty,
                    Model = model,
                    Success = false,
                    ErrorMessage = "No content generated",
                    LatencyMs = stopwatch.ElapsedMilliseconds
                };
            }

            var candidate = geminiResponse.Candidates[0];
            var content = candidate.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
            var usage = geminiResponse.UsageMetadata;

            var promptTokens = usage?.PromptTokenCount ?? 0;
            var completionTokens = usage?.CandidatesTokenCount ?? 0;
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
                FinishReason = candidate.FinishReason,
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

    public async Task<int> EstimateTokensAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));

        try
        {
            _logger.LogInformation("Estimating tokens for prompt");

            var request = new GeminiCountTokensRequest
            {
                Contents = new List<GeminiContent>
                {
                    new GeminiContent
                    {
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart { Text = prompt }
                        }
                    }
                }
            };

            var httpClient = CreateHttpClient();
            var url = $"{_config.BaseUrl}/{_config.ApiVersion}/models/{_config.Model}:countTokens?key={_config.ApiKey}";

            var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Token estimation failed with status {StatusCode}: {Error}",
                    response.StatusCode,
                    errorContent);
                return 0;
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var countResponse = JsonSerializer.Deserialize<GeminiCountTokensResponse>(responseJson, _jsonOptions);

            var tokenCount = countResponse?.TotalTokens ?? 0;
            _logger.LogInformation("Estimated {TokenCount} tokens", tokenCount);

            return tokenCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating tokens");
            return 0;
        }
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

            // Quick availability check with a minimal token estimation request
            var testPrompt = "test";
            var tokenCount = await EstimateTokensAsync(testPrompt);
            
            var isAvailable = tokenCount > 0;
            _logger.LogInformation("Provider availability: {IsAvailable}", isAvailable);
            
            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking provider availability");
            return false;
        }
    }

    private GeminiGenerateRequest BuildGeminiRequest(LlmRequest request)
    {
        var contents = new List<GeminiContent>();

        // Add system message as a separate content if provided
        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            contents.Add(new GeminiContent
            {
                Parts = new List<GeminiPart>
                {
                    new GeminiPart { Text = request.SystemMessage }
                },
                Role = "user"
            });
        }

        // Add main prompt
        contents.Add(new GeminiContent
        {
            Parts = new List<GeminiPart>
            {
                new GeminiPart { Text = request.Prompt }
            },
            Role = "user"
        });

        var geminiRequest = new GeminiGenerateRequest
        {
            Contents = contents
        };

        // Add generation config if any parameters are specified
        if (request.Temperature.HasValue || request.MaxTokens.HasValue)
        {
            geminiRequest.GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = request.Temperature.HasValue ? (double?)request.Temperature.Value : null,
                MaxOutputTokens = request.MaxTokens
            };
        }

        return geminiRequest;
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
