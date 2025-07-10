using System;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Evanto.Mcp.Apps.Extensions;

public static class EvAppExtensions
{
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
