namespace OllamaSharp.Models;

public class SavedChat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SystemRole { get; set; } = string.Empty;
    public List<SavedChatMessage> Messages { get; set; } = [];
    public DateTimeOffset LastOpened { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
}
