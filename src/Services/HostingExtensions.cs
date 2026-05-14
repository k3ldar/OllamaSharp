using System;
using Microsoft.Extensions.DependencyInjection;

namespace OllamaSharp.Services;

public static class HostingExtensions
{
    // Registers a singleton OllamaChatService for Ollama API.
    public static MauiAppBuilder AddOllamaChatService(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<OllamaChatService>(sp =>
        {
            var endpoint = "http://localhost:11434";
            var model = "llama3.2:3b";

            return new OllamaChatService(endpoint, model);
        });

        return builder;
    }
}
