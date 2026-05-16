using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaSharp.Models;
using OllamaSharp.Services;
using OllamaSharp.ViewModels;

namespace OllamaSharp.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    private const string TitleDeleteChat = "Delete Chat";
    private readonly ChatStorageService _storageService;
    private readonly ChatViewModel _chatViewModel;

    [ObservableProperty]
    private ObservableCollection<SavedChat> _savedChats = [];

    public AppShellViewModel(ChatStorageService storageService, ChatViewModel chatViewModel)
    {
        _storageService = storageService;
        _chatViewModel = chatViewModel;
    }

    [RelayCommand]
    public async Task LoadSavedChats()
    {
        try
        {
            var chats = await _storageService.LoadAllChatsAsync();

            SavedChats.Clear();
            foreach (var chat in chats)
            {
                SavedChats.Add(chat);
            }

            System.Diagnostics.Debug.WriteLine($"Loaded {SavedChats.Count} saved chats into menu");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading saved chats: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task DeleteChat(SavedChat chat)
    {
        try
        {
            // Show confirmation dialog
            bool confirmed = await Shell.Current.DisplayAlertAsync(
                TitleDeleteChat,
                $"Are you sure you want to delete '{chat.Title}'?",
                Constants.DialogButtonDelete,
                Constants.DialogButtonTextCancel);

            if (!confirmed)
            {
                System.Diagnostics.Debug.WriteLine($"Chat deletion cancelled: {chat.Title}");
                return;
            }

            await _storageService.DeleteChatAsync(chat.Id);
            SavedChats.Remove(chat);

            // If the deleted chat was the current one, start a new chat
            if (_chatViewModel.CurrentChatId == chat.Id)
            {
                _chatViewModel.StartNewChat();
            }

            System.Diagnostics.Debug.WriteLine($"Chat deleted: {chat.Title}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting chat: {ex.Message}");
            await Shell.Current.DisplayAlertAsync(Constants.Error, $"Failed to delete chat: {ex.Message}", Constants.DialogButtonTextOk);
        }
    }

    [RelayCommand]
    public async Task OpenChat(SavedChat chat)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"OpenChat called for: {chat?.Title}");

            if (chat == null)
            {
                System.Diagnostics.Debug.WriteLine(Constants.DebugMsgChatParameterNull);
                return;
            }

            // Close the flyout first
            Shell.Current.FlyoutIsPresented = false;

            // Auto-save current chat before switching (if it has messages)
            await _chatViewModel.SaveCurrentChatAsync();

            // Load the selected chat
            await _chatViewModel.LoadChatAsync(chat);

            // Navigate to MainPage
            await Shell.Current.GoToAsync(Constants.PageMain);

            // Reload saved chats to update order
            await LoadSavedChats();

            System.Diagnostics.Debug.WriteLine($"Opened chat: {chat.Title}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening chat: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await Shell.Current.DisplayAlertAsync(Constants.Error, $"Failed to open chat: {ex.Message}", Constants.DialogButtonTextOk);
        }
    }

    [RelayCommand]
    public async Task NewChat()
    {
        try
        {
            // Auto-save current chat before creating new one (if it has messages)
            await _chatViewModel.SaveCurrentChatAsync();

            // Start a new chat
            _chatViewModel.StartNewChat();

            // Navigate to MainPage
            await Shell.Current.GoToAsync(Constants.PageMain);

            // Close the flyout
            Shell.Current.FlyoutIsPresented = false;

            System.Diagnostics.Debug.WriteLine(Constants.DebugMsgNewChatStarted);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting new chat: {ex.Message}");
        }
    }

    [RelayCommand]
    public static async Task NavigateToSettings()
    {
        try
        {
            await Shell.Current.GoToAsync(Constants.PageSettings);

            // Close the flyout
            Shell.Current.FlyoutIsPresented = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to settings: {ex.Message}");
        }
    }

    [RelayCommand]
    public static async Task NavigateToAbout()
    {
        try
        {
            await Shell.Current.GoToAsync(Constants.PageAbout);

            // Close the flyout
            Shell.Current.FlyoutIsPresented = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to about: {ex.Message}");
        }
    }
}
