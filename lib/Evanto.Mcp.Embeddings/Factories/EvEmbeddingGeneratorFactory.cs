using System;
using System.ClientModel;
using Microsoft.Extensions.Logging;
using OpenAI;
using Azure;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Embeddings.Extensions;
using Azure.AI.Inference;
using OllamaSharp;

namespace Evanto.Mcp.Embeddings.Factories;

public class EvEmbeddingGeneratorFactory(ILoggerFactory loggerFactory)
{
    private readonly ILoggerFactory                         mLoggerFactory  = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    private readonly ILogger<EvEmbeddingGeneratorFactory>   mLogger         = loggerFactory.CreateLogger<EvEmbeddingGeneratorFactory>() ?? throw new ArgumentNullException(nameof(loggerFactory));

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates an instance of the EvEmbeddingGeneratorFactory. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    /// <param name="loggerFactory">    Logger factory for output. </param>
    /// <returns>   A new instance of EvEmbeddingGeneratorFactory. </returns>
    /// <exception cref="ArgumentNullException">    When loggerFactory is null. </exception>
    ///-------------------------------------------------------------------------------------------------
    public static EvEmbeddingGeneratorFactory Create(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        return new EvEmbeddingGeneratorFactory(loggerFactory);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a chat client based on the root configuration. </summary>
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <param name="rootConfig">      The root configuration containing the selected provider and model. </param>
    /// 
    /// <returns>   The configured IEmbeddingGenerator<String, Embedding<Single>>. </returns>
    /// 
    /// <exception cref="ArgumentNullException">    When rootConfig is null. </exception>
    /// <exception cref="InvalidOperationException"> When the selected provider is not found in the configuration. </exception>
    ///-------------------------------------------------------------------------------------------------
    public IEmbeddingGenerator<String, Embedding<Single>> CreateChatClient(EvHostAppSettings rootConfig)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(rootConfig);
        ArgumentNullException.ThrowIfNull(mLogger);

        // Apply overrides
        var providerName = rootConfig.SelectedProvider;
        var modelName    = rootConfig.SelectedModel;
        var settings     = rootConfig.GetChatClient(providerName);

        if (settings == null)
        {
            throw new InvalidOperationException(
                $"Provider '{providerName}' not found in configuration. " +
                $"Available providers: {String.Join(", ", rootConfig.ChatClients?.Where(p => p != null).Select(p => p.ProviderName) ?? Enumerable.Empty<String>())}");
        }

        modelName = modelName ?? settings.DefaultModel;

        return CreateEmbeddingGenerator(settings, modelName, rootConfig.Telemetry);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a embedding client based on the configuration. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <param name="settings">           The provider settings. </param>
    /// <param name="modelName">          Optional. The name of the model to use. </param>
    /// <param name="telemetrySettings">  Optional. The telemetry settings for OpenTelemetry integration. </param>
    /// 
    /// <exception cref="ArgumentNullException">    When configuration is null. </exception>
    /// <exception cref="InvalidOperationException"> When an unknown provider is configured. </exception>
    /// 
    /// <returns>   The configured IEmbeddingGenerator<String, Embedding<Single>>. </returns>
    ///-------------------------------------------------------------------------------------------------
    public IEmbeddingGenerator<String, Embedding<Single>> CreateEmbeddingGenerator(EvChatClientSettings settings, String modelName, EvTelemetrySettings? telemetrySettings = null)
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

        var telemetryEnabled = telemetrySettings?.Enabled ?? false;

        return settings.ProviderName.ToUpperInvariant() switch
        {
            "OPENAI"      => CreateOpenAIEmbeddingGenerator(settings, modelName, telemetryEnabled),
            "IONOS"       => CreateOpenAIEmbeddingGenerator(settings, modelName, telemetryEnabled),
            "LMSTUDIO"    => CreateOpenAIEmbeddingGenerator(settings, modelName, telemetryEnabled),
            "OLLAMA"      => CreateOllamaEmbeddingGenerator(settings, modelName, telemetryEnabled),
            "OLLAMASHARP" => CreateOllamaSharpEmbeddingGenerator(settings, modelName, telemetryEnabled),
            "AZURE"       => CreateAzureEmbeddingGenerator(settings, modelName, telemetryEnabled),
            "AZUREOAI"    => CreateAzureOpenAIEmbeddingGenerator(settings, modelName, telemetryEnabled),
            _             => throw new InvalidOperationException(
                            $"Unknown provider type: {settings.ProviderName}. " +
                            "Supported types: 'OpenAI', 'Ionos', 'LMStudio', 'Ollama', 'Azure', 'AzureOAI'.")
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates an Ollama embedding client. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// <param name="settings">           The provider settings for Ollama. </param>
    /// <param name="embeddingModelName">          The name of the model to use. </param>
    /// <param name="telemetryEnabled">   Optional. True for using OpenTelemetry. </param>
    ///
    /// <returns>   The configured IEmbeddingGenerator<String, Embedding<Single>> for Ollama. </returns>
    /// <exception cref="InvalidOperationException">    When creating the Ollama client fails. </exception>
    ///-------------------------------------------------------------------------------------------------
    private IEmbeddingGenerator<String, Embedding<Single>> CreateOllamaEmbeddingGenerator(EvChatClientSettings settings, String embeddingModelName, Boolean telemetryEnabled = false)
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(embeddingModelName);

            // Create Ollama generator with Microsoft.Extensions.AI.Ollama (deprecated)
            var generator = new OllamaEmbeddingGenerator(
                new Uri(settings.Endpoint),
                embeddingModelName);

            return generator.Build(mLoggerFactory, settings);
        }

        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Ollama chat client: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates an Ollama sharp embedding client. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// <param name="settings">           The provider settings for Ollama. </param>
    /// <param name="embeddingModelName">          The name of the model to use. </param>
    /// <param name="telemetryEnabled">   Optional. True for using OpenTelemetry. </param>
    ///
    /// <returns>   The configured IEmbeddingGenerator<String, Embedding<Single>> for Ollama. </returns>
    /// <exception cref="InvalidOperationException">    When creating the Ollama client fails. </exception>
    ///-------------------------------------------------------------------------------------------------
    private IEmbeddingGenerator<String, Embedding<Single>> CreateOllamaSharpEmbeddingGenerator(EvChatClientSettings settings, String embeddingModelName, Boolean telemetryEnabled = false)
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(embeddingModelName);

            // Create Ollama generator with OllamaSharp
            var generator = new OllamaApiClient(
                new Uri(settings.Endpoint),
                embeddingModelName);

            return generator.Build(mLoggerFactory, settings);
        }

        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Ollama chat client: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a configured OpenAI chat client. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="settings">           The provider settings for OpenAI. </param>
    /// <param name="modelName">          The name of the model to use. </param>
    /// <param name="telemetryEnabled">   Optional. True for using OpenTelemetry. </param>
    ///
    /// <returns>   The configured IEmbeddingGenerator<String, Embedding<Single>>. </returns>
    ///-------------------------------------------------------------------------------------------------
    private IEmbeddingGenerator<String, Embedding<Single>> CreateOpenAIEmbeddingGenerator(EvChatClientSettings settings, String modelName, Boolean telemetryEnabled = false)
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

            var generator = openAiClient
                .GetEmbeddingClient(settings.DefaultModel)
                .AsIEmbeddingGenerator();

            return generator.Build(mLoggerFactory, settings);
        }

        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create OpenAI chat client: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a configured Azure OpenAI chat client. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <param name="settings">           The provider settings for Azure OpenAI. </param>
    /// <param name="modelName">          The name of the model to use. </param>
    /// <param name="telemetryEnabled">   Optional. True for using OpenTelemetry. </param>
    /// 
    /// <returns>   The configured IEmbeddingGenerator<String, Embedding<Single>> for Azure OpenAI. </returns>
    /// <exception cref="InvalidOperationException">    When creating the Azure OpenAI client fails. </exception>
    ///-------------------------------------------------------------------------------------------------
    private IEmbeddingGenerator<String, Embedding<Single>> CreateAzureOpenAIEmbeddingGenerator(EvChatClientSettings settings, String modelName, Boolean telemetryEnabled = false)
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

            var generator = azureOpenAiClient
                .GetEmbeddingClient(settings.DefaultModel)
                .AsIEmbeddingGenerator();

            return generator.Build(mLoggerFactory, settings);
        }

        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Azure OpenAI chat client: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a configured Azure chat client. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// <param name="settings">           The provider settings for Azure Inference. </param>
    /// <param name="modelName">          The name of the model to use. </param>
    /// <param name="telemetrySettings">  Optional. The telemetry settings for OpenTelemetry integration. </param>
    /// 
    /// <returns>   The configured IEmbeddingGenerator<String, Embedding<Single>> for Azure Inference. </returns>
    /// 
    /// <exception cref="InvalidOperationException">    When creating the Azure Inference client fails. </exception>
    ///-------------------------------------------------------------------------------------------------
    private IEmbeddingGenerator<String, Embedding<Single>> CreateAzureEmbeddingGenerator(EvChatClientSettings settings, String modelName, Boolean telemetryEnabled = false)
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(modelName);

            var endpoint   = new Uri(settings.Endpoint);
            var credential = new AzureKeyCredential(settings.ApiKey);

            // ❸ Basisklient des Azure.AI.Inference-SDK
            var inferenceClient = new EmbeddingsClient(
                endpoint,
                credential,
                new AzureAIInferenceClientOptions(
                    AzureAIInferenceClientOptions.ServiceVersion.V2024_05_01_Preview
                )
            );

            var generator = inferenceClient.AsIEmbeddingGenerator();

            return generator.Build(mLoggerFactory, settings);
        }

        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Azure chat client: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests the connection to the embedding client. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="embeddingClient">  The embedding client to test. </param>
    /// <param name="tools">            Available MCP tools. </param>
    /// <param name="logger">           Logger for output. </param>
    /// <param name="cancellationToken"> Cancellation token. </param>
    ///
    /// <returns>   True if the connection is successful. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Boolean> TestConnectionAsync(
        IEmbeddingGenerator<String, Embedding<Single>>  embeddingClient,
        CancellationToken                               cancellationToken = default,
        Int32                                           timeoutSeconds    = 60)
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(embeddingClient);
            ArgumentNullException.ThrowIfNull(mLogger);

            // Test connection with simple message
            mLogger.LogInformation("🧪 Testing {Provider} connection...", embeddingClient.ToString());

            var testMessages = new[]
            {
                new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, "You are a helpful assistant."),
                new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, "Hello! Just say 'Hi' briefly.")
            };

            // Test without tools first with timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            var response = await embeddingClient.GenerateAsync("Test");

            mLogger.LogInformation("✅ {Provider} connection test successful: {Response}",
                embeddingClient.ToString(), String.Join(",", response.Vector));

            return true;
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "❌ {Provider} connection test failed", embeddingClient?.ToString());
            return false;
        }
    }
}
