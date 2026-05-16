
namespace OllamaSharp
{
    internal static class Constants
    {
        internal const string DefaultSystemBehaviour = "You are lord of the universe and treat everyone like a servant, but still helpful at answering questions";
        internal const string DefaultModel = "llama3.2:3b";
        internal const string DefaultOllamaUrl = "http://localhost:11434";
        internal const string DefaultSaveChatFileSearch = "*.json";
        internal const string DefaultOllamaChatEndpoint = "api/chat";
        internal const string DefaultTextCutoffIndicator = "...";

        internal const string FontFileNameOpenSansRegular = "OpenSans-Regular.ttf";
        internal const string FontNameOpenSansRegular = "OpenSansRegular";
        internal const string FontFileNameFluentSystemsIcons = "FluentSystemIcons-Regular.ttf";
        internal const string FontFileNameOpenSansSemiBold = "OpenSans-Semibold.ttf";
        internal const string FontNameOpenSansSemiBold = "OpenSansSemibold";

        internal const string Error = nameof(Error);
        internal const string ErrAppViewModelNotRegistered = "AppShellViewModel not registered";

        internal const string UriGithubRepo = "https://github.com/k3ldar/OllamaSharp";
        internal const string UriGithubIssues = "https://github.com/k3ldar/OllamaSharp/issues";

        internal const string MessageTypeUser = "user";
        internal const string MessageTypeAssistant = "assistant";
        internal const string MessageRoleTypeSystem = "system";

        internal const string PageMain = "//MainPage";
        internal const string PageAbout = "//AboutPage";
        internal const string PageChat = "//ChatPage";
        internal const string PageSettings = "//SettingsPage";

        internal const string DialogButtonTextOk = "OK";
        internal const string DialogButtonTextCancel = "Cancel";
        internal const string DialogButtonDelete = "Delete";

        internal const string RecentChatFolderName = "Chats";

        internal const string StringSpace = " ";

        internal const string DebugMsgWindowPosOutsideOfBounds = "Saved window position is outside screen bounds. Using default position.";
        internal const string DebugMsgNoMessagesToSave = "No messages to save for current chat.";
        internal const string DebugMsgNewChatStarted = "Started new chat";
        internal const string DebugMsgChatParameterNull = "Chat parameter is null";

        internal const char CharForwardSlash = '/';
        internal const char CharLineFeed = '\n';
        internal const char CharCarriageReturn = '\r';
        internal const char CharTab = '\t';
        internal const char CharSpace = ' ';

        internal const int DefaultMaxHistoryPairs = 15;
        internal const int DefaultTimeOutInMinutes = 3;
        internal const int MaximumTitleLength = 50;
    }
}
