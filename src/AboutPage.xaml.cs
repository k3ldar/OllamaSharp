namespace OllamaSharp;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private async void OnGitHubTapped(object sender, EventArgs e)
    {
        try
        {
            await Launcher.OpenAsync(new Uri("https://github.com/k3ldar/OllamaSharp"));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to open link: {ex.Message}", "OK");
        }
    }

    private async void OnIssuesTapped(object sender, EventArgs e)
    {
        try
        {
            await Launcher.OpenAsync(new Uri("https://github.com/k3ldar/OllamaSharp/issues"));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to open link: {ex.Message}", "OK");
        }
    }
}
