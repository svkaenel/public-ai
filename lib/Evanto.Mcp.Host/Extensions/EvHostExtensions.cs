using System;
using Evanto.Mcp.Common.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Evanto.Mcp.Host.Extensions;

public static class EvHostExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Builds an embedding generator with distributed cache and OpenTelemetry support. </summary>
    /// /// 
    /// <remarks>   SvK, 07.07.2025. </remarks>
    /// 
    /// <param name="chatClient"> The embedding generator to enhance. </param>
    /// <param name="loggerFactory"> The logger factory to use for logging. </param>
    /// <param name="settings">  The embedding settings to use. </param>
    /// <param name="telemetryEnabled">   Whether to enable OpenTelemetry support. Default is true. </param>
    ///  
    /// <returns>   An IEmbeddingGenerator with distributed cache and OpenTelemetry support. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IChatClient Build(this IChatClient chatClient, ILoggerFactory loggerFactory, EvChatClientSettings settings, Boolean telemetryEnabled = true)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(chatClient, "Chat client must be valid!");
        ArgumentNullException.ThrowIfNull(loggerFactory, "Logger factory must be valid!");
        ArgumentNullException.ThrowIfNull(settings, "Embedding settings must be valid!");

        var chatClientBuilder = new ChatClientBuilder(chatClient)
            .UseLogging(loggerFactory)
            .UseDistributedCache(
                new MemoryDistributedCache(
                    Options.Create(new MemoryDistributedCacheOptions())
                )
            );

        if (telemetryEnabled)
        {   // Add OpenTelemetry if enabled
            chatClientBuilder = chatClientBuilder.UseOpenTelemetry(sourceName: $"{settings.ProviderName}-embeddings");
        }

        return chatClientBuilder
            .UseFunctionInvocation(loggerFactory, client =>
            {   // Include detailed errors in function invocation
                client.IncludeDetailedErrors = true; 
            })
            .Build();
    }

 }
