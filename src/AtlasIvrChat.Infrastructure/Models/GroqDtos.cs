namespace AtlasIvrChat.Infrastructure.Models;

internal record GroqRequestPayload(string Model, GroqRequestMessage[] Messages, double Temperature);
internal record GroqRequestMessage(string Role, string Content);
internal record GroqResponseStructure(Choice[] Choices);
internal record Choice(GroqMessage Message);
internal record GroqMessage(string Content);
