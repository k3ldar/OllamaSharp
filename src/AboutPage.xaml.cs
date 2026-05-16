namespace OllamaSharp;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private async void OnGitHubTapped(object sender, EventArgs e)
    {
        OpenLink(Constants.UriGithubRepo);
    }

    private async void OnIssuesTapped(object sender, EventArgs e)
    {
        OpenLink(Constants.UriGithubIssues);
    }

    private async void OpenLink(string url)
    {
        try
        {
            await Launcher.OpenAsync(new Uri(url));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(Constants.Error, $"Unable to open link: {ex.Message}", Constants.DialogButtonTextOk);
        }
    }
}
