﻿using Evanto.Mcp.Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DotNetEnv;

using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using DotNetEnv.Configuration;

namespace Evanto.Mcp.Apps;

public class EvBaseAppHelper
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Loads the configuration from the appsettings.json file. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// <returns>   The RootConfiguration instance with the loaded settings. </returns>
    ///-------------------------------------------------------------------------------------------------
    public T LoadConfiguration<T>() where T : EvBaseAppSettings, new()
    {   // Load .env file from root directory if it exists
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddDotNetEnv(".env", LoadOptions.TraversePath())
            .AddEnvironmentVariables()
            .Build();

        // Bind the configuration to RootConfiguration
        var settings = new T();
        configuration.Bind(settings);

        // Apply environment variable overrides for API keys
        ApplyEnvironmentVariableOverrides(settings);

        if (settings is EvMcpSrvAppSettings srvSettings)
        {   // Manually bind nested sections for proper object creation
            var qdrantSection = configuration.GetSection("Qdrant");
            if (qdrantSection.Exists())
            {
                srvSettings.Qdrant = new EvQdrantSettings();
                qdrantSection.Bind(srvSettings.Qdrant);
            }

            var embeddingSection = configuration.GetSection("Embeddings");
            if (embeddingSection.Exists())
            {
                srvSettings.Embeddings = new EvEmbeddingSettings();
                embeddingSection.Bind(srvSettings.Embeddings);
            }
        }

        return settings;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Loads the configuration from the appsettings.json file and applies command line
    ///             parameters. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// <param name="args"> The command line arguments. </param>
    /// <returns>   The RootConfiguration instance with the loaded settings. </returns>
    ///-------------------------------------------------------------------------------------------------
    public T LoadConfiguration<T>(String[] args) where T : EvHostAppSettings, new()
    {   // check requirements
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        
        var rootConfig = LoadConfiguration<T>();

        // Parse command line parameters and adjust configuration
        rootConfig.ShowThinkNodes  = args.Contains("--think");
        rootConfig.RunTests        = args.Contains("--test") || true;
        rootConfig.EnableTelemetry = args.Contains("--telemetry");

        rootConfig.SetSelectedProvider(GetCommandLineParameter(args, "--provider"));
        rootConfig.SetSelectedModel(GetCommandLineParameter(args, "--model"));

        return rootConfig;
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
    public TracerProvider ConfigureOpenTelemetry(EvHostAppSettings rootConfig, ILogger logger)
    {
        var telemetrySettings = rootConfig.Telemetry;

        logger.LogInformation("🔍 Configuring OpenTelemetry...");
        logger.LogInformation("   Service: {ServiceName} V-{ServiceVersion}", telemetrySettings.ServiceName, telemetrySettings.ServiceVersion);
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

        if (telemetrySettings.EnableConsoleExporter)
        {   // Configure exporters
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
    /// <summary>   Creates a logger for the application. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// 
    /// <param name="rootConfig">   The RootConfiguration instance with the logging settings. </param>
    /// 
    /// <returns>   An ILogger object for the application. </returns>
    ///-------------------------------------------------------------------------------------------------
    public (ILogger, ILoggerFactory) GetLogger(EvBaseAppSettings rootConfig)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(rootConfig, nameof(rootConfig));

        // Create a logger factory
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);

            if (rootConfig.UseConsoleLogging)
            {   // Add console logging
                builder.AddConsole();
            }

            // Add debug logging for VS Code or K8S / Docker environments
            builder.AddDebug();

            if (rootConfig.Telemetry.Enabled && rootConfig.Telemetry.EnableLogging)
            {   // Add OpenTelemetry logging
                ConfigureOpenTelemetryLogging(builder, rootConfig.Telemetry);
            }
        });

        // Return the logger instance
        return (loggerFactory.CreateLogger("Evanto.Mcp"), loggerFactory);
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
    private String? GetCommandLineParameter(String[] args, String parameterName)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        ArgumentNullException.ThrowIfNull(parameterName, nameof(parameterName));

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
    /// <summary>   Configures OpenTelemetry logging. </summary>
    /// 
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// <param name="builder">          The logging builder to configure. </param>
    /// <param name="telemetrySettings"> The telemetry settings from the configuration. </param>
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    private void ConfigureOpenTelemetryLogging(ILoggingBuilder builder, EvTelemetrySettings telemetrySettings)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(telemetrySettings, nameof(telemetrySettings));
        
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
            openTelemetryLoggerOptions.IncludeScopes           = true;
            openTelemetryLoggerOptions.IncludeFormattedMessage = true;
        });
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Applies environment variable overrides for API keys in ChatClients. </summary>
    ///
    /// <remarks>   SvK, 07.01.2025. </remarks>
    ///
    /// <param name="settings"> The settings object to apply overrides to. </param>
    ///-------------------------------------------------------------------------------------------------
    private void ApplyEnvironmentVariableOverrides<T>(T settings) where T : EvBaseAppSettings
    {   // check if chat clients are configured
        settings.ChatClients
            .Where(c => !String.IsNullOrEmpty(c.ProviderName))
            .ToList()
            .ForEach(c => ApplyForProvider(c));

        // also apply to embedding providers if they are configured
        settings.EmbeddingProviders
            .Where(c => !String.IsNullOrEmpty(c.ProviderName))
            .ToList()
            .ForEach(p => ApplyForProvider(p));
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Applies environment variable overrides for a specific chat client provider. </summary>
    ///
    /// <remarks>   SvK, 07.07.2025. </remarks>
    /// 
    /// <param name="chatClient"> The chat client settings to apply overrides to. </param>
    ///-------------------------------------------------------------------------------------------------
    private void ApplyForProvider(EvChatClientSettings chatClient)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(chatClient);

        // Generate environment variable name: {PROVIDER_NAME}_API_KEY
        var envVarName  = $"{chatClient.ProviderName.ToUpperInvariant()}_API_KEY";
        var envVarValue = Environment.GetEnvironmentVariable(envVarName);

        if (!String.IsNullOrEmpty(envVarValue))
        {
            chatClient.ApiKey = envVarValue;
        }
    }

}
