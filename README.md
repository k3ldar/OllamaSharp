# OllamaSharp

A modern, cross-platform chat client for [Ollama](https://ollama.ai/) built with .NET MAUI 10.

## Overview

OllamaSharp is a native chat application that provides an intuitive interface for interacting with local LLM models through Ollama. Built with .NET MAUI, it runs seamlessly on Windows, macOS, iOS, and Android, bringing the power of local AI models to your fingertips.

## Key Features

### 🤖 **AI Chat Interface**
- Real-time streaming responses from Ollama models
- Clean, modern chat UI with user/assistant message bubbles
- Visual typing indicators during AI responses
- Automatic scrolling with smart scroll position management

### 💬 **Conversation Management**
- Save and restore chat conversations
- Multiple chat sessions with unique identifiers
- Persistent chat history across app sessions
- Automatic chat naming and organization

### ⚙️ **Configurable Settings**
- **Server Configuration**: Connect to any Ollama server (local or remote)
- **Model Selection**: Browse and select from available models on your Ollama instance
- **System Role Customization**: Define AI personality and behavior
- **Context Management**: Adjustable conversation history (3-50 message pairs) to balance context vs. performance
- **Real-time Model Discovery**: Automatically fetch available models from your Ollama server

### 🎨 **User Experience**
- Cross-platform native UI using .NET MAUI
- Responsive design optimized for different screen sizes
- Fluent UI icons for a polished look
- Dark/light theme support through MAUI styling
- Stop button to cancel ongoing AI responses

### 🔧 **Technical Features**
- HTTP-based communication with Ollama's REST API
- Streaming response support for real-time feedback
- Conversation history trimming to respect model context limits
- Configurable timeout and cancellation support
- MVVM architecture with CommunityToolkit.Mvvm
- Dependency injection for clean service management

## Architecture

The application follows a clean MVVM pattern with:
- **ViewModels**: `ChatViewModel`, `AppShellViewModel` for UI logic
- **Services**: 
  - `OllamaChatService` - Handles Ollama API communication and streaming
  - `ChatStorageService` - Manages chat persistence to local storage
- **Models**: `ChatMessage`, `SavedChat`, `SavedChatMessage`
- **Views**: Xaml-based UI with `MainPage` (chat), `SettingsPage`, `AboutPage`

## Platform Support

- ✅ **Windows** (10.0.17763.0+)
- ✅ **macOS** (Catalyst 15.0+)
- ✅ **iOS** (15.0+)
- ✅ **Android** (API 21+)

## Requirements

- .NET 10 SDK
- Ollama installed and running (locally or on a remote server)
- Visual Studio 2026 or later (or Visual Studio Code with .NET MAUI extension)

## Getting Started

1. **Install Ollama**: Download and install [Ollama](https://ollama.ai/)
2. **Pull a Model**: Run `ollama pull llama3.2:3b` (or any other model)
3. **Start Ollama Server**: Run `ollama serve` (default: http://localhost:11434)
4. **Launch OllamaSharp**: Build and run the application
5. **Configure Settings**: Go to Settings to set your server URL and select a model
6. **Start Chatting**: Return to the main page and start your conversation!

## Configuration

Default settings can be customized in the Settings page:
- **Server URL**: `http://localhost:11434` (default Ollama endpoint)
- **Model**: `llama3.2:3b` (or any installed model)
- **System Role**: Customize AI personality
- **Context Pairs**: Adjust conversation memory (default: 15 pairs)

Settings are persisted using .NET MAUI Preferences and automatically applied to new chat sessions.

## License

This project is licensed under the GNU GPL v3.0 - see the [LICENSE](LICENSE) file for details.

## Technologies Used

- [.NET MAUI](https://dotnet.microsoft.com/apps/maui) - Cross-platform UI framework
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM helpers
- [Syncfusion.Maui.Toolkit](https://www.syncfusion.com/maui-controls) - Enhanced UI controls
- [Ollama](https://ollama.ai/) - Local LLM runtime

## Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.
