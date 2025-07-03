using Evanto.Mcp.Vectorize.Contracts;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Logging;
using System.Text; // Required for StringBuilder

namespace Evanto.Mcp.Vectorize.Services;

public class EvPdfTextExtractor(ILogger<EvPdfTextExtractor> logger) : IEvPdfTextExtractor
{
    private readonly ILogger<EvPdfTextExtractor> mLogger = logger;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Extract text asynchronously from PDF file. </summary>
    ///
    /// <remarks>   SvK, 26.05.2025. </remarks>
    ///
    /// <param name="filePath">   Full pathname of the PDF file. </param>
    ///
    /// <returns>   A Task&lt;String&gt; containing the extracted text. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<String> ExtractTextAsync(String filePath)
    {   // extract text from PDF using iText library
        try
        {
            return await Task.Run(() =>
            {   // open PDF document
                using var reader   = new PdfReader(filePath);
                using var document = new PdfDocument(reader);
                
                var text = new StringBuilder();
                
                // extract text from each page
                for (Int32 i = 1; i <= document.GetNumberOfPages(); i++)
                {
                    var page = document.GetPage(i);
                    
                    // Use LocationTextExtractionStrategy for better text extraction
                    var strategy = new LocationTextExtractionStrategy();
                    
                    // Extract text from page using iText's PdfTextExtractor
                    var pageText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page, strategy);
                    
                    text.AppendLine(pageText);
                }
                
                return text.ToString();
            });
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to extract text from PDF: {FilePath}", filePath);
            throw; // Re-throw to be handled by caller
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Chunk text into smaller pieces with overlap. </summary>
    ///
    /// <remarks>   SvK, 26.05.2025. </remarks>
    ///
    /// <param name="text">       The text to chunk. </param>
    /// <param name="chunkSize">  Size of each chunk. </param>
    /// <param name="overlap">    The overlap between chunks. </param>
    ///
    /// <returns>   A List&lt;String&gt; containing the text chunks. </returns>
    ///-------------------------------------------------------------------------------------------------
    public List<String> ChunkText(String text, Int32 chunkSize, Int32 overlap)
    {   // chunk text into smaller pieces for better embedding processing
        var chunks = new List<String>();
        
        if (String.IsNullOrWhiteSpace(text))
            return chunks; // Return empty list for null or empty text

        // Clean up the text first
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");
        
        // Split by sentences for better chunking
        var sentences = text.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries)
                           .Select(s => s.Trim())
                           .Where(s => !String.IsNullOrEmpty(s))
                           .ToList();

        if (!sentences.Any())
        {   // fallback: split by paragraphs if no sentences found
            var paragraphs = text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim())
                                .Where(p => !String.IsNullOrEmpty(p))
                                .ToList();
            
            if (!paragraphs.Any())
            {   // ultimate fallback: character-based chunking
                return CreateCharacterBasedChunks(text, chunkSize, overlap);
            }
            
            sentences = paragraphs;
        }

        var currentChunk     = new StringBuilder();
        var currentChunkSize = 0;

        foreach (var sentence in sentences)
        {
            var sentenceLength = sentence.Length + 1; // +1 for space/punctuation
            
            // Check if adding this sentence would exceed chunk size
            if (currentChunkSize + sentenceLength > chunkSize && currentChunk.Length > 0)
            {   // finalize current chunk
                chunks.Add(currentChunk.ToString().Trim());
                
                // start new chunk with overlap
                if (overlap > 0)
                {
                    var overlapText = GetOverlapText(currentChunk.ToString(), overlap);
                    currentChunk     = new StringBuilder(overlapText);
                    currentChunkSize = overlapText.Length;
                }

                else
                {
                    currentChunk.Clear();
                    currentChunkSize = 0;
                }
            }
            
            // Add sentence to current chunk
            if (currentChunk.Length > 0)
            {
                currentChunk.Append(" ");
                currentChunkSize++;
            }

            currentChunk.Append(sentence);
            currentChunkSize += sentence.Length;
        }

        // Add final chunk if not empty
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }

    /// <summary>
    /// Creates character-based chunks as a fallback method.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <param name="chunkSize">Size of each chunk.</param>
    /// <param name="overlap">The overlap between chunks.</param>
    /// <returns>A list of text chunks.</returns>
    private static List<String> CreateCharacterBasedChunks(String text, Int32 chunkSize, Int32 overlap)
    {   // fallback method for character-based chunking
        var chunks = new List<String>();
        var start  = 0;

        while (start < text.Length)
        {
            var end    = Math.Min(start + chunkSize, text.Length);
            var chunk  = text.Substring(start, end - start);
            
            chunks.Add(chunk);
            
            start = Math.Max(start + chunkSize - overlap, start + 1); // Ensure progress
        }

        return chunks;
    }

    /// <summary>
    /// Gets overlap text from the end of the current chunk.
    /// </summary>
    /// <param name="text">The text to get overlap from.</param>
    /// <param name="overlapSize">The size of the overlap.</param>
    /// <returns>The overlap text.</returns>
    private static String GetOverlapText(String text, Int32 overlapSize)
    {   // get overlap text from the end of current chunk
        if (text.Length <= overlapSize)
            return text; // Return entire text if shorter than overlap
            
        return text.Substring(text.Length - overlapSize);
    }
}
