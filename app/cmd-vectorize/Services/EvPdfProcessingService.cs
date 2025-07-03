using Microsoft.Extensions.Logging;
using Evanto.Mcp.Vectorize.Contracts;
using Evanto.Mcp.Embeddings.Contracts;
using Evanto.Mcp.Vectorize.Settings;
using Evanto.Mcp.Vectorize.Models;

namespace Evanto.Mcp.Vectorize.Services;

public class EvPdfProcessingService(
    IEvPdfTextExtractor               textExtractor,
    IEvEmbeddingService               embeddingService,
    IEvVectorStoreService             vectorStoreService,
    IEvFileTrackingService            fileTrackingService,
    EvVectorizeAppSettings          settings,
    ILogger<EvPdfProcessingService>   logger) : IEvPdfProcessingService
{
    private readonly IEvPdfTextExtractor       mTextExtractor         = textExtractor;
    private readonly IEvEmbeddingService       mEmbeddingService      = embeddingService;
    private readonly IEvVectorStoreService     mVectorStoreService    = vectorStoreService;
    private readonly IEvFileTrackingService    mFileTrackingService   = fileTrackingService;
    private readonly EvVectorizeAppSettings        mSettings                = settings;
    private readonly ILogger<EvPdfProcessingService> mLogger          = logger;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    /// Processes all new PDF files found in the configured directory.
    /// </summary>
    /// <returns>A <see cref="EvProcessingResult"/> summarizing the outcome of the processing.</returns>
    ///-------------------------------------------------------------------------------------------------

    public async Task<EvProcessingResult> ProcessNewPdfsAsync()
    {
        var result = new EvProcessingResult();

        try
        {
            // Initialize vector store
            await mVectorStoreService.InitializeAsync();

            // Ensure PDF directory exists
            if (!Directory.Exists(mSettings.PdfDirectory))
            {
                mLogger.LogWarning("PDF directory does not exist: {Directory}", mSettings.PdfDirectory);
                Directory.CreateDirectory(mSettings.PdfDirectory);
                return result; // Return empty result if directory had to be created
            }

            // Get all PDF files
            var pdfFiles = Directory.GetFiles(mSettings.PdfDirectory, "*.pdf", SearchOption.TopDirectoryOnly);

            if (!pdfFiles.Any())
            {
                mLogger.LogInformation("No PDF files found in directory: {Directory}", mSettings.PdfDirectory);
                return result; // Return empty result if no files found
            }

            mLogger.LogInformation("Found {Count} PDF files in directory", pdfFiles.Length);

            // Process each file
            foreach (var filePath in pdfFiles)
            {
                try
                {
                    var fileResult = await ProcessSpecificPdfAsync(filePath);
                    result.ProcessedCount += fileResult.ProcessedCount;
                    result.SkippedCount += fileResult.SkippedCount;
                    result.ErrorCount += fileResult.ErrorCount;

                    foreach (var error in fileResult.Errors)
                    {
                        result.Errors[error.Key] = error.Value;
                    }
                }

                catch (Exception ex)
                {
                    mLogger.LogError(ex, "Failed to process PDF: {FilePath}", filePath);
                    result.ErrorCount++;
                    result.Errors[Path.GetFileName(filePath)] = ex.Message;
                }
            }

            return result;
        }
        
        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to process PDFs");
            throw; // Re-throw the exception to be handled by the caller
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    /// Processes a specific PDF file.
    /// </summary>
    /// <param name="filePath">The absolute path to the PDF file.</param>
    /// <returns>A <see cref="EvProcessingResult"/> summarizing the outcome of processing the single file.</returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvProcessingResult> ProcessSpecificPdfAsync(String filePath)
    {
        var result   = new EvProcessingResult();
        var fileName = Path.GetFileName(filePath);
        
        try
        {
            mLogger.LogInformation("Processing PDF: {FileName}", fileName);

            // Calculate file hash
            var fileHash = mFileTrackingService.CalculateFileHash(filePath);

            // Check if already processed
            if (await mFileTrackingService.IsFileProcessedAsync(filePath, fileHash))
            {
                mLogger.LogInformation("PDF already processed: {FileName}", fileName);
                result.SkippedCount = 1;
                return result;
            }

            // Extract text
            var text = await mTextExtractor.ExtractTextAsync(filePath);
            
            if (String.IsNullOrWhiteSpace(text))
            {
                mLogger.LogWarning("No text extracted from PDF: {FileName}", fileName);
                result.ErrorCount     = 1;
                result.Errors[fileName] = "No text content found";
                return result;
            }

            // Chunk text
            var chunks = mTextExtractor.ChunkText(text, mSettings.EmbeddingSettings!.ChunkSize, mSettings.EmbeddingSettings!.ChunkOverlap);
            
            if (chunks == null || !chunks.Any()) // Added null check for safety
            {
                mLogger.LogWarning("No text chunks created for PDF: {FileName}", fileName);
                result.ErrorCount     = 1;
                result.Errors[fileName] = "No text chunks created";
                return result;
            }

            mLogger.LogInformation("Created {ChunkCount} chunks for PDF: {FileName}", chunks.Count, fileName);

            // Generate embeddings
            var embeddings = await mEmbeddingService.GenerateEmbeddingsAsync(chunks);

            // Create document records
            var documents = new List<EvDocumentRecord>();
            for (Int32 i = 0; i < chunks.Count; i++)
            {
                var documentId = Guid.NewGuid().ToString(); // Use pure GUID for Qdrant compatibility
                
                documents.Add(new EvDocumentRecord
                {
                    Id            = documentId,
                    FileName      = fileName,
                    Content       = chunks[i],
                    Vector        = embeddings[i],
                    ProcessedAt   = DateTime.UtcNow,
                    FileHash      = fileHash,
                    ChunkIndex    = i,
                    TotalChunks   = chunks.Count
                });
            }

            // Store in vector database
            await mVectorStoreService.StoreDocumentsAsync(documents);

            // Mark as processed
            await mFileTrackingService.AddProcessedFileAsync(filePath, fileHash);

            mLogger.LogInformation("Successfully processed PDF: {FileName} ({ChunkCount} chunks)", fileName, chunks.Count);
            result.ProcessedCount = 1;

            return result;
        }

        catch (Exception ex)
        {
            mLogger.LogError(ex, "Failed to process PDF: {FileName}", fileName);
            result.ErrorCount = 1;
            result.Errors[fileName] = ex.Message;
            return result;
        }
    }
}