using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OllamaSharp.Services;

public class OllamaChatService
{
    private HttpClient _httpClient;
    private string _model;
    private readonly List<OllamaMessage> _conversationHistory = new();

    /// <summary>
    /// Maximum number of message pairs (user+assistant) to keep in history.
    /// Older messages are automatically trimmed to avoid exceeding model context limits.
    /// Default: 15 pairs = 30 messages (good for most 3B-13B models)
    /// Recommended: 3B models: 15-20, 7B models: 20-30, 70B models: 5-10
    /// </summary>
    public int MaxHistoryPairs { get; set; } = 15;

    public OllamaChatService(string baseUrl, string model)
    {
        _model = model;
        InitializeHttpClient(baseUrl);
    }

    private void InitializeHttpClient(string baseUrl)
    {
        // Ensure base URL ends with /
        if (!baseUrl.EndsWith('/'))
            baseUrl += '/';

        _httpClient?.Dispose();
        _httpClient = new HttpClient 
        { 
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(5) // Increase timeout for longer responses
        };
    }

    /// <summary>
    /// Updates the server URL and model. Clears conversation history.
    /// </summary>
    public void UpdateConfiguration(string baseUrl, string model)
    {
        InitializeHttpClient(baseUrl);
        _model = model;
        ClearHistory();
        System.Diagnostics.Debug.WriteLine($"OllamaChatService reconfigured: URL={baseUrl}, Model={model}");
    }

    public string SystemRole { get; set; } = "You are lord of the universe and treat everyone like a servant, but still helpful at answering questions";

    private void TrimHistoryIfNeeded()
    {
        // Keep only the most recent N pairs (each pair = user message + assistant response = 2 messages)
        int maxMessages = MaxHistoryPairs * 2;

        if (_conversationHistory.Count > maxMessages)
        {
            int messagesToRemove = _conversationHistory.Count - maxMessages;
            _conversationHistory.RemoveRange(0, messagesToRemove);
            System.Diagnostics.Debug.WriteLine($"Trimmed {messagesToRemove} old messages. History now has {_conversationHistory.Count} messages.");
        }
    }

    public async Task<string> SendMessageAsync(string userMessage, Action<string>? onPartialResponse = null, CancellationToken cancellationToken = default)
    {
        // Trim history before building the request to stay within context limits
        TrimHistoryIfNeeded();

        // Build the message list including system role and conversation history
        var messages = new List<OllamaMessage>();

        // Always include system role first (never trimmed - essential for AI behavior)
        messages.Add(new OllamaMessage { Role = "system", Content = SystemRole });

        // Add conversation history (already trimmed to MaxHistoryPairs)
        messages.AddRange(_conversationHistory);

        // Add current user message
        var currentUserMessage = new OllamaMessage { Role = "user", Content = userMessage };
        messages.Add(currentUserMessage);

        var request = new OllamaChatRequest
        {
            Model = _model,
            Messages = messages,
            Stream = true // Enable streaming for real-time updates
        };

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/chat");
            httpRequest.Content = JsonContent.Create(request);

            using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var fullContent = string.Empty;

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line);
                    if (chunk?.Message?.Content != null && !string.IsNullOrEmpty(chunk.Message.Content))
                    {
                        fullContent += chunk.Message.Content;

                        // Call the callback to update UI in real-time
                        System.Diagnostics.Debug.WriteLine($"Streaming chunk: '{chunk.Message.Content}' - Total length: {fullContent.Length}");
                        onPartialResponse?.Invoke(fullContent);
                    }

                    // Check if this is the final chunk
                    if (chunk?.Done == true)
                        break;
                }
                catch (JsonException)
                {
                    // Skip malformed JSON lines
                    continue;
                }
            }

            if (!string.IsNullOrEmpty(fullContent))
            {
                // Add user message and assistant response to history
                _conversationHistory.Add(currentUserMessage);
                _conversationHistory.Add(new OllamaMessage 
                { 
                    Role = "assistant", 
                    Content = fullContent 
                });

                return fullContent;
            }

            return "No response from Ollama.";
        }
        catch (OperationCanceledException)
        {
            // Re-throw cancellation to be handled by caller
            throw;
        }
        catch (Exception ex)
        {
            return $"Error communicating with Ollama: {ex.Message}";
        }
    }

    public void ClearHistory()
    {
        _conversationHistory.Clear();
    }

    public void RestoreHistory(List<(string role, string content)> history)
    {
        _conversationHistory.Clear();

        foreach (var (role, content) in history)
        {
            _conversationHistory.Add(new OllamaMessage
            {
                Role = role,
                Content = content
            });
        }

        System.Diagnostics.Debug.WriteLine($"Restored {history.Count} messages to conversation history");
    }
}

// DTOs for Ollama API
public class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OllamaMessage> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}

public class OllamaMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class OllamaChatResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("message")]
    public OllamaMessage? Message { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}
