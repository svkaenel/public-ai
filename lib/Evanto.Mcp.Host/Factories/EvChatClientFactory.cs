using System;
using System.ClientModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using OpenAI;
using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Evanto.Mcp.Common.Settings;

namespace Evanto.Mcp.Host.Factories;

public class EvChatClientFactory(ILoggerFactory loggerFactory)
{
    private readonly ILoggerFactory                 mLoggerFactory  = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    private readonly ILogger<EvChatClientFactory>   mLogger         = loggerFactory.CreateLogger<EvChatClientFactory>();

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates an instance of the ChatClientFactory. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    /// <param name="loggerFactory">    Logger factory for output. </param>
    /// <returns>   A new instance of ChatClientFactory. </returns>
    /// <exception cref="ArgumentNullException">    When loggerFactory is null. </exception>
    ///-------------------------------------------------------------------------------------------------
    public static EvChatClientFactory Create(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        return new EvChatClientFactory(loggerFactory);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a chat client based on the root configuration. </summary>
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <param name="rootConfig">      The root configuration containing the selected provider and model. </param>
    /// 
    /// <returns>   The configured IChatClient. </returns>
    /// 
    /// <exception cref="ArgumentNullException">    When rootConfig is null. </exception>
    /// <exception cref="InvalidOperationException"> When the selected provider is not found in the configuration. </exception>
    ///-------------------------------------------------------------------------------------------------
    public IChatClient CreateChatClient(EvHostSettings rootConfig)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(rootConfig);
        ArgumentNullException.ThrowIfNull(mLogger);

        // Apply overrides
        var providerName = rootConfig.SelectedProvider;
        var modelName    = rootConfig.SelectedModel;
        var settings     = rootConfig.GetProvider(providerName);

        if (settings == null)
        {
            throw new InvalidOperationException(
                $"Provider '{providerName}' not found in configuration. " +
                $"Available providers: {String.Join(", ", rootConfig.Providers?.Where(p => p != null).Select(p => p.ProviderName) ?? Enumerable.Empty<String>())}");
        }

        modelName = modelName ?? settings.DefaultModel;

        return CreateChatClient(settings, modelName);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a chat client based on the configuration. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    /// 
    /// <param name="settings">       The provider settings. </param>
    /// <param name="modelName">      Optional. The name of the model to use. </param>
    /// 
    /// <exception cref="ArgumentNullException">    When configuration is null. </exception>
    /// <exception cref="InvalidOperationException"> When an unknown provider is configured. </exception>
    ///-------------------------------------------------------------------------------------------------
    public IChatClient CreateChatClient(EvChatClientSettings settings, String modelName)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(mLogger);
        ArgumentNullException.ThrowIfNull(modelName);

        if (!String.IsNullOrEmpty(modelName) && (settings.AvailableModels.Length > 0))
        {   // Validate if the model is available
            var isAvailable = Array.Exists(settings.AvailableModels, m => m == modelName);
            if (!isAvailable)
            {
                throw new InvalidOperationException(
                    $"Model '{modelName}' not found in provider configuration for provider {settings.ProviderName}'. " +
                    $"Available models: {String.Join(", ", settings.AvailableModels ?? Enumerable.Empty<String>())}");
            }
        }

        mLogger.LogInformation($"🦙 Configuring {settings.ProviderName} provider:");
        mLogger.LogInformation("   Provider: {ProviderName}", settings.ProviderName);
        mLogger.LogInformation("   Endpoint: {Endpoint}", settings.Endpoint);
        mLogger.LogInformation("   Model: {Model}", modelName);
        mLogger.LogInformation("   Available Models: [{Models}]", String.Join(", ", settings.AvailableModels));

        return settings.ProviderName.ToUpperInvariant() switch
        {
            "OPENAI"   => CreateOpenAIChatClient(settings, modelName),
            "IONOS"    => CreateOpenAIChatClient(settings, modelName),
            "LMSTUDIO" => CreateOpenAIChatClient(settings, modelName),
            "OLLAMA"   => CreateOllamaChatClient(settings, modelName),
            "AZURE"    => CreateAzureChatClient(settings, modelName),
            "AZUREOAI" => CreateAzureOpenAIChatClient(settings, modelName),
            _          => throw new InvalidOperationException(
                            $"Unknown chat client type: {settings.ProviderName}. " +
                            "Supported types: 'OpenAI', 'Ionos', 'LMStudio', 'Ollama', 'Azure', 'AzureOAI'.")
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates an Ollama chat client. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// <param name="settings">   The provider settings for Ollama. </param>
    ///
    /// <returns>   The configured IChatClient for Ollama. </returns>
    /// <exception cref="InvalidOperationException">    When creating the Ollama client fails. </exception>
    ///-------------------------------------------------------------------------------------------------
    private IChatClient CreateOllamaChatClient(EvChatClientSettings settings, String modelName)
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(modelName);

            // Create Ollama client with Microsoft.Extensions.AI.Ollama
            var ollamaClient = new OllamaChatClient(settings.Endpoint, modelName);

            // Wrap with additional features
            var chatClient = new ChatClientBuilder(ollamaClient)
                .UseLogging(mLoggerFactory)
                .UseFunctionInvocation()
                .Build();

            return chatClient;
        }

        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Ollama chat client: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a configured OpenAI chat client. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="loggerFactory">    The logger factory for logging. </param>
    ///
    /// <returns>   The configured IChatClient. </returns>
    ///-------------------------------------------------------------------------------------------------
    private IChatClient CreateOpenAIChatClient(EvChatClientSettings settings, String modelName)
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(modelName);

            // create OpenAI client
            var openAiClient = new OpenAIClient(
                new ApiKeyCredential(settings.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(settings.Endpoint)
                }
            );

            var chatClient = openAiClient.GetChatClient(modelName).AsIChatClient();

            // build chat client with additional features
            var finalChatClient = new ChatClientBuilder(chatClient)
                .UseLogging(mLoggerFactory)
                .UseFunctionInvocation(mLoggerFactory, client =>
                {
                    client.IncludeDetailedErrors = true; // Include detailed errors in function invocation
                })
                .Build();

            return finalChatClient;
        }

        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create OpenAI chat client: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a configured Azure OpenAI chat client. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// 
    /// <param name="settings">       The provider settings for Azure OpenAI. </param>
    /// <param name="modelName">      The name of the model to use. </param>
    /// 
    /// <returns>   The configured IChatClient for Azure OpenAI. </returns>
    /// <exception cref="InvalidOperationException">    When creating the Azure OpenAI client fails. </exception>
    ///-------------------------------------------------------------------------------------------------
    private IChatClient CreateAzureOpenAIChatClient(EvChatClientSettings settings, String modelName)
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(modelName);

            // create Azure OpenAI client
            var azureOpenAiClient = new AzureOpenAIClient(
                new Uri(settings.Endpoint),
                new AzureKeyCredential(settings.ApiKey)
            );

            var chatClient = azureOpenAiClient.GetChatClient(modelName).AsIChatClient();

            // build chat client with additional features
            var finalChatClient = new ChatClientBuilder(chatClient)
                .UseLogging(mLoggerFactory)
                .UseFunctionInvocation()
                .Build();

            return finalChatClient;
        }

        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Azure OpenAI chat client: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a configured Azure chat client. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// <param name="settings">       The provider settings for Azure Inference. </param>
    /// 
    /// <returns>   The configured IChatClient for Azure Inference. </returns>
    /// 
    /// <exception cref="InvalidOperationException">    When creating the Azure Inference client fails. </exception>
    ///-------------------------------------------------------------------------------------------------
    private IChatClient CreateAzureChatClient(EvChatClientSettings settings, String modelName)
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(modelName);

            var endpoint = new Uri(settings.Endpoint);
            var credential = new AzureKeyCredential(settings.ApiKey);

            // ❸ Basisklient des Azure.AI.Inference-SDK
            var inferenceClient = new ChatCompletionsClient(
                endpoint,
                credential,
                new AzureAIInferenceClientOptions(
                    AzureAIInferenceClientOptions.ServiceVersion.V2024_05_01_Preview
                )
            );

            // ❹ In ein IChatClient der Microsoft.Extensions.AI-Welt umwandeln
            var chatClient = inferenceClient.AsIChatClient(modelName);

            // build chat client with additional features
            var finalChatClient = new ChatClientBuilder(chatClient)
                .UseLogging(mLoggerFactory)
                .UseFunctionInvocation()
                .Build();

            return finalChatClient;
        }

        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Azure chat client: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests the connection to the chat client. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="chatClient">       The chat client to test. </param>
    /// <param name="tools">            Available MCP tools. </param>
    /// <param name="logger">           Logger for output. </param>
    /// <param name="cancellationToken"> Cancellation token. </param>
    ///
    /// <returns>   True if the connection is successful. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Boolean> TestConnectionAsync(
        IChatClient chatClient,
        CancellationToken cancellationToken = default,
        Int32 timeoutSeconds = 60)
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(chatClient);
            ArgumentNullException.ThrowIfNull(mLogger);

            // Test connection with simple message
            mLogger.LogInformation("🧪 Testing {Provider} connection...", chatClient.ToString());

            var testMessages = new[]
            {
                new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, "You are a helpful assistant."),
                new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, "Hello! Just say 'Hi' briefly.")
            };

            // Test without tools first with timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            var response = await chatClient.GetResponseAsync(
                testMessages,
                new ChatOptions(),
                cancellationToken: cts.Token
            );

            mLogger.LogInformation("✅ {Provider} connection test successful: {Response}",
                chatClient.ToString(), response.Text?.Trim());

            return true;
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "❌ {Provider} connection test failed", chatClient?.ToString());
            return false;
        }
    }
}
