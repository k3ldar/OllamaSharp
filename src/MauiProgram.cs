using Microsoft.Extensions.Logging;
using OllamaSharp.ViewModels;
using OllamaSharp.Services;
using Fonts;
using Syncfusion.Maui.Toolkit.Hosting;

namespace OllamaSharp;

public static class MauiProgram
{

    public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureSyncfusionToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont(Constants.FontFileNameOpenSansRegular, Constants.FontNameOpenSansRegular);
				fonts.AddFont(Constants.FontFileNameOpenSansSemiBold, Constants.FontNameOpenSansSemiBold);
				fonts.AddFont(Constants.FontFileNameFluentSystemsIcons, FluentUI.FontFamily);
			});

		// Register Services
		builder.Services.AddSingleton<ChatStorageService>();

		// Register ViewModels
		builder.Services.AddSingleton<ChatViewModel>();
		builder.Services.AddSingleton<AppShellViewModel>();

		// Register Ollama chat service
		builder.Services.AddSingleton(sp =>
		{
			// Read server settings from preferences
			var baseUrl = Preferences.Get(SettingsPage.PrefKeyServerUrl, Constants.DefaultOllamaUrl);
			var model = Preferences.Get(SettingsPage.PrefKeyModelName, Constants.DefaultModel);

			var ollamaService = new OllamaChatService(baseUrl, model);
			return ollamaService;
		});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
