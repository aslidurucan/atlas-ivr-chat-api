using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AtlasIvrChat.Domain.Interfaces;
using AtlasIvrChat.Domain.Models;
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

        _temperature = double.TryParse(configuration["GroqSettings:Temperature"], out var temp) ? temp : 0.3;
    }

    public async Task<ChatResponse> GenerateResponseAsync(ChatRequest request)
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

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GroqResponseStructure>(JsonOptions);

            var aiText = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Anlayamadım, tekrar eder misiniz?";

            return new ChatResponse { Response = aiText.Trim() };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Groq API uç noktasıyla iletişim kurulurken bir HTTP hatası oluştu. Mesaj: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yapar zeka yanıtı işlenirken beklenmeyen bir iç hata meydana geldi.");
            throw;
        }
    }
}

internal record GroqRequestPayload(string Model, GroqRequestMessage[] Messages, double Temperature);
internal record GroqRequestMessage(string Role, string Content);
internal record GroqResponseStructure(Choice[] Choices);
internal record Choice(GroqMessage Message);
internal record GroqMessage(string Content);