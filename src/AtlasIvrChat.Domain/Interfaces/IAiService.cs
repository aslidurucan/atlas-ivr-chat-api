using AtlasIvrChat.Domain.Models;


namespace AtlasIvrChat.Domain.Interfaces
{
    public interface IAiService
    {
        Task<ChatResponse> GenerateResponseAsync(ChatRequest request, CancellationToken cancellationToken = default);
    }
}
