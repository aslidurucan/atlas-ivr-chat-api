using System.Net.Http.Json;
using AtlasIvrChat.Domain.Interfaces;
using AtlasIvrChat.Domain.Models;

namespace AtlasIvrChat.Infrastructure.Services;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "YOUR_GEMINI_API_KEY_HERE";
    private const string ApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={ApiKey}";

    public GeminiAiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ChatResponse> GenerateResponseAsync(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new ChatResponse { Response = "Gelen mesaj boş olamaz." };
        }

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = request.Message } } }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload);

            if (!response.IsSuccessStatusCode)
            {
                return new ChatResponse { Response = "Yapay zeka servisi şu an yanıt veremiyor. Lütfen tekrar deneyiniz." };
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponseStructure>();

            string aiText = result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "Anlayamadım, tekrar eder misiniz?";

            return new ChatResponse
            {
                Response = aiText.Trim()
            };
        }
        catch (Exception)
        {
            return new ChatResponse { Response = "Sistemsel bir hata oluştu. Lütfen hattan ayrılmayınız." };
        }
    }
}

internal class GeminiResponseStructure
{
    public Candidate[]? Candidates { get; set; }
}

internal class Candidate
{
    public Content? Content { get; set; }
}

internal class Content
{
    public Part[]? Parts { get; set; }
}

internal class Part
{
    public string? Text { get; set; }
}