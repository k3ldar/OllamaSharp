using CommunityToolkit.Mvvm.ComponentModel;

namespace OllamaSharp.Models;

public enum Sender
{
    User,
    Assistant
}

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string _text = string.Empty;

    public required Sender From { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

    // Property to show typing indicator when assistant message is empty
    public bool IsThinking => From == Sender.Assistant && string.IsNullOrEmpty(Text);

    // Notify IsThinking when Text changes
    partial void OnTextChanged(string value)
    {
        OnPropertyChanged(nameof(IsThinking));
    }
}
