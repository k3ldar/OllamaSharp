namespace OllamaSharp.Models;

public class SavedChatMessage
{
    public Sender From { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
}
