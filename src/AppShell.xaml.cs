using OllamaSharp.ViewModels;

namespace OllamaSharp;

public partial class AppShell : Shell
{
	private readonly AppShellViewModel _viewModel;

	public AppShell()
	{
		InitializeComponent();

		// Get ViewModel from DI
		_viewModel = Application.Current?.Handler?.MauiContext?.Services?.GetRequiredService<AppShellViewModel>()
			?? throw new InvalidOperationException(Constants.ErrAppViewModelNotRegistered);

		BindingContext = _viewModel;

		// Simple flyout behavior - slides in/out on all platforms
		FlyoutBehavior = FlyoutBehavior.Flyout;

		// Reload saved chats whenever flyout is opened
		PropertyChanged += async (s, e) =>
		{
			if (e.PropertyName == nameof(FlyoutIsPresented) && FlyoutIsPresented)
			{
				await _viewModel.LoadSavedChatsCommand.ExecuteAsync(null);
			}
		};
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);

		// Load saved chats when shell appears
		await _viewModel.LoadSavedChatsCommand.ExecuteAsync(null);
	}
}
