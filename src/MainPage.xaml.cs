using OllamaSharp.Models;
using OllamaSharp.ViewModels;

namespace OllamaSharp;

public partial class MainPage : ContentPage
{
	private const double BottomThresholdPixels = 20; // How close to bottom counts as "at bottom"

	private readonly ChatViewModel _viewModel;
	private bool _shouldAutoScroll = true;
	private bool _userHasInputData = false;
    private ChatMessage? _currentStreamingMessage = null;

	public MainPage() : this(
		Application.Current?.Handler?.MauiContext?.Services?.GetRequiredService<ChatViewModel>()
		?? throw new InvalidOperationException("ChatViewModel not registered"))
	{ }

	public MainPage(ChatViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		_viewModel = vm;

		// Monitor messages collection changes
		vm.Messages.CollectionChanged += OnMessagesCollectionChanged;
	}

	private void OnMessagesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		// When a new message is added
		if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
		{
			foreach (var item in e.NewItems)
			{
				if (item is ChatMessage msg)
				{
					// If it's a user message, detach scroll handler immediately
					if (msg.From == Sender.User)
					{
						ScrollToBottom();
						continue;
					}

					System.Diagnostics.Debug.WriteLine($"Message added: From={msg.From}, TextLength={msg.Text.Length}");

					// If it's an assistant message, subscribe to text changes for streaming updates
					if (msg.From == Sender.Assistant)
					{
						// Unsubscribe from previous streaming message
						if (_currentStreamingMessage != null)
						{
							_currentStreamingMessage.PropertyChanged -= OnStreamingMessageChanged;
							System.Diagnostics.Debug.WriteLine("Unsubscribed from previous streaming message");
						}

						_currentStreamingMessage = msg;
						_currentStreamingMessage.PropertyChanged += OnStreamingMessageChanged;
						System.Diagnostics.Debug.WriteLine("Subscribed to new streaming message");
					}
				}
			}
		}
	}

	private void OnStreamingMessageChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		// When the streaming message text changes, auto-scroll if user hasn't scrolled away
		if (e.PropertyName == nameof(ChatMessage.Text))
		{
			var msg = sender as ChatMessage;
			ScrollToBottom();
		}
	}

	private void OnScrolled(object? sender, ItemsViewScrolledEventArgs e)
	{
		if (_userHasInputData)
		{
			_userHasInputData = false;
			_shouldAutoScroll = true;
			return;
		}
		else
		{
			if (e.VerticalDelta < 0)
			{
				_shouldAutoScroll = false;
				return;
			}
		}

        // Detect if user is at bottom based on item index
        bool atBottom = e.LastVisibleItemIndex >= _viewModel.Messages.Count - 1;

        if (atBottom)
            _shouldAutoScroll = true;
    }

	private void ScrollToBottom()
	{
		if (MessagesView.ItemsSource is not null && _shouldAutoScroll && _viewModel.Messages.Count > 0)
		{
			MessagesView.ScrollTo(
                _viewModel.Messages.Last(),
                position: ScrollToPosition.End,
                animate: true);
		}
	}

	// Enter key from Editor triggers Send
	private void OnCompleted(object? sender, EventArgs e)
	{
		if (BindingContext is ChatViewModel vm && vm.SendCommand.CanExecute(null))
		{
			vm.SendCommand.Execute(null);
			_userHasInputData = true;
        }
	}

	private void OnTextChanged(object? sender, TextChangedEventArgs e)
	{
		// If user presses Enter, many platforms insert '\n' into the Editor.
		// When we detect trailing newline and there's text before it, submit.
		var text = e.NewTextValue ?? string.Empty;
		if (text.EndsWith("\n", StringComparison.Ordinal))
		{
			if (BindingContext is ChatViewModel vm)
			{
				vm.InputText = text.TrimEnd('\r', '\n');
				if (vm.SendCommand.CanExecute(null))
					vm.SendCommand.Execute(null);
			}
		}
	}

	private async void OnStreamingMessageUpdated()
	{
		if (!_shouldAutoScroll)
		{
			ScrollToBottom();
		}
	}

	protected override async void OnNavigatingFrom(NavigatingFromEventArgs args)
	{
		base.OnNavigatingFrom(args);

		// Auto-save current chat when navigating away
		await _viewModel.SaveCurrentChatAsync();
	}
}
