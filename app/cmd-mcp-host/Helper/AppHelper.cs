using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ConsoleMarkdownRenderer;
using Microsoft.Extensions.Logging.Console;
using Evanto.Mcp.Common.Settings;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace Evanto.Mcp.CommandLineHost.Helper;


public static class AppHelper
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Loads the configuration from the appsettings.json file. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// <returns>   The RootConfiguration instance with the loaded settings. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static EvHostSettings LoadConfiguration(String[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var configuration = configurationBuilder
            .AddEnvironmentVariables()
            .Build();

        // Bind the configuration to RootConfiguration
        var rootConfig = configuration.Get<EvHostSettings>() ?? new EvHostSettings();

        // Parse command line parameters and adjust configuration
        rootConfig.ShowThinkNodes = args.Contains("--think");
        rootConfig.RunTests = args.Contains("--test") || true;
        rootConfig.EnableTelemetry = args.Contains("--telemetry");

        rootConfig.SetSelectedProvider(GetCommandLineParameter(args, "--provider"));
        rootConfig.SetSelectedModel(GetCommandLineParameter(args, "--model"));

        return rootConfig;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates a logger for the application. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// <param name="rootConfig">   The RootConfiguration instance with the logging settings. </param>
    /// <returns>   An ILogger object for the application. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static (ILogger, ILoggerFactory) GetLogger(EvHostSettings rootConfig)
    {   // Create a logger factory
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);

            if (rootConfig.UseConsoleLogging)
            {   // Add console logging
                builder.AddConsole().AddConsoleFormatter<ConsoleFormatter, ConsoleFormatterOptions>(options =>
                {
                    options.IncludeScopes = true;         // Include scopes in console output
                    options.TimestampFormat = "HH:mm:ss ";  // Custom timestamp format
                });
            }

            if (rootConfig.Telemetry.Enabled && rootConfig.Telemetry.EnableLogging)
            {   // Add OpenTelemetry logging
                ConfigureOpenTelemetryLogging(builder, rootConfig.Telemetry);
            }
        });

        // Return the logger instance
        return (loggerFactory.CreateLogger("Brunner.Mcp.CommandLineClient"), loggerFactory);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Configures OpenTelemetry tracing for the application. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="rootConfig">   The root configuration containing telemetry settings. </param>
    /// <param name="logger">       The logger for output. </param>
    ///
    /// <returns>   The configured TracerProvider. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static TracerProvider ConfigureOpenTelemetry(EvHostSettings rootConfig, ILogger logger)
    {
        var telemetrySettings = rootConfig.Telemetry;

        logger.LogInformation("🔍 Configuring OpenTelemetry...");
        logger.LogInformation("   Service: {ServiceName} v{ServiceVersion}", telemetrySettings.ServiceName, telemetrySettings.ServiceVersion);
        logger.LogInformation("   OTLP Endpoint: {OtlpEndpoint}", telemetrySettings.OtlpEndpoint);
        logger.LogInformation("   Console Exporter: {EnableConsoleExporter}", telemetrySettings.EnableConsoleExporter);
        logger.LogInformation("   Sampling Ratio: {SamplingRatio}", telemetrySettings.SamplingRatio);
        logger.LogInformation("   Logging Enabled: {EnableLogging}", telemetrySettings.EnableLogging);
        logger.LogInformation("   Log OTLP Exporter: {EnableLogOtlpExporter}", telemetrySettings.EnableLogOtlpExporter);

        var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: telemetrySettings.ServiceName,
                    serviceVersion: telemetrySettings.ServiceVersion))
            .SetSampler(new TraceIdRatioBasedSampler(telemetrySettings.SamplingRatio));

        // Add activity sources
        foreach (var activitySource in telemetrySettings.ActivitySources)
        {
            tracerProviderBuilder.AddSource(activitySource);
        }

        // Configure exporters
        if (telemetrySettings.EnableConsoleExporter)
        {
            tracerProviderBuilder.AddConsoleExporter();
        }

        if (telemetrySettings.EnableOtlpExporter)
        {
            tracerProviderBuilder.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(telemetrySettings.OtlpEndpoint);
                options.Protocol = OtlpExportProtocol.Grpc;
            });
        }

        var tracerProvider = tracerProviderBuilder.Build();

        logger.LogInformation("✅ OpenTelemetry configured successfully");

        return tracerProvider;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests the chat client connection and functionality with MCP tools. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="logger">           The logger instance to extend. </param>
    /// <param name="chatClient">       The chat client to test. </param>
    /// <param name="tools">            The available MCP tools. </param>
    /// <param name="providerName">     (Optional) The name of the chat provider. Default is "Chat Client". </param>
    ///
    /// <returns>   True if the connection test was successful, false otherwise. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task<Boolean> TestChatClientConnectionAsync(ILogger logger, IChatClient chatClient, IList<McpClientTool> tools, String providerName = "Chat Client")
    {
        try
        {   // Test connection with a simple message with timeout
            await logger.LogOutput("🧪 Testing {ProviderName} connection...", providerName);

            var testMessages = new[]
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant."),
                new ChatMessage(ChatRole.User, "Hello! Just say 'Hi' briefly.")
            };

            // Test without tools first with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var stopwatch = Stopwatch.StartNew();
            var testResponse = await chatClient.GetResponseAsync(testMessages, cancellationToken: cts.Token);

            stopwatch.Stop();

            await logger.LogOutput("✅ {ProviderName} connection successful! Test response: {Response}", providerName, testResponse);
            await logger.LogOutput("⏱️ LLM call duration: {Duration}ms", stopwatch.ElapsedMilliseconds);

            // Define the conversation with tools
            var messages = new[]
            {
                new ChatMessage(ChatRole.System, "You are a helpful time assistant. Use the available tools to get accurate time information."),
                new ChatMessage(ChatRole.User, "What is the current time in Europe/Berlin?")
            };

            // Convert MCP tools to AI tools for the chat client
            var chatOptions = new ChatOptions { Tools = [.. tools] };

            // Invoke the chat with MCP tools with longer timeout
            await logger.LogOutput("🧪 Testing {ProviderName} with MCP tools...", providerName);

            using var mainCts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            var toolStopwatch = Stopwatch.StartNew();

            var response = await chatClient.GetResponseAsync(
                messages,
                chatOptions,
                cancellationToken: mainCts.Token
            );

            toolStopwatch.Stop();

            // Test initial conversation
            await logger.LogOutput("🤖 Assistant response ({Duration} ms):", toolStopwatch.ElapsedMilliseconds);

            Console.WriteLine(response);

            return true;
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during {ProviderName} connection test", providerName);
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   
    /// Filters out think nodes (&lt;think&gt;...&lt;/think&gt;) from text.
    /// Think nodes are used by AI models for internal reasoning that should be hidden from users.
    /// This allows models to have internal reasoning that users don't see unless --think is specified.
    /// </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="mText">    The text to extend and filter. </param>
    ///
    /// <returns>   Text with think nodes removed. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static String FilterThinkNodes(this String? mText)
    {
        if (String.IsNullOrEmpty(mText))
            return String.Empty;

        // Use regex to match <think>...</think> tags, including multiline content
        var thinkPattern = @"<think>.*?</think>";
        var filteredText = Regex.Replace(
            mText,
            thinkPattern,
            String.Empty,
            RegexOptions.Singleline | RegexOptions.IgnoreCase
        );

        // Clean up extra whitespace that might be left after removing think nodes
        filteredText = Regex.Replace(filteredText, @"\n\s*\n\s*\n", "\n\n");

        return filteredText.Trim();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Loads system prompt from file and initializes conversation history. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <param name="logger">  The logger instance to extend. </param>
    ///
    /// <returns>   The initialized conversation history with system prompt. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static async Task<IList<ChatMessage>> InitializeConversationHistoryAsync(ILogger logger)
    {
        String systemPrompt;

        try
        {   // Try to load system prompt from file
            systemPrompt = await File.ReadAllTextAsync("system-prompt.txt");

            await logger.LogOutput("✅ System prompt loaded from system-prompt.txt");
        }

        catch (Exception ex)
        {   // Use default prompt if file loading fails
            logger.LogWarning("⚠️ Could not load system-prompt.txt: {ErrorMessage}. Using default prompt.", ex.Message);
            systemPrompt = "You are a helpful AI assistant with access to time and business data tools. Use the available tools to provide accurate, real-time information to users.";
        }

        // Initialize conversation history with system prompt
        var conversationHistory = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, systemPrompt)
        };

        return conversationHistory;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Starts an interactive chat loop with the chat client. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <param name="logger">              The logger. </param>
    /// <param name="chatClient">           The chat client. </param>
    /// <param name="chatClientProvider">   The chat client provider. </param>
    /// <param name="allTools">             All available MCP tools. </param>
    /// <param name="showThinkNodes">       True to show think nodes in output. </param>
    ///
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task StartInteractiveChatLoopAsync(
        ILogger logger,
        IChatClient chatClient,
        IList<McpClientTool> allTools,
        EvHostSettings rootConfig)
    {   // Initialize conversation
        await logger.LogOutput("💬 Starting interactive chat with {0} + MCP tools...", rootConfig.SelectedProvider);
        await logger.LogOutput($"# === Interactive Chat with {rootConfig.SelectedProvider} + MCP ===");
        await logger.LogOutput("Selected provider: **{0}**, selected model: **{1}**.", rootConfig.SelectedProvider, rootConfig.SelectedModel);

        await logger.LogOutput("Type your questions about business data, or type '**exit**' to quit.");

        // Initialize conversation history with system prompt
        var conversationHistory = await InitializeConversationHistoryAsync(logger);

        while (true)
        {   // Chat loop
            Console.Write("You: ");
            var userInput = Console.ReadLine();

            if (String.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
            {
                Console.WriteLine("\n👋 Goodbye!\n");
                break;
            }

            // Add user message to conversation
            conversationHistory.Add(new ChatMessage(ChatRole.User, userInput));

            try
            {
                Console.WriteLine("\n🤖 Thinking...\n");

                // Get the response with timeout and measure duration
                var chatStopwatch = Stopwatch.StartNew();
                using var chatCts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
                var chatResponse = await chatClient.GetResponseAsync(
                    conversationHistory,
                    new ChatOptions { Tools = [.. allTools], Temperature = rootConfig.Temperature, ToolMode = ChatToolMode.Auto },
                    cancellationToken: chatCts.Token
                );

                chatStopwatch.Stop();

                // Filter <think> nodes if --think parameter is not set
                var displayText = rootConfig.ShowThinkNodes ? chatResponse.Text : chatResponse.Text.FilterThinkNodes();

                await logger.LogOutput($"🤖 **Assistant ({chatStopwatch.ElapsedMilliseconds}ms):** ");
                await logger.LogOutput(displayText);

                Console.WriteLine();

                // Add original (unfiltered) assistant response to conversation history for context
                conversationHistory.Add(new ChatMessage(ChatRole.Assistant, chatResponse.Text));
            }

            catch (Exception chatEx)
            {
                logger.LogError(chatEx, "Error during chat interaction");
                Console.WriteLine($"\n❌ Error: {chatEx.Message}\n");
            }
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Shows the available providers and models from the configuration. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <param name="configuration"> The configuration. </param>
    ///-------------------------------------------------------------------------------------------------
    public static void ShowAvailableProviders(EvHostSettings rootConfig)
    {
        try
        {   // Lade Chat-Client-Konfiguration direkt aus der ChatClient Sektion
            Console.WriteLine("🔍 Verfügbare Provider und Modelle:");
            Console.WriteLine($"   Standard-Provider: {rootConfig.DefaultProvider}");
            Console.WriteLine();

            if (rootConfig.Providers.Any())
            {
                foreach (var provider in rootConfig.Providers)
                {
                    var providerName    = provider.ProviderName;
                    var endpoint        = provider.Endpoint;
                    var defaultModel    = provider.DefaultModel;
                    var availableModels = provider.AvailableModels;

                    Console.WriteLine($"   📦 Provider: {providerName}");
                    Console.WriteLine($"      🔗 Endpoint: {endpoint}");
                    Console.WriteLine($"      🎯 Standard-Modell: {defaultModel}");
                    Console.WriteLine($"      🤖 Verfügbare Modelle: {String.Join(", ", availableModels)}");
                    Console.WriteLine();
                }
            }

            else
            {
                Console.WriteLine("   ⚠️ Keine Provider in der Konfiguration gefunden.");
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fehler beim Laden der Konfiguration: {ex.Message}");
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Shows the help for command line parameters. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public static void ShowHelp()
    {
        Console.WriteLine("📖 Brunner MCP Command Line Client - Verfügbare Parameter:");
        Console.WriteLine();
        Console.WriteLine("  --test             Führt MCP Server Tests durch");
        Console.WriteLine("  --think            Zeigt <think>-Knoten in Chat-Antworten an");
        Console.WriteLine("  --telemetry        Aktiviert OpenTelemetry-Überwachung");
        Console.WriteLine("  --provider <name>  Überschreibt den Standard-Provider (z.B. 'OpenAI', 'Ionos')");
        Console.WriteLine("  --model <name>     Überschreibt das Standard-Modell (z.B. 'gpt-4', 'meta-llama/Llama-3.3-70B-Instruct')");
        Console.WriteLine("  --list             Zeigt alle verfügbaren Provider und Modelle an");
        Console.WriteLine("  --help             Zeigt diese Hilfe an");
        Console.WriteLine();
        Console.WriteLine("Beispiele:");
        Console.WriteLine("  dotnet run -- --provider OpenAI --model gpt-4");
        Console.WriteLine("  dotnet run -- --test --provider Ionos");
        Console.WriteLine("  dotnet run -- --think --model o4-mini");
        Console.WriteLine("  dotnet run -- --telemetry --provider OpenAI");
        Console.WriteLine("  dotnet run -- --list");
        Console.WriteLine();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Helper method for extracting command line parameters. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <param name="args">             The command line arguments. </param>
    /// <param name="parameterName">    The name of the parameter (e.g. "--provider"). </param>
    ///
    /// <returns>   The value of the parameter or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static String? GetCommandLineParameter(String[] args, String parameterName)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (String.Equals(args[i], parameterName, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Logs the output message to both the logger and console. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    /// <param name="logger">   The logger instance to extend. </param>
    /// <param name="message">  The message to log and display. </param>
    /// <param name="args">     Optional arguments for formatting the message. </param>
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    // [DebuggerStepThrough]
    public static async Task LogOutput(this ILogger logger, String message, params Object[] args)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(message, args);
        }

        try
        {   // fails, if markdown parsing fails
            if (args.Length > 0)
            {
                message = String.Format(message, args);
            }

            await Displayer.DisplayMarkdownAsync(message, options: new DisplayOptions
            {
            });
        }

        catch (Exception)
        {   // If markdown rendering fails, log the error and display raw response
            Console.WriteLine(message, args);
        }
    }
    
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Configures OpenTelemetry logging. </summary>
    /// 
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// <param name="builder">          The logging builder to configure. </param>
    /// <param name="telemetrySettings"> The telemetry settings from the configuration. </param>
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static void ConfigureOpenTelemetryLogging(ILoggingBuilder builder, EvTelemetrySettings telemetrySettings)
    {
        builder.AddOpenTelemetry(openTelemetryLoggerOptions =>
        {   // Configure OpenTelemetry logging if enabled
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName    : telemetrySettings.ServiceName,
                    serviceVersion : telemetrySettings.ServiceVersion);

            openTelemetryLoggerOptions.SetResourceBuilder(resourceBuilder);

            if (telemetrySettings.EnableLogOtlpExporter)
            {
                openTelemetryLoggerOptions.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(telemetrySettings.OtlpEndpoint);
                    exporter.Protocol = OtlpExportProtocol.Grpc;

                    if (!String.IsNullOrEmpty(telemetrySettings?.Headers))
                    {   // set if necessary
                        exporter.Headers = telemetrySettings.Headers;
                    }
                });
            }

            if (telemetrySettings.EnableLogConsoleExporter)
            {
                openTelemetryLoggerOptions.AddConsoleExporter();
            }

            // Important options to improve data quality
            openTelemetryLoggerOptions.IncludeScopes = true;
            openTelemetryLoggerOptions.IncludeFormattedMessage = true;
        });
    }

}
