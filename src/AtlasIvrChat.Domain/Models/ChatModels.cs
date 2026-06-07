
using System.ComponentModel.DataAnnotations;

namespace AtlasIvrChat.Domain.Models;

public class ChatRequest
{
    [Required(ErrorMessage = "Gelen mesaj boş olamaz.")]
    [MinLength(1, ErrorMessage = "Mesaj en az 1 karakter olmalıdır.")]
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
}
