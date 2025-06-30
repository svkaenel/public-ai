///-------------------------------------------------------------------------------------------------
/// <summary>   MCP server testing functionality. </summary>
///
/// <remarks>   SvK, 23.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.Text.Json;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Host.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace Evanto.Mcp.Host.Tests;

///-------------------------------------------------------------------------------------------------
/// <summary>   Service for testing MCP servers and their tools. </summary>
///
/// <remarks>   SvK, 23.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvMcpServerTester(ILogger logger)
{
    private readonly ILogger mLogger = logger;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests an entire MCP server including all configured tools. </summary>
    ///
    /// <param name="mcpClientInfo">    Information about the MCP client. </param>
    /// <param name="toolTests">        List of tool test configurations. </param>
    /// <param name="quickTest">        If true, only test the first tool. </param>
    ///
    /// <returns>   The test result. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvMcpTestResult> TestServerAsync(
        EvMcpClientInfo                   mcpClientInfo,
        IList<EvMcpToolTestSettings> toolTests,
        Boolean                         quickTest = false)
    {
        var serverStopwatch = Stopwatch.StartNew();
        var result          = new EvMcpTestResult
        {
            ServerName      = mcpClientInfo.Name ?? "Unknown Server"
        };

        try
        {   // protections
            mLogger.LogInformation("🧪 Testing Server: {ServerName}", result.ServerName);

            if (mcpClientInfo.Client == null)
            {
                result.ErrorMessage = "MCP client is null";
                result.Success      = false;

                return result;
            }

            var availableTools = mcpClientInfo.Tools ?? new List<McpClientTool>();

            if (!availableTools.Any())
            {
                mLogger.LogWarning("⚠️ No tools available for server {ServerName}", result.ServerName);

                result.ErrorMessage = "No tools available";
                result.Success      = false;

                return result;
            }

            // Determine which tools to test
            var toolsToTest = DetermineToolsToTest(availableTools, toolTests, quickTest);

            foreach (var tool in toolsToTest)
            {   // get test configuration for this tool
                var testConfig      = toolTests.FirstOrDefault(t => t.ToolName == tool.Name && t.Enabled);
                var timeoutSeconds  = testConfig?.TimeoutSeconds ?? 30;

                var toolResult      = await TestToolAsync(mcpClientInfo.Client, tool, testConfig, timeoutSeconds);

                result.ToolResults.Add(toolResult);
            }

            result.Success = result.ToolResults.Any() && result.ToolResults.All(t => t.Success);
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "❌ Error testing server {ServerName}", result.ServerName);

            result.ErrorMessage = ex.Message;
            result.Success = false;
        }

        finally
        {
            serverStopwatch.Stop();

            result.TotalDuration = serverStopwatch.Elapsed;
        }

        return result;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests a single MCP tool. </summary>
    ///
    /// <param name="client">           The MCP client. </param>
    /// <param name="tool">             The tool to test. </param>
    /// <param name="testConfig">       The test configuration (optional). </param>
    /// <param name="timeoutSeconds">   The timeout in seconds. </param>
    ///
    /// <returns>   The tool test result. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvMcpToolTestResult> TestToolAsync(
        IMcpClient                  client,
        McpClientTool               tool,
        EvMcpToolTestSettings?        testConfig,
        Int32                       timeoutSeconds)
    {
        var toolStopwatch   = Stopwatch.StartNew();
        var result          = new EvMcpToolTestResult
        {
            ToolName        = tool.Name ?? "Unknown Tool"
        };

        try
        {
            mLogger.LogInformation("📋 Testing Tool: {ToolName}", result.ToolName);

            // Determine parameters to use
            var parameters          = testConfig?.TestParameters ?? GenerateDefaultParameters(tool);
            result.UsedParameters   = parameters;

            // Create cancellation token with timeout
            using var cts           = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            // Call the tool
            var callResult          = await client.CallToolAsync(tool.Name ?? "Unknown", parameters);

            if (callResult.IsError)
            {
                result.Success      = false;
                result.ErrorMessage = "Error calling tool: " + result.ToolName;

                mLogger.LogWarning("⚠️ Tool {ToolName} returned an error", result.ToolName);
            }

            else
            {
                result.Success  = true;
                result.Response = "With V-0.3.0 not extractable" ?? "No response";

                if (!callResult.Content.Any())
                {
                    result.ErrorMessage = "Tool returned no content";

                    mLogger.LogWarning("⚠️ Tool {ToolName} returned no content", result.ToolName);
                }

                mLogger.LogInformation("✅ Tool {ToolName} executed successfully", result.ToolName);
            }
        }

        catch (OperationCanceledException)
        {
            result.Success      = false;
            result.ErrorMessage = $"Tool execution timed out after {timeoutSeconds} seconds";

            mLogger.LogWarning("⏰ Tool {ToolName} timed out", result.ToolName);
        }

        catch (Exception ex)
        {
            result.Success      = false;
            result.ErrorMessage = ex.Message;

            mLogger.LogError(ex, "❌ Error testing tool {ToolName}", result.ToolName);
        }

        finally
        {
            toolStopwatch.Stop();

            result.Duration = toolStopwatch.Elapsed;
        }

        return result;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Generates default parameters for a tool based on its schema. </summary>
    ///
    /// <param name="tool"> The tool. </param>
    ///
    /// <returns>   Dictionary of default parameters. </returns>
    ///-------------------------------------------------------------------------------------------------
    private Dictionary<String, Object?> GenerateDefaultParameters(McpClientTool tool)
    {
        var parameters = new Dictionary<String, Object?>();

        try
        {   // Deserialize the JSON schema
            var schema = tool.JsonSchema.Deserialize<EvMcpToolJsonScheme>();

            if (schema?.Properties != null)
            {
                var requiredProperties = new HashSet<String>();

                // Get required properties
                if (schema.Required != null)
                {
                    foreach (var required in schema.Required)
                    {
                        requiredProperties.Add(required);
                    }
                }

                // Generate values only for required properties
                foreach (var property in schema.Properties)
                {
                    if (requiredProperties.Contains(property.Key))
                    {
                        var value = GenerateDefaultValueFromPropertyDefinition(property.Key, property.Value);
                        parameters[property.Key] = value;
                    }
                }
            }
        }

        catch (Exception ex)
        {
            mLogger.LogWarning("⚠️ Error generating default parameters for tool {ToolName}: {Error}",
                tool.Name, ex.Message);
        }

        return parameters;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Generates a default value based on parameter name and PropertyDefinition. </summary>
    ///
    /// <param name="parameterName">    Name of the parameter. </param>
    /// <param name="propertyDef">      The property definition. </param>
    ///
    /// <returns>   The generated default value. </returns>
    ///-------------------------------------------------------------------------------------------------
    private Object? GenerateDefaultValueFromPropertyDefinition(String parameterName, EvJsonPropertyDefinition propertyDef)
    {
        try
        {
            var typeName = propertyDef.Type?.ToLowerInvariant();

            return typeName switch
            {
                "string" => GenerateStringValue(parameterName),
                "integer" => 0,
                "number" => 0.0,
                "boolean" => false,
                "array" => new Object[0],
                "object" => null,
                _ => GenerateStringValue(parameterName) // Fallback to string
            };
        }

        catch (Exception ex)
        {
            mLogger.LogWarning("⚠️ Error generating value for parameter {ParameterName}: {Error}",
                parameterName, ex.Message);

            return null;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Generates a default value based on parameter name and JSON schema. </summary>
    ///
    /// <param name="parameterName">    Name of the parameter. </param>
    /// <param name="schemaElement">    The JSON schema element. </param>
    ///
    /// <returns>   The generated default value. </returns>
    ///-------------------------------------------------------------------------------------------------
    private Object? GenerateDefaultValue(String parameterName, JsonElement schemaElement)
    {
        try
        {   // Try to get the type from schema
            if (schemaElement.TryGetProperty("type", out var typeElement))
            {
                var typeName = typeElement.GetString();

                return typeName?.ToLowerInvariant() switch
                {
                    "string" => GenerateStringValue(parameterName),
                    "integer" => 0,
                    "number" => 0.0,
                    "boolean" => false,
                    "array" => new Object[0],
                    "object" => null,
                    _ => null
                };
            }

            // Fallback: generate based on parameter name
            return GenerateStringValue(parameterName);
        }

        catch (Exception ex)
        {
            mLogger.LogWarning("⚠️ Error generating value for parameter {ParameterName}: {Error}",
                parameterName, ex.Message);
            return null;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Generates a context-appropriate string value based on parameter name. </summary>
    ///
    /// <param name="parameterName">    Name of the parameter. </param>
    ///
    /// <returns>   A context-appropriate string value. </returns>
    ///-------------------------------------------------------------------------------------------------
    private String GenerateStringValue(String parameterName)
    {
        var lowerName = parameterName.ToLowerInvariant();

        return lowerName switch
        {
            var name when name.Contains("email") || name.Contains("mail") => "test@example.com",
            var name when name.Contains("name") || name.Contains("kunde") => "Test User",
            var name when name.Contains("nummer") || name.Contains("id") => "12345",
            var name when name.Contains("url") || name.Contains("link") => "https://example.com",
            var name when name.Contains("phone") || name.Contains("telefon") => "+49 123 456789",
            var name when name.Contains("address") || name.Contains("adresse") => "Teststraße 123",
            var name when name.Contains("city") || name.Contains("stadt") => "Teststadt",
            var name when name.Contains("country") || name.Contains("land") => "Deutschland",
            var name when name.Contains("date") || name.Contains("datum") => DateTime.Now.ToString("yyyy-MM-dd"),
            _ => "Test"
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Determines which tools to test based on configuration and mode. </summary>
    ///
    /// <param name="availableTools">   The available tools. </param>
    /// <param name="toolTests">        The tool test configurations. </param>
    /// <param name="quickTest">        True for quick test mode. </param>
    ///
    /// <returns>   List of tools to test. </returns>
    ///-------------------------------------------------------------------------------------------------
    private IList<McpClientTool> DetermineToolsToTest(
        IList<McpClientTool>            availableTools,
        IList<EvMcpToolTestSettings> toolTests,
        Boolean                         quickTest)
    {
        var result = availableTools.Where(t => ShouldBeTested(t, toolTests));

        return quickTest ? result.Take(1).ToList() : result.ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Determines if a tool should be tested based on its configuration. </summary>
    ///
    /// <param name="tool">            The tool to check. </param>
    /// <param name="toolTests">       The list of tool test configurations. </param>
    ///
    /// <returns>   True if the tool should be tested, false otherwise. </returns>
    ///-------------------------------------------------------------------------------------------------
    private Boolean ShouldBeTested(McpClientTool tool, IList<EvMcpToolTestSettings> toolTests)
    {   // Check if the tool has a test configuration
        var testConfig = toolTests.FirstOrDefault(t => (t.ToolName == tool.Name) && t.Enabled);
        // If no tests configured, test all tools
        return (testConfig != null) || (toolTests.Count == 0);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests all configured MCP servers using their tool test configurations. </summary>
    ///
    /// <param name="logger">           The logger instance. </param>
    /// <param name="rootConfig">       The configuration containing server settings. </param>
    /// <param name="mcpClients">       List of MCP client instances. </param>
    ///
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task TestAllMcpServersAsync(
        ILogger                 logger,
        EvHostSettings       rootConfig,
        IList<EvMcpClientInfo>    mcpClients)
    {
        try
        {
            logger.LogInformation("🧪 Starting MCP Server Tests...");
            
            var tester          = new EvMcpServerTester(logger);

            var overallSuccess  = true;
            var totalDuration   = TimeSpan.Zero;

            foreach (var mcpClientInfo in mcpClients)
            {
                var serverConfig = rootConfig.McpServers?.FirstOrDefault(s => s.Name == mcpClientInfo.Name);
                var toolTests    = serverConfig?.ToolTests ?? new List<EvMcpToolTestSettings>();

                var result       = await tester.TestServerAsync(mcpClientInfo, toolTests, rootConfig.QuickTests);
                totalDuration    = totalDuration.Add(result.TotalDuration);
                
                if (!result.Success)
                {
                    overallSuccess = false;
                }

                PrintTestResults(logger, result);
            }

            logger.LogInformation("🏁 Overall Test Results: {Status} | Total Duration: {Duration}ms",
                overallSuccess ? "✅ All Passed" : "❌ Some Failed",
                (Int32) totalDuration.TotalMilliseconds);
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during MCP server testing");
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Prints the test results to the console in a formatted way. </summary>
    ///
    /// <param name="logger">   The logger instance. </param>
    /// <param name="result">   The test result to print. </param>
    ///-------------------------------------------------------------------------------------------------
    private static void PrintTestResults(ILogger logger, EvMcpTestResult result)
    {
        logger.LogInformation("🧪 Testing Server: {ServerName}", result.ServerName);
        logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        if (!String.IsNullOrEmpty(result.ErrorMessage))
        {
            logger.LogError("❌ Server Error: {Error}", result.ErrorMessage);
            return;
        }

        foreach (var toolResult in result.ToolResults)
        {
            var parametersJson = JsonSerializer.Serialize(
                toolResult.UsedParameters, 
                new JsonSerializerOptions { WriteIndented = false });

            logger.LogInformation("📋 Tool: {ToolName}", toolResult.ToolName);
            logger.LogInformation("   Parameters: {Parameters}", parametersJson);
            
            if (toolResult.Success)
            {
                logger.LogInformation("   ✅ Success | Duration: {Duration}ms", 
                    (Int32) toolResult.Duration.TotalMilliseconds);
                
                if (!String.IsNullOrEmpty(toolResult.Response) || false)
                {   // in V-0.3.0-prerelease.1 the response is not extractable
                    var truncatedResponse = toolResult.Response.Length > 100 
                        ? toolResult.Response.Substring(0, 100) + "..." 
                        : toolResult.Response;

                    logger.LogInformation("   Response: {Response}", truncatedResponse);
                }
            }

            else
            {
                logger.LogWarning("   ❌ Failed | Duration: {Duration}ms | Error: {Error}",
                    (Int32)toolResult.Duration.TotalMilliseconds,
                    toolResult.ErrorMessage ?? "Unknown error");
            }
            
            logger.LogInformation("");
        }

        logger.LogInformation("Server Test Summary: {Successful}/{Total} tools passed | Total Duration: {Duration}ms",
            result.SuccessfulTests, result.TotalTests, (Int32) result.TotalDuration.TotalMilliseconds);
        logger.LogInformation("");
    }

}
