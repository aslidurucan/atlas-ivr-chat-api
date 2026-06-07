using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AtlasIvrChat.Domain.Interfaces;
using AtlasIvrChat.Domain.Models;
using AtlasIvrChat.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AtlasIvrChat.Infrastructure.Services;

public class GroqAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly double _temperature;
    private readonly ILogger<GroqAiService> _logger;
    private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GroqAiService(HttpClient httpClient, IConfiguration configuration, ILogger<GroqAiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _apiKey = configuration["GroqSettings:ApiKey"]?.Trim()
                  ?? throw new InvalidOperationException("GroqSettings:ApiKey sistemde yapılandırılamadı.");

        _model = configuration["GroqSettings:Model"]?.Trim() ?? "llama-3.1-8b-instant";
        var tempStr = configuration["GroqSettings:Temperature"];
        _temperature = double.TryParse(tempStr, System.Globalization.CultureInfo.InvariantCulture, out var parsedTemp)
            ? parsedTemp
            : 0.3;
    }

    public async Task<ChatResponse> GenerateResponseAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new GroqRequestPayload(
            Model: _model,
            Messages: [new GroqRequestMessage(Role: "user", Content: request.Message)],
            Temperature: _temperature
        );

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GroqResponseStructure>(JsonOptions, cancellationToken);
            var aiText = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Anlayamadım, tekrar eder misiniz?";

            return new ChatResponse { Response = aiText.Trim() };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yapay zeka entegrasyon katmanında bir kriz oluştu. Sabit fallback yanıtı devreye alınıyor.");

            return new ChatResponse
            {
                Response = "Şu anda işlemlerinizi gerçekleştiremiyorum. Lütfen daha sonra tekrar arayınız."
            };
        }
    }
}