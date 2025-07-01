using System;
using ConsoleMarkdownRenderer;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Evanto.Mcp.Apps.Extensions;

public static class EvAppExtensions
{
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
}
