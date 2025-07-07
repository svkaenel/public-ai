using System;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Evanto.Mcp.Apps.Extensions;

public static class EvAppExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Logs the output message to both the logger and console. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    /// 
    /// <param name="logger">   The logger instance to extend. </param>
    /// <param name="message">  The message to log and display. </param>
    /// <param name="args">     Optional arguments for formatting the message. </param>
    /// 
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    // [DebuggerStepThrough]
    public static async Task LogOutput(this ILogger logger, String message, params Object[] args)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(message, args);
        }

        // if necessary also a markdown renderer like MarkDig can be used
        Console.WriteLine(message, args);

        await Task.CompletedTask; // ensure method is async (for e.g. markdown rendering)
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
    public static String FilterThinkNodes(this String? mText)
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
    /// <summary>
    ///     Resolves a relative path against the current working directory.
    /// </summary>
    /// 
    /// <remarks>SvK, 03.06.2025.</remarks>
    /// 
    /// <param name="relativePath"> The relative path to resolve. </param>
    /// <param name="basePath">     Optional base path to resolve against. Defaults to
    ///                             the current working directory if not provided.</param>
    /// 
    /// <returns>   The absolute path resolved from the relative path. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static String ResolveRelative(this String relativePath, String? basePath = null)
    {
        basePath ??= Directory.GetCurrentDirectory();
        return Path.GetFullPath(relativePath, basePath);
    }
}
