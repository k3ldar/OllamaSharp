using System.Text.Json;
using OllamaSharp.Models;

namespace OllamaSharp.Services;

public class ChatStorageService
{
    private readonly string _chatsDirectory;

    public ChatStorageService()
    {
        _chatsDirectory = Path.Combine(FileSystem.AppDataDirectory, "Chats");

        // Ensure the Chats directory exists
        if (!Directory.Exists(_chatsDirectory))
        {
            Directory.CreateDirectory(_chatsDirectory);
            System.Diagnostics.Debug.WriteLine($"Created Chats directory: {_chatsDirectory}");
        }
    }

    public string GetChatsDirectory() => _chatsDirectory;

    public async Task SaveChatAsync(SavedChat chat)
    {
        try
        {
            var filePath = Path.Combine(_chatsDirectory, $"{chat.Id}.json");
            var json = JsonSerializer.Serialize(chat, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            await File.WriteAllTextAsync(filePath, json);
            System.Diagnostics.Debug.WriteLine($"Chat saved: {chat.Title} ({chat.Id})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving chat: {ex.Message}");
            throw;
        }
    }

    public async Task<SavedChat?> LoadChatAsync(Guid id)
    {
        try
        {
            var filePath = Path.Combine(_chatsDirectory, $"{id}.json");

            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"Chat file not found: {id}");
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var chat = JsonSerializer.Deserialize<SavedChat>(json);

            System.Diagnostics.Debug.WriteLine($"Chat loaded: {chat?.Title} ({id})");
            return chat;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading chat: {ex.Message}");
            return null;
        }
    }

    public async Task<List<SavedChat>> LoadAllChatsAsync()
    {
        var chats = new List<SavedChat>();

        try
        {
            if (!Directory.Exists(_chatsDirectory))
                return chats;

            var files = Directory.GetFiles(_chatsDirectory, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var chat = JsonSerializer.Deserialize<SavedChat>(json);

                    if (chat != null)
                        chats.Add(chat);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading chat file {file}: {ex.Message}");
                }
            }

            // Sort by LastOpened descending (most recent first)
            chats = chats.OrderByDescending(c => c.LastOpened).ToList();

            System.Diagnostics.Debug.WriteLine($"Loaded {chats.Count} chat(s)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading chats: {ex.Message}");
        }

        return chats;
    }

    public async Task DeleteChatAsync(Guid id)
    {
        try
        {
            var filePath = Path.Combine(_chatsDirectory, $"{id}.json");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                System.Diagnostics.Debug.WriteLine($"Chat deleted: {id}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting chat: {ex.Message}");
            throw;
        }

        // Make async to match signature
        await Task.CompletedTask;
    }

    public string GenerateTitle(string firstMessage)
    {
        if (string.IsNullOrWhiteSpace(firstMessage))
            return "New Chat";

        // Take first 5 words
        var words = firstMessage.Trim().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var titleWords = words.Take(5);
        var title = string.Join(" ", titleWords);

        // Add ellipsis if there are more words
        if (words.Length > 5)
            title += "...";

        // Limit title length to 50 characters max
        if (title.Length > 50)
            title = title.Substring(0, 47) + "...";

        return title;
    }
}
