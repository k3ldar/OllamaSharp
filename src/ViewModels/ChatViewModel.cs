using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaSharp.Models;
using OllamaSharp.Services;
using UiChatMessage = OllamaSharp.Models.ChatMessage;

namespace OllamaSharp.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private const string NewChatText = "New Chat";
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private Guid? _currentChatId;

    [ObservableProperty]
    private string _currentChatTitle = NewChatText;

    [ObservableProperty]
    private bool _hasMessages;

    private CancellationTokenSource _cancellationTokenSource;

    public ObservableCollection<UiChatMessage> Messages { get; } = [];

    private readonly OllamaChatService _chatService;
    private readonly ChatStorageService _storageService;

    public ChatViewModel(OllamaChatService chatService, ChatStorageService storageService)
    {
        _chatService = chatService;
        _storageService = storageService;

        // Initialize service with global settings
        UpdateServiceFromPreferences();
    }

    private void UpdateServiceFromPreferences()
    {
        // Read global settings from preferences
        var systemRole = Preferences.Get(SettingsPage.PrefKeySystemRole, Constants.DefaultSystemBehaviour);
        var maxHistoryPairs = Preferences.Get(SettingsPage.PrefKeyMaxHistoryPairs, Constants.DefaultMaxHistoryPairs);

        _chatService.SystemRole = systemRole;
        _chatService.MaxHistoryPairs = maxHistoryPairs;

        System.Diagnostics.Debug.WriteLine($"ChatViewModel initialized with: SystemRole='{systemRole}', MaxHistoryPairs={maxHistoryPairs}");
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task Send()
    {
        var text = InputText?.Trim();

        if (string.IsNullOrEmpty(text))
            return;

        // Refresh settings from preferences before sending
        UpdateServiceFromPreferences();

        // Add the user's message
        Messages.Add(new UiChatMessage { From = Sender.User, Text = text });

        // Update has messages flag
        HasMessages = true;

        // Clear input
        InputText = string.Empty;

        // Add a placeholder assistant message 
        var assistantMessage = new UiChatMessage { From = Sender.Assistant, Text = "" };
        Messages.Add(assistantMessage);

        // Create new cancellation token
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        IsGenerating = true;

        // Call AI with streaming callback to update UI in real-time
        try
        {
            await _chatService.SendMessageAsync(text, partialResponse =>
            {
                System.Diagnostics.Debug.WriteLine($"ViewModel received partial: length={partialResponse.Length}");
                // Update on the main thread so UI updates immediately
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"UI thread updating text to length: {partialResponse.Length}");
                    assistantMessage.Text = partialResponse;
                });
            }, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                assistantMessage.Text += $"{Constants.CharLineFeed}{Constants.CharLineFeed}[Stopped by user]";
            });
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                assistantMessage.Text = $"Error: {ex.Message}";
            });
        }
        finally
        {
            IsGenerating = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            // Auto-save after message exchange
            await SaveCurrentChatAsync();
        }
    }

    public async Task SaveCurrentChatAsync()
    {
        // Don't save if there are no messages
        if (Messages.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine(Constants.DebugMsgNoMessagesToSave);
            return;
        }

        try
        {
            // Create or update chat
            var savedChat = new SavedChat
            {
                Id = CurrentChatId ?? Guid.NewGuid(),
                Model = Preferences.Get(SettingsPage.PrefKeyModelName, Constants.DefaultModel),
                SystemRole = Preferences.Get(SettingsPage.PrefKeySystemRole, Constants.DefaultSystemBehaviour),
                LastOpened = DateTimeOffset.Now,
                Messages = [.. Messages.Select(m => new SavedChatMessage
                {
                    From = m.From,
                    Text = m.Text,
                    Timestamp = m.Timestamp
                })]
            };

            // Generate title from first user message if not already set
            if (CurrentChatId == null || CurrentChatTitle == NewChatText)
            {
                var firstUserMessage = Messages.FirstOrDefault(m => m.From == Sender.User);

                if (firstUserMessage != null)
                {
                    savedChat.Title = ChatStorageService.GenerateTitle(firstUserMessage.Text);
                    savedChat.Created = DateTimeOffset.Now;
                    CurrentChatTitle = savedChat.Title;
                }
                else
                {
                    savedChat.Title = NewChatText;
                }
            }
            else
            {
                savedChat.Title = CurrentChatTitle;

                // Load existing chat to preserve Created date
                var existingChat = await _storageService.LoadChatAsync(savedChat.Id);

                if (existingChat != null)
                {
                    savedChat.Created = existingChat.Created;
                }
            }

            CurrentChatId = savedChat.Id;
            await _storageService.SaveChatAsync(savedChat);

            System.Diagnostics.Debug.WriteLine($"Chat saved: {savedChat.Title} ({savedChat.Id})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving chat: {ex.Message}");
        }
    }

    public void StartNewChat()
    {
        System.Diagnostics.Debug.WriteLine(Constants.DebugMsgNewChatStarted);

        // Cancel any ongoing generation
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        // Reset generation state
        IsGenerating = false;

        CurrentChatId = null;
        CurrentChatTitle = NewChatText;
        Messages.Clear();
        HasMessages = false;
        _chatService.ClearHistory();
        InputText = string.Empty;

        // Refresh settings from preferences
        UpdateServiceFromPreferences();
    }

    public async Task LoadChatAsync(SavedChat chat)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Loading chat: {chat.Title} ({chat.Id})");

            // Cancel any ongoing generation
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            // Reset generation state
            IsGenerating = false;

            // Set current chat
            CurrentChatId = chat.Id;
            CurrentChatTitle = chat.Title;

            // Update last opened timestamp
            chat.LastOpened = DateTimeOffset.Now;
            await _storageService.SaveChatAsync(chat);

            // Clear existing messages
            Messages.Clear();

            // Load messages
            foreach (var msg in chat.Messages)
            {
                Messages.Add(new UiChatMessage
                {
                    From = msg.From,
                    Text = msg.Text,
                    Timestamp = msg.Timestamp
                });
            }

            // Update has messages flag
            HasMessages = Messages.Count > 0;

            // Update service configuration with saved settings
            var currentUrl = Preferences.Get(SettingsPage.PrefKeyServerUrl, Constants.DefaultOllamaUrl);
            _chatService.UpdateConfiguration(currentUrl, chat.Model);
            _chatService.SystemRole = chat.SystemRole;

            // Restore conversation history to service
            var history = chat.Messages.Select(m => (
                role: m.From == Sender.User ? Constants.MessageTypeUser : Constants.MessageTypeAssistant,
                content: m.Text
            )).ToList();
            _chatService.RestoreHistory(history);

            System.Diagnostics.Debug.WriteLine($"Chat loaded: {chat.Messages.Count} messages");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading chat: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }

    // Triggers sending a predefined prompt from UI buttons
    [RelayCommand]
    private async Task SendPrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return;

        InputText = prompt.Trim();

        if (CanSend())
        {
            await Send();
        }
    }

    private bool CanSend() => !string.IsNullOrWhiteSpace(InputText) && !IsGenerating;
}
