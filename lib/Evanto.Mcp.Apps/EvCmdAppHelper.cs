using System;
using System.Diagnostics;
using Evanto.Mcp.Apps.Extensions;
using Evanto.Mcp.Common.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace Evanto.Mcp.Apps;

public class EvCmdAppHelper : EvBaseAppHelper
{
    public static EvCmdAppHelper Create { get; } = new EvCmdAppHelper();

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
    public async Task<Boolean> TestChatClientConnectionAsync(ILogger logger, IChatClient chatClient, IList<McpClientTool> tools, String providerName = "Chat Client")
    {
        try
        {   // check requirements
            ArgumentNullException.ThrowIfNull(chatClient, nameof(chatClient));
            ArgumentNullException.ThrowIfNull(tools, nameof(tools));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            // Test connection with a simple message with timeout
            logger.LogInformation("🧪 Testing {ProviderName} connection...", providerName);

            var testMessages = new[]
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant."),
                new ChatMessage(ChatRole.User, "Hello! Just say 'Hi' briefly.")
            };

            // Test without tools first with timeout
            using var cts      = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var   stopwatch    = Stopwatch.StartNew();
            var   testResponse = await chatClient.GetResponseAsync(testMessages, cancellationToken: cts.Token);

            stopwatch.Stop();

            logger.LogInformation("✅ {ProviderName} connection successful! Test response: {Response}", providerName, testResponse);
            logger.LogInformation("⏱️ LLM call duration: {Duration}ms", stopwatch.ElapsedMilliseconds);

            // Define the conversation with tools
            var messages = new[]
            {
                new ChatMessage(ChatRole.System, "You are a helpful time assistant. Use the available tools to get accurate time information."),
                new ChatMessage(ChatRole.User, "What is the current time in Europe/Berlin?")
            };

            // Convert MCP tools to AI tools for the chat client
            var chatOptions = new ChatOptions { Tools = [.. tools] };

            // Invoke the chat with MCP tools with longer timeout
            logger.LogInformation("🧪 Testing {ProviderName} with MCP tools...", providerName);

            using var mainCts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            var toolStopwatch = Stopwatch.StartNew();

            var response = await chatClient.GetResponseAsync(
                messages,
                chatOptions,
                cancellationToken: mainCts.Token
            );

            toolStopwatch.Stop();

            // Test initial conversation
            logger.LogInformation("🤖 Assistant response ({Duration} ms):", toolStopwatch.ElapsedMilliseconds);

            return true;
        }

        catch (Exception ex)
        {
            logger.LogWarning(ex, "❌ Error during {ProviderName} connection test.", providerName);
            return false;
        }
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
    private async Task<IList<ChatMessage>> InitializeConversationHistoryAsync(ILogger logger)
    {
        String systemPrompt;

        try
        {   // Try to load system prompt from file
            systemPrompt = await File.ReadAllTextAsync("system-prompt.txt");

            logger.LogInformation("✅ System prompt loaded from system-prompt.txt");
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
    public async Task StartInteractiveChatLoopAsync(
        ILogger              logger,
        IChatClient          chatClient,
        IList<McpClientTool> allTools,
        EvHostAppSettings    rootConfig)
    {   // Initialize conversation
        logger.LogInformation("Starting interactive chat with + MCP tools...");
        logger.LogInformation("Selected provider '{provider}', selected model: '{model}'.", rootConfig.SelectedProvider, rootConfig.SelectedModel);

        Console.WriteLine($"=== Interactive Chat with {rootConfig.SelectedProvider} and model {rootConfig.SelectedModel} + MCP ===");
        Console.WriteLine("💬 Type your questions about 'SupportWizard' data, or type 'exit' to quit.");

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
                var   chatStopwatch = Stopwatch.StartNew();
                using var chatCts   = new CancellationTokenSource(TimeSpan.FromSeconds(180));
                var   chatResponse  = await chatClient.GetResponseAsync(
                    conversationHistory,
                    new ChatOptions { Tools = [.. allTools], Temperature = rootConfig.Temperature, ToolMode = ChatToolMode.Auto },
                    cancellationToken: chatCts.Token
                );

                chatStopwatch.Stop();

                // Filter <think> nodes if --think parameter is not set
                var displayText = rootConfig.ShowThinkNodes ? chatResponse.Text : chatResponse.Text.FilterThinkNodes();

                Console.WriteLine($"🤖 Assistant ({chatStopwatch.ElapsedMilliseconds}ms): ");
                Console.WriteLine(displayText);

                Console.WriteLine();

                // Add original (unfiltered) assistant response to conversation history for context
                conversationHistory.Add(new ChatMessage(ChatRole.Assistant, chatResponse.Text));
            }

            catch (Exception chatEx)
            {
                logger.LogError(chatEx, "Error during chat interaction");
                Console.Error.WriteLine($"\n❌ Error in chat loop with assistant: {chatEx.Message}\n");
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
    public void ShowAvailableProviders(EvHostAppSettings rootConfig)
    {
        try
        {   // Load Chat-Client configuration directly from the ChatClient section
            Console.WriteLine("🔍 Available Providers and Models:");
            Console.WriteLine($"   Default Provider: {rootConfig.DefaultChatClient}");
            Console.WriteLine();

            if (rootConfig.ChatClients.Any())
            {
                foreach (var provider in rootConfig.ChatClients)
                {
                    var providerName    = provider.ProviderName;
                    var endpoint        = provider.Endpoint;
                    var defaultModel    = provider.DefaultModel;
                    var availableModels = provider.AvailableModels;

                    Console.WriteLine($"   📦 Provider: {providerName}");
                    Console.WriteLine($"      🔗 Endpoint: {endpoint}");
                    Console.WriteLine($"      🎯 Default Model: {defaultModel}");
                    Console.WriteLine($"      🤖 Available Models: {String.Join(", ", availableModels)}");
                    Console.WriteLine();
                }
            }

            else
            {
                Console.WriteLine("   ⚠️ No providers found in configuration.");
            }
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Error loading configuration: {ex.Message}");
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Shows the help for command line parameters. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public void ShowHelp()
    {
        Console.WriteLine("📖 Evanto MCP Command Line Client - Available Parameters:");
        Console.WriteLine();
        Console.WriteLine("  --test             Performs MCP Server tests");
        Console.WriteLine("  --think            Shows <think> nodes in chat responses");
        Console.WriteLine("  --telemetry        Activates OpenTelemetry monitoring");
        Console.WriteLine("  --provider <name>  Overrides the default provider (e.g. 'OpenAI', 'Ionos')");
        Console.WriteLine("  --model <name>     Overrides the default model (e.g. 'gpt-4', 'meta-llama/Llama-3.3-70B-Instruct')");
        Console.WriteLine("  --list             Shows all available providers and models");
        Console.WriteLine("  --help             Shows this help");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run -- --provider OpenAI --model gpt-4");
        Console.WriteLine("  dotnet run -- --test --provider Ionos");
        Console.WriteLine("  dotnet run -- --think --model o4-mini");
        Console.WriteLine("  dotnet run -- --telemetry --provider OpenAI");
        Console.WriteLine("  dotnet run -- --list");
        Console.WriteLine();
    }

}
