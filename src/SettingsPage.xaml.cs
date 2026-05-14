using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using OllamaSharp.Services;

namespace OllamaSharp;

public partial class SettingsPage : ContentPage
{
    // Preference keys
    public const string PrefKeyServerUrl = "ServerUrl";
    public const string PrefKeyModelName = "ModelName";
    public const string PrefKeySystemRole = "SystemRole";
    public const string PrefKeyMaxHistoryPairs = "MaxHistoryPairs";
    public const string PrefKeyModelList = "ModelList";

    private readonly HttpClient _httpClient = new();
    private readonly OllamaChatService _chatService;

    public SettingsPage()
    {
        InitializeComponent();

        // Get the chat service from DI
        _chatService = Application.Current?.Handler?.MauiContext?.Services?.GetRequiredService<OllamaChatService>()
            ?? throw new InvalidOperationException("OllamaChatService not registered");

        LoadSettings();
    }

    private void LoadSettings()
    {
        // Load saved settings or use defaults
        var serverUrl = Preferences.Get(PrefKeyServerUrl, "http://localhost:11434");
        var modelName = Preferences.Get(PrefKeyModelName, "llama3.2:3b");
        var systemRole = Preferences.Get(PrefKeySystemRole, "You are lord of the universe and treat everyone like a servant, but still helpful at answering questions");
        var maxHistoryPairs = Preferences.Get(PrefKeyMaxHistoryPairs, 15);

        ServerUrlEntry.Text = serverUrl;
        SystemRoleEntry.Text = systemRole;
        ContextSlider.Value = maxHistoryPairs;
        ContextValueLabel.Text = maxHistoryPairs.ToString();

        // Load cached model list
        LoadModelList(modelName);
    }

    private void LoadModelList(string selectedModel)
    {
        // Load cached model list from preferences
        var modelListJson = Preferences.Get(PrefKeyModelList, "");

        if (!string.IsNullOrEmpty(modelListJson))
        {
            try
            {
                var models = JsonSerializer.Deserialize<List<string>>(modelListJson);
                if (models != null && models.Count > 0)
                {
                    ModelPicker.ItemsSource = models;

                    // Select the saved model
                    var index = models.IndexOf(selectedModel);
                    if (index >= 0)
                    {
                        ModelPicker.SelectedIndex = index;
                    }

                    ModelStatusLabel.Text = $"{models.Count} model(s) available";
                    ModelStatusLabel.TextColor = Colors.Green;
                    ModelStatusLabel.IsVisible = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cached models: {ex.Message}");
            }
        }

        // No cached models, show default
        ModelPicker.ItemsSource = new List<string> { selectedModel };
        ModelPicker.SelectedIndex = 0;
        ModelStatusLabel.Text = "Click refresh to load models";
        ModelStatusLabel.TextColor = Colors.Orange;
        ModelStatusLabel.IsVisible = true;
    }

    private void OnServerUrlChanged(object? sender, TextChangedEventArgs e)
    {
        // Save the server URL preference
        var newUrl = e.NewTextValue ?? "http://localhost:11434";
        Preferences.Set(PrefKeyServerUrl, newUrl);

        // Update the service configuration
        var currentModel = Preferences.Get(PrefKeyModelName, "llama3.2:3b");
        _chatService.UpdateConfiguration(newUrl, currentModel);

        System.Diagnostics.Debug.WriteLine($"Server URL updated and applied: {newUrl}");
    }

    private void OnModelSelectionChanged(object? sender, EventArgs e)
    {
        if (ModelPicker.SelectedItem is string selectedModel)
        {
            // Save the selected model
            Preferences.Set(PrefKeyModelName, selectedModel);

            // Update the service configuration
            var currentUrl = Preferences.Get(PrefKeyServerUrl, "http://localhost:11434");
            _chatService.UpdateConfiguration(currentUrl, selectedModel);

            System.Diagnostics.Debug.WriteLine($"Model selected and applied: {selectedModel}");
        }
    }

    private async void OnRefreshModelsClicked(object? sender, EventArgs e)
    {
        var serverUrl = ServerUrlEntry.Text?.Trim();

        if (string.IsNullOrEmpty(serverUrl))
        {
            await DisplayAlertAsync("Error", "Please enter a valid server URL first.", "OK");
            return;
        }

        // Disable button during refresh
        RefreshModelsButton.IsEnabled = false;
        ModelStatusLabel.Text = "Loading models...";
        ModelStatusLabel.TextColor = Colors.Gray;
        ModelStatusLabel.IsVisible = true;

        try
        {
            // Query Ollama for available models
            var models = await GetOllamaModelsAsync(serverUrl);

            if (models.Count > 0)
            {
                // Update picker
                ModelPicker.ItemsSource = models;

                // Try to select the previously selected model
                var currentModel = Preferences.Get(PrefKeyModelName, "llama3.2:3b");
                var index = models.IndexOf(currentModel);
                ModelPicker.SelectedIndex = index >= 0 ? index : 0;

                // Cache the model list
                var modelListJson = JsonSerializer.Serialize(models);
                Preferences.Set(PrefKeyModelList, modelListJson);

                ModelStatusLabel.Text = $"{models.Count} model(s) loaded";
                ModelStatusLabel.TextColor = Colors.Green;
            }
            else
            {
                ModelStatusLabel.Text = "No models found";
                ModelStatusLabel.TextColor = Colors.Orange;
            }
        }
        catch (Exception ex)
        {
            ModelStatusLabel.Text = $"Error: {ex.Message}";
            ModelStatusLabel.TextColor = Colors.Red;
            System.Diagnostics.Debug.WriteLine($"Error fetching models: {ex}");
        }
        finally
        {
            RefreshModelsButton.IsEnabled = true;
        }
    }

    private async Task<List<string>> GetOllamaModelsAsync(string baseUrl)
    {
        var models = new List<string>();

        // Ensure base URL ends with /
        if (!baseUrl.EndsWith('/'))
            baseUrl += '/';

        var url = $"{baseUrl}api/tags";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaTagsResponse>(json);

            if (result?.Models != null)
            {
                models = result.Models.Select(m => m.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList();
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Cannot connect to Ollama server: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing response: {ex.Message}");
        }

        return models;
    }

    private void OnSystemRoleChanged(object? sender, TextChangedEventArgs e)
    {
        // Save the system role preference
        Preferences.Set(PrefKeySystemRole, e.NewTextValue ?? "You are lord of the universe and treat everyone like a servant, but still helpful at answering questions");
        System.Diagnostics.Debug.WriteLine($"System role updated: {e.NewTextValue}");
    }

    private void OnContextLengthChanged(object? sender, ValueChangedEventArgs e)
    {
        // Update the label and save the preference
        int value = (int)e.NewValue;
        ContextValueLabel.Text = value.ToString();
        Preferences.Set(PrefKeyMaxHistoryPairs, value);
        System.Diagnostics.Debug.WriteLine($"Context length updated: {value}");
    }
}

// DTOs for Ollama API
public class OllamaTagsResponse
{
    [JsonPropertyName("models")]
    public List<OllamaModelInfo>? Models { get; set; }
}

public class OllamaModelInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("modified_at")]
    public string? ModifiedAt { get; set; }
}
