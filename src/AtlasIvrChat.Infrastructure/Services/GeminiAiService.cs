using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AtlasIvrChat.Domain.Interfaces;
using AtlasIvrChat.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace AtlasIvrChat.Infrastructure.Services;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GroqSettings:ApiKey"]?.Trim() ?? string.Empty;
    }

    public async Task<ChatResponse> GenerateResponseAsync(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return new ChatResponse { Response = "Gelen mesaj boş olamaz." };

        if (string.IsNullOrWhiteSpace(_apiKey))
            return new ChatResponse { Response = "API Anahtarı sistemde bulunamadı." };

        var payload = new GroqRequestPayload(
            Model: "llama-3.1-8b-instant",
            Messages: [new GroqRequestMessage(Role: "user", Content: request.Message)],
            Temperature: 0.3
        );

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GroqResponseStructure>(JsonOptions);
            var aiText = result?.Choices?[0].Message.Content ?? "Anlayamadım, tekrar eder misiniz?";

            return new ChatResponse { Response = aiText.Trim() };
        }
        catch (Exception ex)
        {
            return new ChatResponse { Response = $"Sistemsel bir kesinti oluştu. (Hata: {ex.Message})" };
        }
    }
}

internal record GroqRequestPayload(string Model, GroqRequestMessage[] Messages, double Temperature);
internal record GroqRequestMessage(string Role, string Content);
internal record GroqResponseStructure(Choice[] Choices);
internal record Choice(GroqMessage Message);
internal record GroqMessage(string Content);