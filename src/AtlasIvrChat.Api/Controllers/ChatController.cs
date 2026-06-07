using AtlasIvrChat.Domain.Interfaces;
using AtlasIvrChat.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace AtlasIvrChat.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IAiService _aiService;

    public ChatController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    {
        var result = await _aiService.GenerateResponseAsync(request);

        return Ok(result);
    }
}